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
        public IPlaylistManager PlaylistManager { get; private set; }

        public IDatabaseFactory DatabaseFactory { get; private set; }

        public abstract Task<PlaylistItem> GetNext(bool navigate);

        public abstract Task<PlaylistItem> GetPrevious(bool navigate);

        public override void InitializeComponent(ICore core)
        {
            this.DatabaseFactory = core.Factories.Database;
            this.PlaylistManager = core.Managers.Playlist;
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
                        .WithAsyncEnumerator(enumerator => enumerator.FirstOrDefault());
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
                        .WithAsyncEnumerator(enumerator => enumerator.FirstOrDefault());
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
                        .WithAsyncEnumerator(enumerator => enumerator.FirstOrDefault());
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
                        .WithAsyncEnumerator(enumerator => enumerator.FirstOrDefault());
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
                playlistItem = await this.GetFirstPlaylistItem();
            }
            else
            {
                Logger.Write(this, LogLevel.Debug, "Current playlist item is sequence: {0}", this.PlaylistManager.CurrentItem.Sequence);
                playlistItem = await this.GetNextPlaylistItem(this.PlaylistManager.CurrentItem.Sequence);
                if (playlistItem == null)
                {
                    Logger.Write(this, LogLevel.Debug, "Sequence was too large, wrapping around to first item.");
                    playlistItem = await this.GetFirstPlaylistItem();
                }
            }
            if (playlistItem == null)
            {
                Logger.Write(this, LogLevel.Debug, "Playlist was empty.");
            }
            return playlistItem;
        }

        public override async Task<PlaylistItem> GetPrevious(bool navigate)
        {
            var playlistItem = default(PlaylistItem);
            if (this.PlaylistManager.CurrentItem == null)
            {
                Logger.Write(this, LogLevel.Debug, "Current playlist item is empty, assuming last item.");
                playlistItem = await this.GetLastPlaylistItem();
            }
            else
            {
                Logger.Write(this, LogLevel.Debug, "Previous playlist item is sequence: {0}", this.PlaylistManager.CurrentItem.Sequence);
                playlistItem = await this.GetPreviousPlaylistItem(this.PlaylistManager.CurrentItem.Sequence);
                if (playlistItem == null)
                {
                    Logger.Write(this, LogLevel.Debug, "Sequence was too small, wrapping around to last item.");
                    playlistItem = await this.GetLastPlaylistItem();
                }
            }
            if (playlistItem == null)
            {
                Logger.Write(this, LogLevel.Debug, "Playlist was empty.");
            }
            return playlistItem;
        }
    }

    public class ShufflePlaylistNavigationStrategy : PlaylistNavigationStrategy
    {
        public static readonly object SyncRoot = new object();

        public ShufflePlaylistNavigationStrategy()
        {
            this.Sequences = new List<int>();
        }

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
            using (var database = this.DatabaseFactory.Create())
            {
                var table = database.Tables.PlaylistItem;
                var query = database.QueryFactory.Build();
                var column = table.Column("Sequence");
                query.Output.AddColumn(column);
                query.Source.AddTable(table);
                using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                {
                    using (var reader = database.ExecuteReader(query, transaction))
                    {
                        using (var sequence = reader.GetAsyncEnumerator())
                        {
                            this.Sequences.Clear();
                            this.Position = 0;
                            while (await sequence.MoveNextAsync())
                            {
                                this.Sequences.Add(sequence.Current.Get<int>(column));
                            }
                            this.Sequences.Shuffle();
                        }
                    }
                }
            }
        }

        public override async Task<PlaylistItem> GetNext(bool navigate)
        {
            if (this.Sequences.Count == 0)
            {
                return default(PlaylistItem);
            }
            Monitor.Enter(SyncRoot);
            try
            {
                var playlistItem = await this.PlaylistManager.Get(this.Sequences[this.Position]);
                if (navigate)
                {
                    this.NavigateNext();
                }
                return playlistItem;
            }
            finally
            {
                Monitor.Exit(SyncRoot);
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
            if (this.Sequences.Count == 0)
            {
                return default(PlaylistItem);
            }
            Monitor.Enter(SyncRoot);
            try
            {
                if (navigate)
                {
                    this.NavigatePrevious();
                    this.NavigatePrevious();
                }
                var playlistItem = await this.PlaylistManager.Get(this.Sequences[this.Position]);
                if (navigate)
                {
                    this.NavigateNext();
                }
                return playlistItem;
            }
            finally
            {
                Monitor.Exit(SyncRoot);
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
