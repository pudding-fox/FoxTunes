using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace FoxTunes
{
    public abstract class ConfigurableUIComponentBase : UIComponentBase, IConfigurableComponent
    {
        public static readonly DependencyProperty ConfigurationProperty = DependencyProperty.Register(
           "Configuration",
           typeof(IConfiguration),
           typeof(ConfigurableUIComponentBase),
           new PropertyMetadata(null, new PropertyChangedCallback(OnConfigurationChanged))
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
            if (this.ConfigurationChanged != null)
            {
                this.ConfigurationChanged(this, EventArgs.Empty);
            }
        }

        public event EventHandler ConfigurationChanged;

        protected virtual async Task SaveConfiguration()
        {
            var configuration = default(IConfiguration);
            await Windows.Invoke(() => configuration = this.Configuration).ConfigureAwait(false);
            if (configuration != null)
            {
                configuration.Save();
            }
        }

        public abstract IEnumerable<ConfigurationSection> GetConfigurationSections();
    }
}
