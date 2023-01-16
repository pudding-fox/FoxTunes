using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class LayoutTree : ViewModelBase
    {
        private UIComponentConfiguration _SelectedConfiguration { get; set; }

        public UIComponentConfiguration SelectedConfiguration
        {
            get
            {
                return this._SelectedConfiguration;
            }
            set
            {
                if (object.ReferenceEquals(this.SelectedConfiguration, value))
                {
                    return;
                }
                this._SelectedConfiguration = value;
                this.OnSelectedConfigurationChanged();
            }
        }

        protected virtual void OnSelectedConfigurationChanged()
        {
            if (this.SelectedConfigurationChanged != null)
            {
                this.SelectedConfigurationChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("SelectedConfiguration");
        }

        public event EventHandler SelectedConfigurationChanged;

        private ObservableCollection<UIComponentConfiguration> _Configurations { get; set; }

        public ObservableCollection<UIComponentConfiguration> Configurations
        {
            get
            {
                return this._Configurations;
            }
            set
            {
                if (object.ReferenceEquals(this.Configurations, value))
                {
                    return;
                }
                this._Configurations = value;
                this.OnConfigurationsChanged();
            }
        }

        protected virtual void OnConfigurationsChanged()
        {
            if (this.ConfigurationsChanged != null)
            {
                this.ConfigurationsChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Configurations");
        }

        public event EventHandler ConfigurationsChanged;

        protected override void InitializeComponent(ICore core)
        {
            UIComponentRoot.ActiveChanged += this.OnActiveChanged;
            var task = this.Refresh();
            base.InitializeComponent(core);
        }

        protected virtual void OnActiveChanged(object sender, EventArgs e)
        {
            var task = this.Refresh();
        }

        protected virtual Task Refresh()
        {
            return Windows.Invoke(() =>
            {
                var configurations = new List<UIComponentConfiguration>();
                foreach (var root in UIComponentRoot.Active)
                {
                    root.AddHandler(
                        UIComponentContainer.ConfigurationChangedEvent,
                        new RoutedPropertyChangedEventHandler<UIComponentConfiguration>(this.OnConfigurationChanged)
                    );
                    configurations.Add(root.Configuration);
                }
                this.Configurations = new ObservableCollection<UIComponentConfiguration>(configurations);
            });
        }

        protected virtual void OnConfigurationChanged(object sender, RoutedPropertyChangedEventArgs<UIComponentConfiguration> e)
        {
            if (this.Configurations == null)
            {
                return;
            }
            for (var a = 0; a < this.Configurations.Count; a++)
            {
                if (!object.ReferenceEquals(this.Configurations[a], e.OldValue))
                {
                    continue;
                }
                this.Configurations[a] = e.NewValue;
                break;
            }
        }

        public ICommand ShowDesignerOverlayCommand
        {
            get
            {
                return CommandFactory.Instance.CreateCommand(this.ShowDesignerOverlay);
            }
        }

        public void ShowDesignerOverlay()
        {
            var configuration = this.SelectedConfiguration;
            if (configuration == null)
            {
                return;
            }
            LayoutDesignerBehaviour.Instance.ShowDesignerOverlay(configuration);
        }

        protected override Freezable CreateInstanceCore()
        {
            return new LayoutTree();
        }

        protected override void OnDisposing()
        {
            UIComponentRoot.ActiveChanged -= this.OnActiveChanged;
            base.OnDisposing();
        }
    }
}
