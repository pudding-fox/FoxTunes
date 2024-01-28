using FoxDb;
using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class PlaylistSelector : ViewModelBase
    {
        const int TIMEOUT = 1000;

        public PlaylistSelector()
        {
            this.Debouncer = new AsyncDebouncer(TIMEOUT);
        }

        public AsyncDebouncer Debouncer { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public IDatabaseFactory DatabaseFactory { get; private set; }

        public IPlaylistBrowser PlaylistBrowser { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        private CollectionManager<Playlist> _Items { get; set; }

        public CollectionManager<Playlist> Items
        {
            get
            {
                return this._Items;
            }
            set
            {
                this._Items = value;
                this.OnItemsChanged();
            }
        }

        protected virtual void OnItemsChanged()
        {
            if (this.ItemsChanged != null)
            {
                this.ItemsChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Items");
        }

        public event EventHandler ItemsChanged;

        protected virtual Task RefreshItems()
        {
            var playlists = this.PlaylistBrowser.GetPlaylists();
            return Windows.Invoke(() => this.Items.ItemsSource = new ObservableCollection<Playlist>(playlists));
        }

        protected virtual Task RefreshSelectedItem()
        {
            var playlist = this.PlaylistManager.SelectedPlaylist;
            if (object.ReferenceEquals(this.Items.SelectedValue, playlist))
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            return Windows.Invoke(() => this.Items.SelectedValue = playlist);
        }

        public virtual async Task Refresh()
        {
            await this.RefreshItems().ConfigureAwait(false);
            await this.RefreshSelectedItem().ConfigureAwait(false);
        }

        protected override void InitializeComponent(ICore core)
        {
            this.PlaylistManager = core.Managers.Playlist;
            this.PlaylistManager.SelectedPlaylistChanged += this.OnSelectedPlaylistChanged;
            this.DatabaseFactory = core.Factories.Database;
            this.PlaylistBrowser = core.Components.PlaylistBrowser;
            this.SignalEmitter = core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
            this.Items = new CollectionManager<Playlist>()
            {
                ExchangeHandler = (item1, item2) =>
                {
                    var temp = item1.Sequence;
                    item1.Sequence = item2.Sequence;
                    item2.Sequence = temp;
                    this.Debouncer.Exec(this.Save);
                },
            };
            this.Items.SelectedValueChanged += this.OnSelectedValueChanged;
            this.Dispatch(this.Refresh);
            base.InitializeComponent(core);
        }

        protected virtual Task OnSignal(object sender, ISignal signal)
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

        protected virtual void OnSelectedPlaylistChanged(object sender, EventArgs e)
        {
            var task = this.RefreshSelectedItem();
        }

        protected virtual void OnSelectedValueChanged(object sender, EventArgs e)
        {
            var playlist = this.Items.SelectedValue;
            if (playlist == null)
            {
                return;
            }
            if (object.ReferenceEquals(this.PlaylistManager.SelectedPlaylist, playlist))
            {
                return;
            }
            this.PlaylistManager.SelectedPlaylist = playlist;
        }

        public Task AddPlaylist(LibraryHierarchyNode libraryHierarchyNode)
        {
            return PlaylistsActionsBehaviour.Instance.AddPlaylist(libraryHierarchyNode);
        }

        public Task AddPlaylist(IEnumerable<PlaylistItem> playlistItems)
        {
            return PlaylistsActionsBehaviour.Instance.AddPlaylist(playlistItems);
        }

        public Task AddPlaylist(IEnumerable<string> paths)
        {
            return PlaylistsActionsBehaviour.Instance.AddPlaylist(paths);
        }

        public ICommand RemovePlaylistCommand
        {
            get
            {
                return CommandFactory.Instance.CreateCommand(this.RemovePlaylist);
            }
        }

        public Task RemovePlaylist()
        {
            return PlaylistsActionsBehaviour.Instance.RemovePlaylist();
        }

        public ICommand AddPlaylistCommand
        {
            get
            {
                return CommandFactory.Instance.CreateCommand(this.AddPlaylist);
            }
        }

        protected virtual Task AddPlaylist()
        {
            return PlaylistsActionsBehaviour.Instance.AddPlaylist();
        }

        public Task Save()
        {
            return this.Save(this.Items.Updated);
        }

        public async Task Save(IEnumerable<Playlist> playlists)
        {
            using (var database = this.DatabaseFactory.Create())
            {
                using (var task = new SingletonReentrantTask(CancellationToken.None, ComponentSlots.Database, SingletonReentrantTask.PRIORITY_HIGH, async cancellationToken =>
                {
                    using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                    {
                        var set = database.Set<Playlist>(transaction);
                        await set.AddOrUpdateAsync(playlists).ConfigureAwait(false);
                        transaction.Commit();
                    }
                }))
                {
                    await task.Run().ConfigureAwait(false);
                }
            }
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistUpdated)).ConfigureAwait(false);
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Playlists();
        }

        protected override void OnDisposing()
        {
            if (this.PlaylistManager != null)
            {
                this.PlaylistManager.SelectedPlaylistChanged -= this.OnSelectedPlaylistChanged;
            }
            if (this.SignalEmitter != null)
            {
                this.SignalEmitter.Signal -= this.OnSignal;
            }
            base.OnDisposing();
        }
    }
}
