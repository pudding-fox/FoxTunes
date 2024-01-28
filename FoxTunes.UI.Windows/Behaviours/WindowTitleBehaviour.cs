using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class WindowTitleBehaviour : StandardBehaviour, IConfigurableComponent, IDisposable
    {
        public IPlaybackManager PlaybackManager { get; private set; }

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
            await this.Refresh().ConfigureAwait(false);
            if (this.ScriptChanged != null)
            {
                this.ScriptChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Script");
        }

        public event EventHandler ScriptChanged;

        public override void InitializeComponent(ICore core)
        {
            this.PlaybackManager = core.Managers.Playback;
            this.PlaybackManager.CurrentStreamChanged += this.OnCurrentStreamChanged;
            this.ScriptingRuntime = core.Components.ScriptingRuntime;
            this.ScriptingContext = this.ScriptingRuntime.CreateContext();
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<TextConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                WindowTitleBehaviourConfiguration.WINDOW_TITLE_SCRIPT_ELEMENT
            ).ConnectValue(async value => await this.SetScript(value).ConfigureAwait(false));
            Windows.MainWindowCreated += this.OnWindowCreated;
            Windows.SettingsWindowCreated += this.OnWindowCreated;
            this.Dispatch(this.Refresh);
            base.InitializeComponent(core);
        }

        protected virtual void OnCurrentStreamChanged(object sender, EventArgs e)
        {
            //Critical: Don't block in this event handler, it causes a deadlock.
            this.Dispatch(this.Refresh);
        }

        protected virtual void OnWindowCreated(object sender, EventArgs e)
        {
            this.Dispatch(this.Refresh);
        }

        protected virtual Task Refresh()
        {
            var outputStream = this.PlaybackManager.CurrentStream;
            var runner = new PlaylistItemScriptRunner(
                this.ScriptingContext,
                outputStream != null ? outputStream.PlaylistItem : null,
                this.Script
            );
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
            if (this.PlaybackManager != null)
            {
                this.PlaybackManager.CurrentStreamChanged -= this.OnCurrentStreamChanged;
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
