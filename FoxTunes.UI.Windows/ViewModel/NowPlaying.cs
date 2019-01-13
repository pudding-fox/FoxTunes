using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace FoxTunes.ViewModel
{
    public class NowPlaying : ViewModelBase, IDisposable
    {
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
        }

        public Task SetDisplayScript(string value)
        {
            this._DisplayScript = value;
            return this.OnDisplayScriptChanged();
        }

        protected virtual async Task OnDisplayScriptChanged()
        {
            await this.Refresh();
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
            this.PlaylistManager = this.Core.Managers.Playlist;
            this.PlaylistManager.CurrentItemChanged += this.OnCurrentItemChanged;
            this.ScriptingRuntime = this.Core.Components.ScriptingRuntime;
            this.ScriptingContext = this.ScriptingRuntime.CreateContext();
            this.Configuration = this.Core.Components.Configuration;
            this.Configuration.GetElement<TextConfigurationElement>(
                MiniPlayerBehaviourConfiguration.SECTION,
                MiniPlayerBehaviourConfiguration.NOW_PLAYING_SCRIPT_ELEMENT
            ).ConnectValue<string>(async value => await this.SetDisplayScript(value));
            var task = this.Refresh();
            base.InitializeComponent(core);
        }

        protected virtual async void OnCurrentItemChanged(object sender, AsyncEventArgs e)
        {
            using (e.Defer())
            {
                await this.Refresh();
            }
        }

        protected virtual Task Refresh()
        {
            var runner = new PlaylistItemScriptRunner(this.ScriptingContext, this.PlaylistManager.CurrentItem, this.DisplayScript);
            runner.Prepare();
            var displayValue = runner.Run();
            return Windows.Invoke(() =>
            {
                this.CurrentItem = this.PlaylistManager.CurrentItem;
                this.DisplayValue = displayValue;
            });
        }

        protected override Freezable CreateInstanceCore()
        {
            return new NowPlaying();
        }

        protected override void OnDisposing()
        {
            if (this.PlaylistManager != null)
            {
                this.PlaylistManager.CurrentItemChanged -= this.OnCurrentItemChanged;
            }
            if (this.ScriptingContext != null)
            {
                this.ScriptingContext.Dispose();
            }
            base.OnDisposing();
        }
    }
}
