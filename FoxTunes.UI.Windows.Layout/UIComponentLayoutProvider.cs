using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    [ComponentRelease(ReleaseType.Default)]
    public class UIComponentLayoutProvider : UILayoutProviderBase, IConfigurableComponent, IDisposable
    {
        public const string ID = "CCCC083C-3C19-4AAC-97C7-565AF8F83115";

        public override string Id
        {
            get
            {
                return ID;
            }
        }

        public override string Name
        {
            get
            {
                return Strings.UIComponentLayoutProvider_Name;
            }
        }

        public override string Description
        {
            get
            {
                return Strings.UIComponentLayoutProvider_Description;
            }
        }

        public LayoutDesignerBehaviour LayoutDesignerBehaviour { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public TextConfigurationElement Main { get; private set; }

        private UIComponentConfiguration _MainComponent { get; set; }

        public UIComponentConfiguration MainComponent
        {
            get
            {
                return this._MainComponent;
            }
            set
            {
                this._MainComponent = value;
                this.OnMainComponentChanged();
            }
        }

        protected virtual void OnMainComponentChanged()
        {
            if (this.MainComponentChanged != null)
            {
                this.MainComponentChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("MainComponent");
        }

        public event EventHandler MainComponentChanged;

        public override void InitializeComponent(ICore core)
        {
            global::FoxTunes.Windows.ShuttingDown += this.OnShuttingDown;
            this.LayoutDesignerBehaviour = ComponentRegistry.Instance.GetComponent<LayoutDesignerBehaviour>();
            this.LayoutDesignerBehaviour.IsDesigningChanged += this.OnIsDesigningChanged;
            this.Configuration = core.Components.Configuration;
            this.Main = this.Configuration.GetElement<TextConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                UIComponentLayoutProviderConfiguration.MAIN
            );
            this.Main.ConnectValue(value => this.MainComponent = this.LoadComponent(value));
            base.InitializeComponent(core);
        }

        protected virtual void OnShuttingDown(object sender, EventArgs e)
        {
            this.Save();
        }

        protected virtual void OnIsDesigningChanged(object sender, EventArgs e)
        {
            if (this.LayoutDesignerBehaviour.IsDesigning)
            {
                return;
            }
            this.Save();
        }

        protected virtual UIComponentConfiguration LoadComponent(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                try
                {
                    using (var stream = new MemoryStream(Encoding.Default.GetBytes(value)))
                    {
                        return Serializer.LoadComponent(stream);
                    }
                }
                catch (Exception e)
                {
                    Logger.Write(this, LogLevel.Warn, "Failed to load component: {0}", e.Message);
                }
            }
            return null;
        }

        public override bool IsComponentActive(string id)
        {
            foreach (var root in UIComponentRoot.Active)
            {
                if (this.IsComponentActive(root, id))
                {
                    return true;
                }
            }
            return false;
        }

        protected virtual bool IsComponentActive(UIComponentRoot root, string id)
        {
            var stack = new Stack<UIComponentConfiguration>();
            stack.Push(root.Component);
            while (stack.Count > 0)
            {
                var component = stack.Pop();
                if (component == null)
                {
                    continue;
                }
                if (string.Equals(component.Component, id, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                if (component.Children != null)
                {
                    foreach (var child in component.Children)
                    {
                        stack.Push(child);
                    }
                }
            }
            return false;
        }

        public override UIComponentBase Load(UILayoutTemplate template)
        {
            switch (template)
            {
                case UILayoutTemplate.Main:
                    var root = new UIComponentRoot();
                    root.SetBinding(
                        UIComponentPanel.ComponentProperty,
                        new Binding()
                        {
                            Source = this,
                            Path = new PropertyPath("MainComponent")
                        }
                    );
                    return root;
            }
            throw new NotImplementedException();
        }

        protected virtual void Save()
        {
            if (this.Configuration == null || this.Main == null)
            {
                return;
            }
            try
            {
                using (var stream = new MemoryStream())
                {
                    Serializer.Save(stream, this.MainComponent);
                    this.Main.Value = Encoding.Default.GetString(stream.ToArray());
                }
                this.Configuration.Save();
            }
            catch (Exception e)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to save config: {0}", e.Message);
            }
        }

        public void Reset()
        {
            this.MainComponent = this.LoadComponent(this.Main.Value);
            this.OnUpdated();
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return UIComponentLayoutProviderConfiguration.GetConfigurationSections();
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
            global::FoxTunes.Windows.ShuttingDown -= this.OnShuttingDown;
        }

        ~UIComponentLayoutProvider()
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
