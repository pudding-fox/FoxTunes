using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class Playlists : ViewModelBase
    {
        public IPlaylistManager PlaylistManager { get; private set; }

        public IPlaylistBrowser PlaylistBrowser { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        private PlaylistCollection _Items { get; set; }

        public PlaylistCollection Items
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

        public Playlist SelectedItem
        {
            get
            {
                if (this.PlaylistManager == null)
                {
                    return Playlist.Empty;
                }
                return this.PlaylistManager.SelectedPlaylist;
            }
            set
            {
                if (this.PlaylistManager == null || value == null)
                {
                    return;
                }
                this.PlaylistManager.SelectedPlaylist = value;
            }
        }

        protected virtual void OnSelectedItemChanged()
        {
            if (this.SelectedItemChanged != null)
            {
                this.SelectedItemChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("SelectedItem");
        }

        public event EventHandler SelectedItemChanged;

        protected virtual Task RefreshItems()
        {
            var playlists = this.PlaylistBrowser.GetPlaylists();
            if (this.Items == null)
            {
                return Windows.Invoke(() => this.Items = new PlaylistCollection(playlists));
            }
            else
            {
                return Windows.Invoke(this.Items.Update(playlists));
            }
        }

        public virtual async Task Reload()
        {
            await this.RefreshItems().ConfigureAwait(false);
            await Windows.Invoke(() =>
            {
                this.OnSelectedItemChanged();
            }).ConfigureAwait(false);
        }

        public override void InitializeComponent(ICore core)
        {
            this.PlaylistManager = core.Managers.Playlist;
            this.PlaylistManager.SelectedPlaylistChanged += this.OnSelectedPlaylistChanged;
            this.PlaylistBrowser = core.Components.PlaylistBrowser;
            this.SignalEmitter = core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
            //TODO: Bad .Wait().
            this.Reload().Wait();
            base.InitializeComponent(core);
        }

        protected virtual Task OnSignal(object sender, ISignal signal)
        {
            switch (signal.Name)
            {
                case CommonSignals.PlaylistUpdated:
                    return this.Reload();
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        protected virtual void OnSelectedPlaylistChanged(object sender, EventArgs e)
        {
            var task = Windows.Invoke(new Action(this.OnSelectedItemChanged));
        }

        public ICommand AddPlaylistCommand
        {
            get
            {
                return CommandFactory.Instance.CreateCommand(this.AddPlaylist);
            }
        }

        public Task AddPlaylist()
        {
            return PlaylistsActionsBehaviour.Instance.AddPlaylist();
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
