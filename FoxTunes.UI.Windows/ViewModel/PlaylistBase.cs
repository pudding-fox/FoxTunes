using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes.ViewModel
{
    public abstract class PlaylistBase : ViewModelBase
    {
        public IPlaylistBrowser PlaylistBrowser { get; private set; }

        public IScriptingRuntime ScriptingRuntime { get; private set; }

        public IDatabaseFactory DatabaseFactory { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public IEnumerable<PlaylistItem> Items
        {
            get
            {
                if (this.PlaylistBrowser == null)
                {
                    return Enumerable.Empty<PlaylistItem>();
                }
                return this.PlaylistBrowser.GetItems();
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
                if (this.PlaylistBrowser != null)
                {
                    switch (this.PlaylistBrowser.State)
                    {
                        case PlaylistBrowserState.Loading:
                            return "Loading...";
                    }
                }
                if (this.PlaylistManager != null)
                {
                    if (!this.PlaylistManager.CanNavigate)
                    {
                        return "Add to playlist by dropping files here.";
                    }
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
                if (this.PlaylistBrowser != null)
                {
                    switch (this.PlaylistBrowser.State)
                    {
                        case PlaylistBrowserState.Loading:
                            return true;
                    }
                }
                if (this.PlaylistManager != null)
                {
                    if (!this.PlaylistManager.CanNavigate)
                    {
                        return true;
                    }
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
            this.DatabaseFactory = this.Core.Factories.Database;
            this.SignalEmitter = this.Core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
            this.PlaylistBrowser = this.Core.Components.PlaylistBrowser;
            this.PlaylistBrowser.StateChanged += this.OnStateChanged;
            this.ScriptingRuntime = this.Core.Components.ScriptingRuntime;
            this.PlaylistManager = this.Core.Managers.Playlist;
            this.PlaylistManager.CanNavigateChanged += this.OnCanNavigateChanged;
            this.PlaybackManager = this.Core.Managers.Playback;
            //TODO: Bad .Wait().
            this.RefreshStatus().Wait();
            base.InitializeComponent(core);
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
            var task = this.RefreshStatus();
        }

        protected virtual void OnCanNavigateChanged(object sender, EventArgs e)
        {
            var task = this.RefreshStatus();
        }

        protected virtual Task RefreshItems()
        {
            return Windows.Invoke(new Action(this.OnItemsChanged));
        }

        protected override void OnDisposing()
        {
            if (this.SignalEmitter != null)
            {
                this.SignalEmitter.Signal -= this.OnSignal;
            }
            base.OnDisposing();
        }
    }
}
