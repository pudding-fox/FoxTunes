using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class WindowTitleBehaviour : StandardBehaviour, IConfigurableComponent, IDisposable
    {
        public IPlaylistManager PlaylistManager { get; private set; }

        public IScriptingRuntime ScriptingRuntime { get; private set; }

        public IScriptingContext ScriptingContext { get; private set; }

        public IConfiguration Configuration { get; private set; }

        private string _Script { get; set; }

        public string Script
        {
            get
            {
                return this._Script;
            }
        }

        public Task SetScript(string value)
        {
            this._Script = value;
            return this.OnScriptChanged();
        }

        protected virtual async Task OnScriptChanged()
        {
            await this.Refresh();
            if (this.ScriptChanged != null)
            {
                this.ScriptChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Script");
        }

        public event EventHandler ScriptChanged;

        public override void InitializeComponent(ICore core)
        {
            this.PlaylistManager = core.Managers.Playlist;
            this.PlaylistManager.CurrentItemChanged += this.OnCurrentItemChanged;
            this.ScriptingRuntime = core.Components.ScriptingRuntime;
            this.ScriptingContext = this.ScriptingRuntime.CreateContext();
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<TextConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                WindowTitleBehaviourConfiguration.WINDOW_TITLE_SCRIPT_ELEMENT
            ).ConnectValue(async value => await this.SetScript(value));
            Windows.MainWindowCreated += this.OnWindowCreated;
            Windows.SettingsWindowCreated += this.OnWindowCreated;
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

        protected virtual void OnWindowCreated(object sender, EventArgs e)
        {
            var task = this.Refresh();
        }

        protected virtual Task Refresh()
        {
            var runner = new PlaylistItemScriptRunner(this.ScriptingContext, this.PlaylistManager.CurrentItem, this.Script);
            runner.Prepare();
            var value = Convert.ToString(runner.Run());
            return Windows.Invoke(() => this.SetWindowTitle(value));
        }

        protected virtual void SetWindowTitle(string title)
        {
            if (Windows.IsMainWindowCreated)
            {
                Windows.MainWindow.Title = title;
            }
            if (Windows.IsMiniWindowCreated)
            {
                Windows.MiniWindow.Title = title;
            }
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return WindowTitleBehaviourConfiguration.GetConfigurationSections();
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
            if (this.PlaylistManager != null)
            {
                this.PlaylistManager.CurrentItemChanged -= this.OnCurrentItemChanged;
            }
            if (this.ScriptingContext != null)
            {
                this.ScriptingContext.Dispose();
            }
            Windows.MainWindowCreated -= this.OnWindowCreated;
            Windows.SettingsWindowCreated -= this.OnWindowCreated;
        }

        ~WindowTitleBehaviour()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            try
            {
                this.Dispose(true);
            }
            catch
            {
                //Nothing can be done, never throw on GC thread.
            }
        }
    }
}
