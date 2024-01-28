using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace FoxTunes
{
    public class UIComponentLayoutProvider : UILayoutProviderBase, IConfigurableComponent, IDisposable
    {
        public override string Id
        {
            get
            {
                return UIComponentLayoutProviderConfiguration.ID;
            }
        }

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
            this.Configuration = core.Components.Configuration;
            this.Main = this.Configuration.GetElement<TextConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                UIComponentLayoutProviderConfiguration.MAIN
            );
            this.Main.ConnectValue(value => this.MainComponent = this.LoadComponent(value));
            base.InitializeComponent(core);
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
            this.Save();
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
