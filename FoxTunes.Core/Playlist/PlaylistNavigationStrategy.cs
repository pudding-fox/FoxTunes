using FoxDb;
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

        public abstract Task<PlaylistItem> GetNext(PlaylistItem playlistItem, bool navigate);

        public abstract Task<PlaylistItem> GetPrevious(PlaylistItem playlistItem, bool navigate);

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
        public override async Task<PlaylistItem> GetNext(PlaylistItem playlistItem, bool navigate)
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

        public override async Task<PlaylistItem> GetPrevious(PlaylistItem playlistItem, bool navigate)
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

        public int Position { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            base.InitializeComponent(core);
            this.SignalEmitter = core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
            //TODO: Bad .Wait().
            this.Refresh().Wait();
        }

        private Task OnSignal(object sender, ISignal signal)
        {
            switch (signal.Name)
            {
                case CommonSignals.PlaylistUpdated:
                    return this.Refresh();
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public async Task Refresh()
        {
#if NET40
            this.Semaphore.Wait();
#else
            await this.Semaphore.WaitAsync().ConfigureAwait(false);
#endif
            try
            {
                this.Position = 0;
                this.Sequences.Clear();
                using (var database = this.DatabaseFactory.Create())
                {
                    var table = database.Tables.PlaylistItem;
                    var query = database.QueryFactory.Build();
                    var column = table.GetColumn(ColumnConfig.By("Sequence"));
                    query.Output.AddColumn(column);
                    query.Source.AddTable(table);
                    using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                    {
                        using (var reader = database.ExecuteReader(query, transaction))
                        {
                            using (var sequence = reader.GetAsyncEnumerator())
                            {
                                while (await sequence.MoveNextAsync().ConfigureAwait(false))
                                {
                                    this.Sequences.Add(sequence.Current.Get<int>(column));
                                }
                                this.Sequences.Shuffle();
                            }
                        }
                    }
                }
            }
            finally
            {
                this.Semaphore.Release();
            }
        }

        public override async Task<PlaylistItem> GetNext(PlaylistItem playlistItem, bool navigate)
        {
#if NET40
            this.Semaphore.Wait();
#else
            await this.Semaphore.WaitAsync().ConfigureAwait(false);
#endif
            try
            {
                var position = this.Sequences.IndexOf(playlistItem.Sequence);
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
                var seqence = this.Sequences[position];
                var playlist = this.PlaylistBrowser.GetPlaylist(playlistItem);
                return await this.GetNext(playlist, seqence, navigate).ConfigureAwait(false);
            }
            finally
            {
                this.Semaphore.Release();
            }
        }

        protected virtual async Task<PlaylistItem> GetNext(Playlist playlist, int sequence, bool navigate)
        {
            var playlistItem = await this.PlaylistBrowser.GetItem(playlist, sequence).ConfigureAwait(false);
            if (navigate)
            {
                this.NavigateNext();
            }
            return playlistItem;
        }

        protected virtual void NavigateNext()
        {
            if (this.Position >= this.Sequences.Count - 1)
            {
                this.Position = 0;
            }
            else
            {
                this.Position++;
            }
        }

        public override async Task<PlaylistItem> GetPrevious(PlaylistItem playlistItem, bool navigate)
        {
#if NET40
            this.Semaphore.Wait();
#else
            await this.Semaphore.WaitAsync().ConfigureAwait(false);
#endif
            try
            {
                var position = this.Sequences.IndexOf(playlistItem.Sequence);
                if (position < 0)
                {
                    return null;
                }
                if (position > 0)
                {
                    position = this.Sequences.Count - 1;
                }
                else
                {
                    position--;
                }
                var seqence = this.Sequences[position];
                var playlist = this.PlaylistBrowser.GetPlaylist(playlistItem);
                return await this.GetPrevious(playlist, seqence, navigate).ConfigureAwait(false);
            }
            finally
            {
                this.Semaphore.Release();
            }
        }

        protected virtual async Task<PlaylistItem> GetPrevious(Playlist playlist, int sequence, bool navigate)
        {
            var playlistItem = await this.PlaylistBrowser.GetItem(playlist, sequence).ConfigureAwait(false);
            if (navigate)
            {
                this.NavigatePrevious();
            }
            return playlistItem;
        }

        protected virtual void NavigatePrevious()
        {
            if (this.Position > 0)
            {
                this.Position--;
            }
            else
            {
                this.Position = this.Sequences.Count - 1;
            }
        }
    }
}
