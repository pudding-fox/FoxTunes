using FoxTunes.Interfaces;
using System;
using System.Windows;
using System.Windows.Controls;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for ComponentSettingsDialog.xaml
    /// </summary>
    public partial class ComponentSettingsDialog : UserControl
    {
        public static readonly DependencyProperty ConfigurationProperty = DependencyProperty.Register(
           "Configuration",
           typeof(IConfiguration),
           typeof(ComponentSettingsDialog),
           new PropertyMetadata(null, new PropertyChangedCallback(OnConfigurationChanged))
       );

        public static IConfiguration GetConfiguration(ComponentSettingsDialog source)
        {
            return (IConfiguration)source.GetValue(ConfigurationProperty);
        }

        public static void SetConfiguration(ComponentSettingsDialog source, IConfiguration value)
        {
            source.SetValue(ConfigurationProperty, value);
        }

        private static void OnConfigurationChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var componentSettingsDialog = sender as ComponentSettingsDialog;
            if (componentSettingsDialog == null)
            {
                return;
            }
            componentSettingsDialog.OnConfigurationChanged();
        }

        public static readonly DependencyProperty SectionsProperty = DependencyProperty.Register(
            "Sections",
            typeof(StringCollection),
            typeof(ComponentSettingsDialog),
            new PropertyMetadata(default(StringCollection))
        );

        public static StringCollection GetSections(ComponentSettingsDialog source)
        {
            return (StringCollection)source.GetValue(SectionsProperty);
        }

        public static void SetSections(ComponentSettingsDialog source, StringCollection value)
        {
            source.SetValue(SectionsProperty, value);
        }

        public ComponentSettingsDialog()
        {
            this.InitializeComponent();
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

        public StringCollection Sections
        {
            get
            {
                return this.GetValue(SectionsProperty) as StringCollection;
            }
            set
            {
                this.SetValue(SectionsProperty, value);
            }
        }

        protected virtual void OnSectionsChanged()
        {
            if (this.SectionsChanged != null)
            {
                this.SectionsChanged(this, EventArgs.Empty);
            }
        }

        public event EventHandler SectionsChanged;

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            if (this.Configuration == null)
            {
                this.Configuration = Core.Instance.Components.Configuration;
            }
            this.Refresh();
            base.OnVisualParentChanged(oldParent);
        }

        public void Refresh()
        {
            var viewModel = this.FindResource<global::FoxTunes.ViewModel.ComponentSettings>("ViewModel");
            if (viewModel == null)
            {
                return;
            }
            viewModel.Refresh();
        }
    }
}
