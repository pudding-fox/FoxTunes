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

        public abstract Task<PlaylistItem> GetNext(bool navigate);

        public abstract Task<PlaylistItem> GetNext(PlaylistItem playlistItem);

        public abstract Task<PlaylistItem> GetPrevious(bool navigate);

        public abstract Task<PlaylistItem> GetPrevious(PlaylistItem playlistItem);

        public override void InitializeComponent(ICore core)
        {
            this.PlaylistBrowser = core.Components.PlaylistBrowser;
            this.PlaylistManager = core.Managers.Playlist;
            this.DatabaseFactory = core.Factories.Database;
            base.InitializeComponent(core);
        }

        public async Task<PlaylistItem> GetFirstPlaylistItem()
        {
            using (var database = this.DatabaseFactory.Create())
            {
                using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                {
                    return await database.AsQueryable<PlaylistItem>(transaction)
                        .OrderBy(playlistItem => playlistItem.Sequence)
                        .Take(1)
                        .WithAsyncEnumerator(enumerator => enumerator.FirstOrDefault()).ConfigureAwait(false);
                }
            }
        }

        public async Task<PlaylistItem> GetLastPlaylistItem()
        {
            using (var database = this.DatabaseFactory.Create())
            {
                using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                {
                    return await database.AsQueryable<PlaylistItem>(transaction)
                        .OrderByDescending(playlistItem => playlistItem.Sequence)
                        .Take(1)
                        .WithAsyncEnumerator(enumerator => enumerator.FirstOrDefault()).ConfigureAwait(false);
                }
            }
        }

        public async Task<PlaylistItem> GetNextPlaylistItem(int sequence)
        {
            using (var database = this.DatabaseFactory.Create())
            {
                using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                {
                    return await database.AsQueryable<PlaylistItem>(transaction)
                        .Where(playlistItem => playlistItem.Sequence > sequence)
                        .OrderBy(playlistItem => playlistItem.Sequence)
                        .Take(1)
                        .WithAsyncEnumerator(enumerator => enumerator.FirstOrDefault()).ConfigureAwait(false);
                }
            }
        }

        public async Task<PlaylistItem> GetPreviousPlaylistItem(int sequence)
        {
            using (var database = this.DatabaseFactory.Create())
            {
                using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                {
                    return await database.AsQueryable<PlaylistItem>(transaction)
                        .Where(playlistItem => playlistItem.Sequence < sequence)
                        .OrderByDescending(playlistItem => playlistItem.Sequence)
                        .Take(1)
                        .WithAsyncEnumerator(enumerator => enumerator.FirstOrDefault()).ConfigureAwait(false);
                }
            }
        }
    }

    public class StandardPlaylistNavigationStrategy : PlaylistNavigationStrategy
    {
        public override async Task<PlaylistItem> GetNext(bool navigate)
        {
            var playlistItem = default(PlaylistItem);
            if (this.PlaylistManager.CurrentItem == null)
            {
                Logger.Write(this, LogLevel.Debug, "Current playlist item is empty, assuming first item.");
                playlistItem = await this.GetFirstPlaylistItem().ConfigureAwait(false);
            }
            else
            {
                Logger.Write(this, LogLevel.Debug, "Current playlist item is sequence: {0}", this.PlaylistManager.CurrentItem.Sequence);
                playlistItem = await this.GetNextPlaylistItem(this.PlaylistManager.CurrentItem.Sequence).ConfigureAwait(false);
                if (playlistItem == null)
                {
                    Logger.Write(this, LogLevel.Debug, "Sequence was too large, wrapping around to first item.");
                    playlistItem = await this.GetFirstPlaylistItem().ConfigureAwait(false);
                }
            }
            if (playlistItem == null)
            {
                Logger.Write(this, LogLevel.Debug, "Playlist was empty.");
            }
            return playlistItem;
        }

        public override async Task<PlaylistItem> GetNext(PlaylistItem playlistItem)
        {
            playlistItem = await this.GetNextPlaylistItem(playlistItem.Sequence).ConfigureAwait(false);
            if (playlistItem == null)
            {
                Logger.Write(this, LogLevel.Debug, "Sequence was too large, wrapping around to first item.");
                playlistItem = await this.GetFirstPlaylistItem().ConfigureAwait(false);
            }
            return playlistItem;
        }

        public override async Task<PlaylistItem> GetPrevious(bool navigate)
        {
            var playlistItem = default(PlaylistItem);
            if (this.PlaylistManager.CurrentItem == null)
            {
                Logger.Write(this, LogLevel.Debug, "Current playlist item is empty, assuming last item.");
                playlistItem = await this.GetLastPlaylistItem().ConfigureAwait(false);
            }
            else
            {
                Logger.Write(this, LogLevel.Debug, "Previous playlist item is sequence: {0}", this.PlaylistManager.CurrentItem.Sequence);
                playlistItem = await this.GetPreviousPlaylistItem(this.PlaylistManager.CurrentItem.Sequence).ConfigureAwait(false);
                if (playlistItem == null)
                {
                    Logger.Write(this, LogLevel.Debug, "Sequence was too small, wrapping around to last item.");
                    playlistItem = await this.GetLastPlaylistItem().ConfigureAwait(false);
                }
            }
            if (playlistItem == null)
            {
                Logger.Write(this, LogLevel.Debug, "Playlist was empty.");
            }
            return playlistItem;
        }

        public override async Task<PlaylistItem> GetPrevious(PlaylistItem playlistItem)
        {
            playlistItem = await this.GetPreviousPlaylistItem(playlistItem.Sequence).ConfigureAwait(false);
            if (playlistItem == null)
            {
                Logger.Write(this, LogLevel.Debug, "Sequence was too small, wrapping around to last item.");
                playlistItem = await this.GetLastPlaylistItem().ConfigureAwait(false);
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
            await this.Semaphore.WaitAsync();
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

        public override async Task<PlaylistItem> GetNext(bool navigate)
        {
#if NET40
            this.Semaphore.Wait();
#else
            await this.Semaphore.WaitAsync();
#endif
            try
            {
                if (this.Sequences.Count == 0)
                {
                    return default(PlaylistItem);
                }
                var playlistItem = await this.PlaylistBrowser.Get(this.Sequences[this.Position]).ConfigureAwait(false);
                if (navigate)
                {
                    this.NavigateNext();
                }
                return playlistItem;
            }
            finally
            {
                this.Semaphore.Release();
            }
        }

        public override async Task<PlaylistItem> GetNext(PlaylistItem playlistItem)
        {
#if NET40
            this.Semaphore.Wait();
#else
            await this.Semaphore.WaitAsync();
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
                return await this.PlaylistBrowser.Get(this.Sequences[position]);
            }
            finally
            {
                this.Semaphore.Release();
            }
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

        public override async Task<PlaylistItem> GetPrevious(bool navigate)
        {
#if NET40
            this.Semaphore.Wait();
#else
            await this.Semaphore.WaitAsync();
#endif
            try
            {
                if (this.Sequences.Count == 0)
                {
                    return default(PlaylistItem);
                }
                if (navigate)
                {
                    this.NavigatePrevious();
                    this.NavigatePrevious();
                }
                var playlistItem = await this.PlaylistBrowser.Get(this.Sequences[this.Position]).ConfigureAwait(false);
                if (navigate)
                {
                    this.NavigateNext();
                }
                return playlistItem;
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
            await this.Semaphore.WaitAsync();
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
                return await this.PlaylistBrowser.Get(this.Sequences[position]);
            }
            finally
            {
                this.Semaphore.Release();
            }
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
