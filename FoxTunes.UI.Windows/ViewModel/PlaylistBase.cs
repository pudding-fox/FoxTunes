using FoxTunes.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes.ViewModel
{
    public abstract class PlaylistBase : ViewModelBase
    {
        const string LOADING = "Loading...";

        const string UPDATING = "Updating...";

        const string EMPTY = "Add to playlist by dropping files here.";

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
                if (!this.PlaylistManager.CanNavigate)
                {
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
                switch (this.PlaylistBrowser.State)
                {
                    case PlaylistBrowserState.Loading:
                        return LOADING;
                }
                return null;
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
                if (this.PlaylistBrowser == null || this.PlaylistManager == null || this.Items == null)
                {
                    return true;
                }
                if (this.Items.Count > 0)
                {
                    return false;
                }
                switch (this.PlaylistBrowser.State)
                {
                    case PlaylistBrowserState.Loading:
                        return true;
                }
                if (!this.PlaylistManager.CanNavigate)
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

        protected virtual Task OnSignal(object sender, ISignal signal)
        {
            switch (signal.Name)
            {
                case CommonSignals.PlaylistUpdated:
                    return this.RefreshItems();
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        protected virtual void OnStateChanged(object sender, EventArgs e)
        {
            this.Dispatch(this.RefreshStatus);
        }

        protected virtual async Task RefreshItems()
        {
            var items = this.PlaylistBrowser.GetItems();
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
