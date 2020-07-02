using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class ToolWindowBehaviour : StandardBehaviour, IInvocableComponent, IConfigurableComponent, IDisposable
    {
        const int TIMEOUT = 1000;

        public const string NEW = "AAAA";

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

        protected virtual async Task Load()
        {
            if (!string.IsNullOrEmpty(this.Element.Value))
            {
                try
                {
                    var configs = this.Configuration.LoadValue<ToolWindowConfiguration[]>(this.Element.Value);
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

        protected virtual async Task Load(ToolWindowConfiguration config)
        {
            try
            {
                var window = default(ToolWindow);
                await global::FoxTunes.Windows.Invoke(() =>
                {
                    window = new ToolWindow();
                    window.DataContext = this.Core;
                    window.Configuration = config;
                    window.Closed += this.OnClosed;
                    window.Show();
                }).ConfigureAwait(false);
                this.Windows[config] = window;
                config.PropertyChanged += this.OnConfigPropertyChanged;
            }
            catch (Exception e)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to load config: {0}", e.Message);
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
                this.Element.Value = this.Configuration.SaveValue(this.Windows.Keys.ToArray());
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

        protected virtual void OnConfigPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.Debouncer.Exec(this.Save);
        }

        public override void InitializeComponent(ICore core)
        {
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

        protected virtual void OnShuttingDown(object sender, EventArgs e)
        {
            var task = this.Shutdown();
        }

        public IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                yield return new InvocationComponent(InvocationComponent.CATEGORY_SETTINGS, NEW, "New Window");
            }
        }

        public Task InvokeAsync(IInvocationComponent component)
        {
            switch (component.Id)
            {
                case NEW:
                    return this.New();
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public Task New()
        {
            return this.Load(new ToolWindowConfiguration()
            {
                Title = "New Window",
                Width = 400,
                Height = 250
            });
        }

        public Task Shutdown()
        {
            return global::FoxTunes.Windows.Invoke(() =>
            {
                foreach (var window in this.Windows.Values)
                {
                    window.Closed -= this.OnClosed;
                    window.Close();
                }
                this.Windows.Clear();
            });
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
    }

    [Serializable]
    public class ToolWindowConfiguration : BaseComponent, ISerializable
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

        private string _Content { get; set; }

        public string Content
        {
            get
            {
                return this._Content;
            }
            set
            {
                this._Content = value;
                this.OnContentChanged();
            }
        }

        protected virtual void OnContentChanged()
        {
            if (this.ContentChanged != null)
            {
                this.ContentChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Content");
        }

        public event EventHandler ContentChanged;

        #region ISerializable

        protected ToolWindowConfiguration(SerializationInfo info, StreamingContext context)
        {
            this.Title = info.GetString(nameof(this.Title));
            this.Left = info.GetInt32(nameof(this.Left));
            this.Top = info.GetInt32(nameof(this.Top));
            this.Width = info.GetInt32(nameof(this.Width));
            this.Height = info.GetInt32(nameof(this.Height));
            this.Content = info.GetString(nameof(this.Content));
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(this.Title), this.Title);
            info.AddValue(nameof(this.Left), this.Left);
            info.AddValue(nameof(this.Top), this.Top);
            info.AddValue(nameof(this.Width), this.Width);
            info.AddValue(nameof(this.Height), this.Height);
            info.AddValue(nameof(this.Content), this.Content);
        }

        #endregion
    }
}
