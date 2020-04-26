using FoxDb;
using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.Database)]
    public class PlaylistBrowser : StandardComponent, IPlaylistBrowser
    {
        private PlaylistBrowserState _State { get; set; }

        public PlaylistBrowserState State
        {
            get
            {
                return this._State;
            }
            set
            {
                this._State = value;
                this.OnStateChanged();
            }
        }

        protected virtual void OnStateChanged()
        {
            if (this.StateChanged != null)
            {
                this.StateChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("State");
        }

        public event EventHandler StateChanged;

        public ICore Core { get; private set; }

        public IPlaylistCache PlaylistCache { get; private set; }

        public IDatabaseFactory DatabaseFactory { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public PlaylistNavigationStrategy NavigationStrategy { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.PlaylistCache = core.Components.PlaylistCache;
            this.DatabaseFactory = core.Factories.Database;
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<BooleanConfigurationElement>(
                PlaylistBehaviourConfiguration.SECTION,
                PlaylistBehaviourConfiguration.SHUFFLE_ELEMENT
            ).ConnectValue(value =>
            {
                if (value)
                {
                    this.NavigationStrategy = new ShufflePlaylistNavigationStrategy();
                }
                else
                {
                    this.NavigationStrategy = new StandardPlaylistNavigationStrategy();
                }
                this.NavigationStrategy.InitializeComponent(this.Core);
            });
            base.InitializeComponent(core);
        }

        public PlaylistItem[] GetItems()
        {
            return this.PlaylistCache.GetItems(this.GetItemsCore);
        }

        private IEnumerable<PlaylistItem> GetItemsCore()
        {
            this.State |= PlaylistBrowserState.Loading;
            try
            {
                using (var database = this.DatabaseFactory.Create())
                {
                    using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                    {
                        var sequence = database.AsQueryable<PlaylistItem>(transaction)
                            .OrderBy(playlistItem => playlistItem.Sequence);
                        foreach (var element in sequence)
                        {
                            yield return element;
                        }
                    }
                }
            }
            finally
            {
                this.State &= ~PlaylistBrowserState.Loading;
            }
        }

        public virtual async Task<PlaylistItem> Get(int sequence)
        {
            using (var database = this.DatabaseFactory.Create())
            {
                using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                {
                    return await database.AsQueryable<PlaylistItem>(transaction)
                        .Where(playlistItem => playlistItem.Sequence == sequence)
                        .Take(1)
                        .WithAsyncEnumerator(enumerator => enumerator.FirstOrDefault()).ConfigureAwait(false);
                }
            }
        }

        public async Task<int> GetInsertIndex()
        {
            var playlistItem = await this.NavigationStrategy.GetLastPlaylistItem().ConfigureAwait(false);
            if (playlistItem == null)
            {
                return 0;
            }
            else
            {
                return playlistItem.Sequence + 1;
            }
        }

        public Task<PlaylistItem> GetNext(bool navigate)
        {
            return this.NavigationStrategy.GetNext(navigate);
        }

        public Task<PlaylistItem> GetPrevious(bool navigate)
        {
            return this.NavigationStrategy.GetPrevious(navigate);
        }

        public virtual async Task<PlaylistItem> Get(string fileName)
        {
            using (var database = this.DatabaseFactory.Create())
            {
                using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                {
                    return await database.AsQueryable<PlaylistItem>(transaction)
                        .Where(playlistItem => playlistItem.FileName == fileName)
                        .Take(1)
                        .WithAsyncEnumerator(enumerator => enumerator.FirstOrDefault()).ConfigureAwait(false);
                }
            }
        }
    }
}
