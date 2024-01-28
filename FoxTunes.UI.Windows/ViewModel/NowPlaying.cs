using FoxTunes.Interfaces;
using System;
using System.Windows;

namespace FoxTunes.ViewModel
{
    public class NowPlaying : ViewModelBase, IDisposable
    {
        public IForegroundTaskRunner ForegroundTaskRunner { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public IScriptingRuntime ScriptingRuntime { get; private set; }

        public IScriptingContext ScriptingContext { get; private set; }

        public IConfiguration Configuration { get; private set; }

        private PlaylistItem _CurrentItem { get; set; }

        public PlaylistItem CurrentItem
        {
            get
            {
                return this._CurrentItem;
            }
            private set
            {
                this._CurrentItem = value;
                this.OnCurrentItemChanged();
            }
        }

        protected virtual void OnCurrentItemChanged()
        {
            if (this.CurrentItemChanged != null)
            {
                this.CurrentItemChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("CurrentItem");
        }

        public event EventHandler CurrentItemChanged = delegate { };

        private string _DisplayScript { get; set; }

        public string DisplayScript
        {
            get
            {
                return this._DisplayScript;
            }
            set
            {
                this._DisplayScript = value;
                this.OnDisplayScriptChanged();
            }
        }

        protected virtual void OnDisplayScriptChanged()
        {
            this.Refresh();
            if (this.DisplayScriptChanged != null)
            {
                this.DisplayScriptChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("DisplayScript");
        }

        public event EventHandler DisplayScriptChanged = delegate { };

        private object _DisplayValue { get; set; }

        public object DisplayValue
        {
            get
            {
                return this._DisplayValue;
            }
            set
            {
                this._DisplayValue = value;
                this.OnDisplayValueChanged();
            }
        }

        protected virtual void OnDisplayValueChanged()
        {
            if (this.DisplayValueChanged != null)
            {
                this.DisplayValueChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("DisplayValue");
        }

        public event EventHandler DisplayValueChanged = delegate { };

        public override void InitializeComponent(ICore core)
        {
            this.ForegroundTaskRunner = this.Core.Components.ForegroundTaskRunner;
            this.PlaylistManager = this.Core.Managers.Playlist;
            this.PlaylistManager.CurrentItemChanged += (sender, e) => this.Refresh();
            this.ScriptingRuntime = this.Core.Components.ScriptingRuntime;
            this.ScriptingContext = this.ScriptingRuntime.CreateContext();
            this.Configuration = this.Core.Components.Configuration;
            this.Configuration.GetElement<TextConfigurationElement>(
                MiniPlayerBehaviourConfiguration.SECTION,
                MiniPlayerBehaviourConfiguration.NOW_PLAYING_SCRIPT_ELEMENT
            ).ConnectValue<string>(value => this.DisplayScript = value);
            this.Refresh();
            base.InitializeComponent(core);
        }

        protected virtual void Refresh()
        {
            //TODO: Bad awaited Task.
            this.ForegroundTaskRunner.Run(() =>
            {
                var runner = new PlaylistItemScriptRunner(this.ScriptingContext, this.PlaylistManager.CurrentItem, this.DisplayScript);
                runner.Prepare();
                this.CurrentItem = this.PlaylistManager.CurrentItem;
                this.DisplayValue = runner.Run();
            });
        }

        protected override Freezable CreateInstanceCore()
        {
            return new NowPlaying();
        }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.IsDisposed || !disposing)
            {
                return;
            }
            this.OnDisposing();
            this.IsDisposed = true;
        }

        protected virtual void OnDisposing()
        {
            if (this.ScriptingContext != null)
            {
                this.ScriptingContext.Dispose();
            }
        }

        ~NowPlaying()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            this.Dispose(true);
        }
    }
}
