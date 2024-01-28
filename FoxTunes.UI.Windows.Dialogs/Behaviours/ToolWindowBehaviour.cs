using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class ToolWindowBehaviour : StandardBehaviour, IInvocableComponent, IConfigurableComponent, IDisposable
    {
        const int TIMEOUT = 1000;

        public const string NEW = "AAAA";

        public const string MANAGE = "BBBB";

        static ToolWindowBehaviour()
        {
            Reset();
        }

        public ToolWindowBehaviour()
        {
            this.Debouncer = new Debouncer(TIMEOUT);
            this.Windows = new Dictionary<ToolWindowConfiguration, ToolWindow>();
        }

        public ICore Core { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public TextConfigurationElement Element { get; private set; }

        public Debouncer Debouncer { get; private set; }

        public IDictionary<ToolWindowConfiguration, ToolWindow> Windows { get; private set; }

        public bool IsLoaded { get; private set; }

        protected virtual async Task Load()
        {
            //Whatever happens we're ready.
            this.IsLoaded = true;
            if (!string.IsNullOrEmpty(this.Element.Value))
            {
                try
                {
                    var configs = Serializer.LoadValue<ToolWindowConfiguration[]>(this.Element.Value);
                    foreach (var config in configs)
                    {
                        await this.Load(config).ConfigureAwait(false);
                    }
                }
                catch (Exception e)
                {
                    Logger.Write(this, LogLevel.Warn, "Failed to load config: {0}", e.Message);
                }
            }
        }

        protected virtual async Task<ToolWindow> Load(ToolWindowConfiguration config)
        {
            try
            {
                var window = default(ToolWindow);
                await global::FoxTunes.Windows.Invoke(() =>
                {
                    window = new ToolWindow();
                    window.DataContext = this.Core;
                    window.Configuration = config;
                    window.ShowActivated = false;
                    //Don't set owner as it causes an "always on top" type of behaviour.
                    //We have an option for that.
                    //window.Owner = global::FoxTunes.Windows.ActiveWindow;
                    window.Closed += this.OnClosed;
                }).ConfigureAwait(false);
                this.Windows[config] = window;
                return window;
            }
            catch (Exception e)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to load config: {0}", e.Message);
                return null;
            }
        }

        protected virtual async Task Show()
        {
            foreach (var pair in this.Windows)
            {
                await this.Show(pair.Key, pair.Value).ConfigureAwait(false);
            }
        }

        protected virtual Task Show(ToolWindowConfiguration config, ToolWindow window)
        {
            var show = false;
            var main = global::FoxTunes.Windows.IsMainWindowCreated && global::FoxTunes.Windows.MainWindow.IsVisible;
            var mini = global::FoxTunes.Windows.IsMiniWindowCreated && global::FoxTunes.Windows.MiniWindow.IsVisible;
            if (config.ShowWithMainWindow && main)
            {
                show = true;
            }
            else if (config.ShowWithMiniWindow && mini)
            {
                show = true;
            }
            if (show)
            {
                return global::FoxTunes.Windows.Invoke(window.Show);
            }
            else
            {
                return global::FoxTunes.Windows.Invoke(window.Hide);
            }
        }

        protected virtual void Unload(ToolWindowConfiguration config)
        {
            if (!this.Windows.Remove(config))
            {
                return;
            }
            this.Debouncer.Exec(this.Save);
        }

        protected virtual void Save()
        {
            if (this.Configuration == null || this.Element == null)
            {
                return;
            }
            try
            {
                this.Element.Value = Serializer.SaveValue(this.Windows.Keys.ToArray());
                this.Configuration.Save();
            }
            catch (Exception e)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to save config: {0}", e.Message);
            }
        }

        protected virtual void OnClosed(object sender, EventArgs e)
        {
            var window = sender as ToolWindow;
            if (window == null)
            {
                return;
            }
            this.Unload(window.Configuration);
        }

        public override void InitializeComponent(ICore core)
        {
            global::FoxTunes.Windows.ActiveWindowChanged += this.OnActiveWindowChanged;
            global::FoxTunes.Windows.ShuttingDown += this.OnShuttingDown;
            this.Core = core;
            this.Configuration = core.Components.Configuration;
            this.Element = this.Configuration.GetElement<TextConfigurationElement>(
                ToolWindowBehaviourConfiguration.SECTION,
                ToolWindowBehaviourConfiguration.ELEMENT
            );
            var task = this.Load();
            base.InitializeComponent(core);
        }

        protected virtual void OnActiveWindowChanged(object sender, EventArgs e)
        {
            this.Dispatch(async () =>
            {
                if (!this.IsLoaded)
                {
                    await this.Load().ConfigureAwait(false);
                }
                await this.Show().ConfigureAwait(false);
            });
        }

        protected virtual void OnShuttingDown(object sender, EventArgs e)
        {
            var task = this.Shutdown();
        }

        public IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                if (!IsToolWindowManagerWindowCreated)
                {
                    yield return new InvocationComponent(InvocationComponent.CATEGORY_SETTINGS, NEW, "New Window");
                    yield return new InvocationComponent(InvocationComponent.CATEGORY_SETTINGS, MANAGE, "Manage Windows", attributes: InvocationComponent.ATTRIBUTE_SEPARATOR);
                }
            }
        }

        public Task InvokeAsync(IInvocationComponent component)
        {
            switch (component.Id)
            {
                case NEW:
                    return this.New();
                case MANAGE:
                    return this.Manage();
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public async Task New()
        {
            var config = new ToolWindowConfiguration()
            {
                Title = "New Window",
                Width = 400,
                Height = 250,
                ShowWithMainWindow = true,
                ShowWithMiniWindow = false
            };
            var window = await this.Load(config).ConfigureAwait(false);
            if (window == null)
            {
                return;
            }
            await this.Show(config, window).ConfigureAwait(false);
        }

        public Task Manage()
        {
            return global::FoxTunes.Windows.Invoke(() =>
            {
                if (!IsToolWindowManagerWindowCreated)
                {
                    ToolWindowManagerWindow.DataContext = this.Core;
                    ToolWindowManagerWindow.Show();
                }
            });
        }

        public Task Shutdown()
        {
            if (this.Windows.Count > 0)
            {
                this.Save();
            }
            return global::FoxTunes.Windows.Invoke(() =>
            {
                if (IsToolWindowManagerWindowCreated)
                {
                    ToolWindowManagerWindow.Close();
                }
                foreach (var window in this.Windows.Values)
                {
                    window.Closed -= this.OnClosed;
                    window.Close();
                }
                this.Windows.Clear();
            });
        }

        public async Task Update(IEnumerable<ToolWindowConfiguration> configs)
        {
            if (this.IsLoaded)
            {
                await this.Shutdown().ConfigureAwait(false);
            }
            foreach (var config in configs)
            {
                await this.Load(config).ConfigureAwait(false);
            }
            await this.Show().ConfigureAwait(false);
            this.Debouncer.Exec(this.Save);
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return ToolWindowBehaviourConfiguration.GetConfigurationSections();
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
            var task = this.Shutdown();
        }

        ~ToolWindowBehaviour()
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

        private static Lazy<Window> _ToolWindowManagerWindow { get; set; }

        public static bool IsToolWindowManagerWindowCreated
        {
            get
            {
                return _ToolWindowManagerWindow.IsValueCreated;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public static Window ToolWindowManagerWindow
        {
            get
            {
                var raiseEvent = !IsToolWindowManagerWindowCreated;
                try
                {
                    return _ToolWindowManagerWindow.Value;
                }
                finally
                {
                    if (IsToolWindowManagerWindowCreated && raiseEvent)
                    {
                        OnToolWindowManagerWindowCreated();
                    }
                }
            }
        }

        private static void OnToolWindowManagerWindowCreated()
        {
            ToolWindowManagerWindow.Closed += OnToolWindowManagerWindowClosed;
            if (ToolWindowManagerWindowCreated == null)
            {
                return;
            }
            ToolWindowManagerWindowCreated(ToolWindowManagerWindow, EventArgs.Empty);
        }

        public static event EventHandler ToolWindowManagerWindowCreated;

        private static void OnToolWindowManagerWindowClosed(object sender, EventArgs e)
        {
            if (IsToolWindowManagerWindowCreated)
            {
                ResourceDisposer.Dispose(ToolWindowManagerWindow);
            }
            _ToolWindowManagerWindow = new Lazy<Window>(() => new ToolWindowManagerWindow() { Owner = global::FoxTunes.Windows.ActiveWindow });
            if (ToolWindowManagerWindowClosed == null)
            {
                return;
            }
            ToolWindowManagerWindowClosed(typeof(ToolWindowManagerWindow), EventArgs.Empty);
        }

        public static event EventHandler ToolWindowManagerWindowClosed;

        private static void Reset()
        {
            _ToolWindowManagerWindow = new Lazy<Window>(() => new ToolWindowManagerWindow() { Owner = global::FoxTunes.Windows.ActiveWindow });
        }
    }

    public class ToolWindowConfiguration
    {
        public ToolWindowConfiguration()
        {

        }

        private string _Title { get; set; }

        public string Title
        {
            get
            {
                return this._Title;
            }
            set
            {
                this._Title = value;
                this.OnTitleChanged();
            }
        }

        protected virtual void OnTitleChanged()
        {
            if (this.TitleChanged != null)
            {
                this.TitleChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Title");
        }

        public event EventHandler TitleChanged;

        private int _Left { get; set; }

        public int Left
        {
            get
            {
                return this._Left;
            }
            set
            {
                this._Left = value;
                this.OnLeftChanged();
            }
        }

        protected virtual void OnLeftChanged()
        {
            if (this.LeftChanged != null)
            {
                this.LeftChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Left");
        }

        public event EventHandler LeftChanged;

        private int _Top { get; set; }

        public int Top
        {
            get
            {
                return this._Top;
            }
            set
            {
                this._Top = value;
                this.OnTopChanged();
            }
        }

        protected virtual void OnTopChanged()
        {
            if (this.TopChanged != null)
            {
                this.TopChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Top");
        }

        public event EventHandler TopChanged;

        private int _Width { get; set; }

        public int Width
        {
            get
            {
                return this._Width;
            }
            set
            {
                this._Width = value;
                this.OnWidthChanged();
            }
        }

        protected virtual void OnWidthChanged()
        {
            if (this.WidthChanged != null)
            {
                this.WidthChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Width");
        }

        public event EventHandler WidthChanged;

        private int _Height { get; set; }

        public int Height
        {
            get
            {
                return this._Height;
            }
            set
            {
                this._Height = value;
                this.OnHeightChanged();
            }
        }

        protected virtual void OnHeightChanged()
        {
            if (this.HeightChanged != null)
            {
                this.HeightChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Height");
        }

        public event EventHandler HeightChanged;

        private UIComponentConfiguration _Component { get; set; }

        public UIComponentConfiguration Component
        {
            get
            {
                return this._Component;
            }
            set
            {
                this._Component = value;
                this.OnComponentChanged();
            }
        }

        protected virtual void OnComponentChanged()
        {
            if (this.ComponentChanged != null)
            {
                this.ComponentChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Component");
        }

        public event EventHandler ComponentChanged;

        private bool _ShowWithMainWindow { get; set; }

        public bool ShowWithMainWindow
        {
            get
            {
                return this._ShowWithMainWindow;
            }
            set
            {
                this._ShowWithMainWindow = value;
                this.OnShowWithMainWindowChanged();
            }
        }

        protected virtual void OnShowWithMainWindowChanged()
        {
            if (this.ShowWithMainWindowChanged != null)
            {
                this.ShowWithMainWindowChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("ShowWithMainWindow");
        }

        public event EventHandler ShowWithMainWindowChanged;

        private bool _ShowWithMiniWindow { get; set; }

        public bool ShowWithMiniWindow
        {
            get
            {
                return this._ShowWithMiniWindow;
            }
            set
            {
                this._ShowWithMiniWindow = value;
                this.OnShowWithMiniWindowChanged();
            }
        }

        protected virtual void OnShowWithMiniWindowChanged()
        {
            if (this.ShowWithMiniWindowChanged != null)
            {
                this.ShowWithMiniWindowChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("ShowWithMiniWindow");
        }

        public event EventHandler ShowWithMiniWindowChanged;

        private bool _AlwaysOnTop { get; set; }

        public bool AlwaysOnTop
        {
            get
            {
                return this._AlwaysOnTop;
            }
            set
            {
                this._AlwaysOnTop = value;
                this.OnAlwaysOnTopChanged();
            }
        }

        protected virtual void OnAlwaysOnTopChanged()
        {
            if (this.AlwaysOnTopChanged != null)
            {
                this.AlwaysOnTopChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("AlwaysOnTop");
        }

        public event EventHandler AlwaysOnTopChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged == null)
            {
                return;
            }
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
