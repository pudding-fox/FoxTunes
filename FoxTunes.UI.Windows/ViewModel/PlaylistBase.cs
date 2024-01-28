using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.TextFormatting;

namespace FoxTunes.ViewModel
{
    public abstract class PlaylistBase : ViewModelBase
    {
        protected virtual string LOADING
        {
            get
            {
                return "Loading...";
            }
        }

        protected virtual string UPDATING
        {
            get
            {
                return "Updating...";
            }
        }

        protected virtual string EMPTY
        {
            get
            {
                return "Add to playlist by dropping files here.";
            }
        }

        public IPlaylistBrowser PlaylistBrowser { get; private set; }

        public IScriptingRuntime ScriptingRuntime { get; private set; }

        public IDatabaseFactory DatabaseFactory { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        private PlaylistItemCollection _Items { get; set; }

        public PlaylistItemCollection Items
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

        public virtual string StatusMessage
        {
            get
            {
                if (this.PlaylistBrowser == null || this.PlaylistManager == null || this.Items == null)
                {
                    return LOADING;
                }
                if (this.Items.Count > 0)
                {
                    return null;
                }
                switch (this.PlaylistBrowser.State)
                {
                    case PlaylistBrowserState.Loading:
                        return LOADING;
                }
                var isUpdating = global::FoxTunes.BackgroundTask.Active
                        .OfType<PlaylistTaskBase>()
                        .Any();
                if (isUpdating)
                {
                    return UPDATING;
                }
                else
                {
                    return EMPTY;
                }
            }
        }

        protected virtual void OnStatusMessageChanged()
        {
            if (this.StatusMessageChanged != null)
            {
                this.StatusMessageChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("StatusMessage");
        }

        public event EventHandler StatusMessageChanged;

        public virtual bool HasStatusMessage
        {
            get
            {
                if (this.Items == null || this.Items.Count == 0)
                {
                    return true;
                }
                return false;
            }
        }

        protected virtual void OnHasStatusMessageChanged()
        {
            if (this.HasStatusMessageChanged != null)
            {
                this.HasStatusMessageChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("HasStatusMessage");
        }

        public event EventHandler HasStatusMessageChanged;

        protected virtual Task RefreshStatus()
        {
            return Windows.Invoke(() =>
            {
                this.OnStatusMessageChanged();
                this.OnHasStatusMessageChanged();
            });
        }

        protected abstract Task<Playlist> GetPlaylist();

        public override void InitializeComponent(ICore core)
        {
            global::FoxTunes.BackgroundTask.ActiveChanged += this.OnActiveChanged;
            this.DatabaseFactory = this.Core.Factories.Database;
            this.SignalEmitter = this.Core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
            this.PlaylistBrowser = this.Core.Components.PlaylistBrowser;
            this.PlaylistBrowser.StateChanged += this.OnStateChanged;
            this.ScriptingRuntime = this.Core.Components.ScriptingRuntime;
            this.PlaylistManager = this.Core.Managers.Playlist;
            this.PlaybackManager = this.Core.Managers.Playback;
            //TODO: Bad .Wait().
            this.RefreshStatus().Wait();
            base.InitializeComponent(core);
        }

        protected virtual void OnActiveChanged(object sender, EventArgs e)
        {
            this.Dispatch(this.RefreshStatus);
        }

        protected virtual async Task OnSignal(object sender, ISignal signal)
        {
            switch (signal.Name)
            {
                case CommonSignals.PlaylistUpdated:
                    var playlists = signal.State as IEnumerable<Playlist>;
                    if (playlists != null)
                    {
                        var playlist = await this.GetPlaylist().ConfigureAwait(false);
                        if (playlists.Contains(playlist))
                        {
                            await this.RefreshItems().ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        await this.RefreshItems().ConfigureAwait(false);
                    }
                    break;
            }
        }

        protected virtual void OnStateChanged(object sender, EventArgs e)
        {
            this.Dispatch(this.RefreshStatus);
        }

        protected virtual async Task RefreshItems()
        {
            var playlist = await this.GetPlaylist().ConfigureAwait(false);
            if (playlist == null)
            {
                return;
            }
            await this.RefreshItems(playlist).ConfigureAwait(false);
        }

        protected virtual async Task RefreshItems(Playlist playlist)
        {
            var items = this.PlaylistBrowser.GetItems(playlist);
            if (this.Items == null)
            {
                await Windows.Invoke(() => this.Items = new PlaylistItemCollection(items)).ConfigureAwait(false);
            }
            else
            {
                await Windows.Invoke(this.Items.Update(items)).ConfigureAwait(false);
            }
            await this.RefreshStatus().ConfigureAwait(false);
        }

        protected override void OnDisposing()
        {
            global::FoxTunes.BackgroundTask.ActiveChanged -= this.OnActiveChanged;
            if (this.PlaylistBrowser != null)
            {
                this.PlaylistBrowser.StateChanged += this.OnStateChanged;
            }
            if (this.SignalEmitter != null)
            {
                this.SignalEmitter.Signal -= this.OnSignal;
            }
            base.OnDisposing();
        }
    }
}
