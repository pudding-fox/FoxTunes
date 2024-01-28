using FoxTunes.Interfaces;
using FoxTunes.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FoxTunes
{
    public abstract class ConfigurableUIComponentBase : UIComponentBase, IConfigurableComponent, IInvocableComponent
    {
        public const string SETTINGS = "ZZZZ";

        public static readonly DependencyProperty ConfigurationProperty = DependencyProperty.RegisterAttached(
           "Configuration",
           typeof(IConfiguration),
           typeof(ConfigurableUIComponentBase),
           new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, OnConfigurationChanged)
       );

        public static IConfiguration GetConfiguration(ConfigurableUIComponentBase source)
        {
            return (IConfiguration)source.GetValue(ConfigurationProperty);
        }

        public static void SetConfiguration(ConfigurableUIComponentBase source, IConfiguration value)
        {
            source.SetValue(ConfigurationProperty, value);
        }

        private static void OnConfigurationChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var componentBase = sender as ConfigurableUIComponentBase;
            if (componentBase == null)
            {
                return;
            }
            componentBase.OnConfigurationChanged();
        }

        public ConfigurableUIComponentBase()
        {
            var menu = new Menu();
            menu.Components = new ObservableCollection<IInvocableComponent>()
            {
                this
            };
            this.ContextMenu = menu;
        }

        public IUserInterface UserInterface { get; private set; }

        protected override void InitializeComponent(ICore core)
        {
            this.UserInterface = core.Components.UserInterface;
            base.InitializeComponent(core);
        }

        private IConfiguration _Configuration { get; set; }

        protected IConfiguration GetConfiguration()
        {
            return this._Configuration;
        }

        public IConfiguration Configuration
        {
            get
            {
                return GetConfiguration(this);
            }
            set
            {
                SetConfiguration(this, value);
            }
        }

        protected virtual void OnConfigurationChanged()
        {
            this._Configuration = this.Configuration;
            if (this.Resources != null)
            {
                foreach (var configurationTarget in this.Resources.Values.OfType<IConfigurationTarget>())
                {
                    configurationTarget.Configuration = this._Configuration;
                }
            }
            if (this.ConfigurationChanged != null)
            {
                this.ConfigurationChanged(this, EventArgs.Empty);
            }
        }

        public event EventHandler ConfigurationChanged;

        public abstract IEnumerable<string> InvocationCategories { get; }

        public virtual IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                var invocationCategory = this.InvocationCategories.FirstOrDefault();
                if (!string.IsNullOrEmpty(invocationCategory))
                {
                    yield return new InvocationComponent(
                        invocationCategory,
                        SETTINGS,
                        StringResources.General_Settings,
                        attributes: InvocationComponent.ATTRIBUTE_SEPARATOR
                    );
                }
            }
        }

        public virtual Task InvokeAsync(IInvocationComponent component)
        {
            if (string.Equals(component.Id, SETTINGS, StringComparison.OrdinalIgnoreCase))
            {
                return this.ShowSettings();
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        protected abstract Task ShowSettings();

        protected virtual void SaveSettings()
        {
            this.GetConfiguration().Save();
        }

        public abstract IEnumerable<ConfigurationSection> GetConfigurationSections();
    }
}
