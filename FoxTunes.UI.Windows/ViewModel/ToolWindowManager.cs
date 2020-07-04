using FoxTunes.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class ToolWindowManager : ViewModelBase
    {
        public ToolWindowBehaviour Behaviour { get; private set; }

        private DoubleConfigurationElement _ScalingFactor { get; set; }

        public DoubleConfigurationElement ScalingFactor
        {
            get
            {
                return this._ScalingFactor;
            }
            set
            {
                this._ScalingFactor = value;
                this.OnScalingFactorChanged();
            }
        }

        protected virtual void OnScalingFactorChanged()
        {
            if (this.ScalingFactorChanged != null)
            {
                this.ScalingFactorChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("ScalingFactor");
        }

        public event EventHandler ScalingFactorChanged;

        private CollectionManager<ToolWindowConfiguration> _Windows { get; set; }

        public CollectionManager<ToolWindowConfiguration> Windows
        {
            get
            {
                return this._Windows;
            }
            set
            {
                this._Windows = value;
                this.OnWindowsChanged();
            }
        }

        protected virtual void OnWindowsChanged()
        {
            this.OnPropertyChanged("Windows");
        }

        public override void InitializeComponent(ICore core)
        {
            this.Behaviour = ComponentRegistry.Instance.GetComponent<ToolWindowBehaviour>();
            this.Windows = new CollectionManager<ToolWindowConfiguration>()
            {
                ItemFactory = () => new ToolWindowConfiguration()
                {
                    Title = "New Window",
                    Width = 400,
                    Height = 250,
                    ShowWithMainWindow = true,
                    ShowWithMiniWindow = false
                }
            };
            this.Dispatch(this.Refresh);
            base.InitializeComponent(core);
        }

        protected virtual Task Refresh()
        {
            var windows = this.Behaviour.Windows.Keys;
            return global::FoxTunes.Windows.Invoke(() => this.Windows.ItemsSource = new ObservableCollection<ToolWindowConfiguration>(windows));
        }

        public bool ToolWindowManagerVisible
        {
            get
            {
                return global::FoxTunes.Windows.IsToolWindowManagerWindowCreated;
            }
            set
            {
                if (value)
                {
                    global::FoxTunes.Windows.ToolWindowManagerWindow.DataContext = this.Core;
                    global::FoxTunes.Windows.ToolWindowManagerWindow.Show();
                }
                else if (global::FoxTunes.Windows.IsToolWindowManagerWindowCreated)
                {
                    global::FoxTunes.Windows.ToolWindowManagerWindow.Close();
                }
            }
        }

        protected virtual void OnToolWindowManagerVisibleChanged()
        {
            if (this.ToolWindowManagerVisibleChanged != null)
            {
                this.ToolWindowManagerVisibleChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("ToolWindowManagerVisible");
        }

        public event EventHandler ToolWindowManagerVisibleChanged;

        public ICommand SaveCommand
        {
            get
            {
                return CommandFactory.Instance.CreateCommand(this.Save);
            }
        }

        public Task Save()
        {
            return this.Behaviour.Update(this.Windows.ItemsSource);
        }

        public ICommand CancelCommand
        {
            get
            {
                return new Command(() => this.ToolWindowManagerVisible = false);
            }
        }

        protected override Freezable CreateInstanceCore()
        {
            return new ToolWindowManager();
        }
    }
}
