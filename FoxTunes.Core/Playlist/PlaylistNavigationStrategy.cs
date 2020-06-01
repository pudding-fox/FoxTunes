using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FoxTunes
{
    public abstract class PlaylistNavigationStrategy : BaseComponent
    {
        public IPlaylistBrowser PlaylistBrowser { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public IDatabaseFactory DatabaseFactory { get; private set; }

        protected virtual Playlist GetPlaylist(PlaylistItem playlistItem)
        {
            if (playlistItem == null)
            {
                return this.PlaylistManager.CurrentPlaylist ?? this.PlaylistManager.SelectedPlaylist;
            }
            return this.PlaylistBrowser.GetPlaylist(playlistItem);
        }

        public abstract Task<PlaylistItem> GetNext(PlaylistItem playlistItem);

        public abstract Task<PlaylistItem> GetPrevious(PlaylistItem playlistItem);

        public override void InitializeComponent(ICore core)
        {
            this.PlaylistBrowser = core.Components.PlaylistBrowser;
            this.PlaylistManager = core.Managers.Playlist;
            this.DatabaseFactory = core.Factories.Database;
            base.InitializeComponent(core);
        }

        public async Task<PlaylistItem> GetFirstPlaylistItem(Playlist playlist)
        {
            using (var database = this.DatabaseFactory.Create())
            {
                using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                {
                    return await database.AsQueryable<PlaylistItem>(transaction)
                        .Where(playlistItem => playlistItem.Playlist_Id == playlist.Id)
                        .OrderBy(playlistItem => playlistItem.Sequence)
                        .Take(1)
                        .WithAsyncEnumerator(enumerator => enumerator.FirstOrDefault()).ConfigureAwait(false);
                }
            }
        }

        public async Task<PlaylistItem> GetLastPlaylistItem(Playlist playlist)
        {
            using (var database = this.DatabaseFactory.Create())
            {
                using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                {
                    return await database.AsQueryable<PlaylistItem>(transaction)
                        .Where(playlistItem => playlistItem.Playlist_Id == playlist.Id)
                        .OrderByDescending(playlistItem => playlistItem.Sequence)
                        .Take(1)
                        .WithAsyncEnumerator(enumerator => enumerator.FirstOrDefault()).ConfigureAwait(false);
                }
            }
        }

        public async Task<PlaylistItem> GetNextPlaylistItem(Playlist playlist, int sequence)
        {
            using (var database = this.DatabaseFactory.Create())
            {
                using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                {
                    return await database.AsQueryable<PlaylistItem>(transaction)
                        .Where(playlistItem => playlistItem.Playlist_Id == playlist.Id && playlistItem.Sequence > sequence)
                        .OrderBy(playlistItem => playlistItem.Sequence)
                        .Take(1)
                        .WithAsyncEnumerator(enumerator => enumerator.FirstOrDefault()).ConfigureAwait(false);
                }
            }
        }

        public async Task<PlaylistItem> GetPreviousPlaylistItem(Playlist playlist, int sequence)
        {
            using (var database = this.DatabaseFactory.Create())
            {
                using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                {
                    return await database.AsQueryable<PlaylistItem>(transaction)
                        .Where(playlistItem => playlistItem.Playlist_Id == playlist.Id && playlistItem.Sequence < sequence)
                        .OrderByDescending(playlistItem => playlistItem.Sequence)
                        .Take(1)
                        .WithAsyncEnumerator(enumerator => enumerator.FirstOrDefault()).ConfigureAwait(false);
                }
            }
        }
    }

    public class StandardPlaylistNavigationStrategy : PlaylistNavigationStrategy
    {
        public override async Task<PlaylistItem> GetNext(PlaylistItem playlistItem)
        {
            var playlist = this.GetPlaylist(playlistItem);
            if (playlistItem == null)
            {
                return await this.GetFirstPlaylistItem(playlist).ConfigureAwait(false);
            }
            playlistItem = await this.GetNextPlaylistItem(playlist, playlistItem.Sequence).ConfigureAwait(false);
            if (playlistItem == null)
            {
                playlistItem = await this.GetFirstPlaylistItem(playlist).ConfigureAwait(false);
            }
            return playlistItem;
        }

        public override async Task<PlaylistItem> GetPrevious(PlaylistItem playlistItem)
        {
            var playlist = this.GetPlaylist(playlistItem);
            if (playlistItem == null)
            {
                return await this.GetLastPlaylistItem(playlist).ConfigureAwait(false);
            }
            playlistItem = await this.GetPreviousPlaylistItem(playlist, playlistItem.Sequence).ConfigureAwait(false);
            if (playlistItem == null)
            {
                playlistItem = await this.GetLastPlaylistItem(playlist).ConfigureAwait(false);
            }
            return playlistItem;
        }
    }

    public class ShufflePlaylistNavigationStrategy : PlaylistNavigationStrategy
    {
        public ShufflePlaylistNavigationStrategy()
        {
            this.Semaphore = new SemaphoreSlim(1, 1);
            this.Sequences = new List<int>();
        }

        public SemaphoreSlim Semaphore { get; private set; }

        public IList<int> Sequences { get; private set; }

        public Playlist Playlist { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            base.InitializeComponent(core);
            this.SignalEmitter = core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
        }

        private Task OnSignal(object sender, ISignal signal)
        {
            switch (signal.Name)
            {
                case CommonSignals.PlaylistUpdated:
                    if (this.Playlist != null)
                    {
                        var playlists = signal.State as IEnumerable<Playlist>;
                        if (playlists != null && playlists.Contains(this.Playlist))
                        {
                            return this.Refresh();
                        }
                    }
                    break;
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public Task Refresh()
        {
            if (this.Playlist != null)
            {
                return this.Refresh(this.Playlist);
            }
            else
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
        }

        protected virtual async Task Refresh(Playlist playlist)
        {
            this.Playlist = playlist;
            this.Sequences.Clear();
            using (var database = this.DatabaseFactory.Create())
            {
                using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                {
                    var table = database.Tables.PlaylistItem;
                    var column = table.GetColumn(ColumnConfig.By("Sequence"));
                    var builder = database.QueryFactory.Build();
                    builder.Output.AddColumn(column);
                    builder.Source.AddTable(table);
                    builder.Filter.AddColumn(table.GetColumn(ColumnConfig.By("Playlist_Id")));
                    using (var reader = database.ExecuteReader(builder, (parameters, phase) =>
                    {
                        switch (phase)
                        {
                            case DatabaseParameterPhase.Fetch:
                                parameters["playlistId"] = playlist.Id;
                                break;
                        }
                    }, transaction))
                    {
                        using (var sequence = reader.GetAsyncEnumerator())
                        {
                            while (await sequence.MoveNextAsync().ConfigureAwait(false))
                            {
                                this.Sequences.Add(sequence.Current.Get<int>(column));
                            }
                        }
                    }
                }
            }
            this.Sequences.Shuffle();
        }

        public override async Task<PlaylistItem> GetNext(PlaylistItem playlistItem)
        {
#if NET40
            this.Semaphore.Wait();
#else
            await this.Semaphore.WaitAsync().ConfigureAwait(false);
#endif
            try
            {
                var playlist = this.GetPlaylist(playlistItem);
                if (this.Playlist == null || this.Playlist != playlist)
                {
                    await this.Refresh(playlist).ConfigureAwait(false);
                }
                if (this.Sequences.Count == 0)
                {
                    return default(PlaylistItem);
                }
                var position = default(int);
                if (playlistItem != null)
                {
                    position = this.Sequences.IndexOf(playlistItem.Sequence);
                }
                else
                {
                    position = 0;
                }
                if (position < 0)
                {
                    return null;
                }
                if (position >= this.Sequences.Count - 1)
                {
                    position = 0;
                }
                else
                {
                    position++;
                }
                var sequence = this.Sequences[position];
                return await this.PlaylistBrowser.GetItem(playlist, sequence).ConfigureAwait(false);
            }
            finally
            {
                this.Semaphore.Release();
            }
        }

        public override async Task<PlaylistItem> GetPrevious(PlaylistItem playlistItem)
        {
#if NET40
            this.Semaphore.Wait();
#else
            await this.Semaphore.WaitAsync().ConfigureAwait(false);
#endif
            try
            {
                var playlist = this.GetPlaylist(playlistItem);
                if (this.Playlist == null || this.Playlist != playlist)
                {
                    await this.Refresh(playlist).ConfigureAwait(false);
                }
                if (this.Sequences.Count == 0)
                {
                    return default(PlaylistItem);
                }
                var position = default(int);
                if (playlistItem != null)
                {
                    position = this.Sequences.IndexOf(playlistItem.Sequence);
                }
                else
                {
                    position = 0;
                }
                if (position < 0)
                {
                    return null;
                }
                if (position <= 0)
                {
                    position = this.Sequences.Count - 1;
                }
                else
                {
                    position--;
                }
                var sequence = this.Sequences[position];
                return await this.PlaylistBrowser.GetItem(playlist, sequence).ConfigureAwait(false);
            }
            finally
            {
                this.Semaphore.Release();
            }
        }
    }
}
