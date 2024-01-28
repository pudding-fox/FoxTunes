using FoxTunes.Interfaces;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class Main : ViewModelBase
    {
        public IConfiguration Configuration { get; private set; }

        public Icon Icon
        {
            get
            {
                using (var stream = typeof(Main).Assembly.GetManifestResourceStream("FoxTunes.UI.Windows.Images.Fox.ico"))
                {
                    if (stream == null)
                    {
                        return null;
                    }
                    return new Icon(stream);
                }
            }
        }

        private BooleanConfigurationElement _ShowNotifyIcon { get; set; }

        public BooleanConfigurationElement ShowNotifyIcon
        {
            get
            {
                return this._ShowNotifyIcon;
            }
            set
            {
                this._ShowNotifyIcon = value;
                this.OnShowNotifyIconChanged();
            }
        }

        protected virtual void OnShowNotifyIconChanged()
        {
            if (this.ShowNotifyIconChanged != null)
            {
                this.ShowNotifyIconChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("ShowNotifyIcon");
        }

        public event EventHandler ShowNotifyIconChanged;

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
        
        public override void InitializeComponent(ICore core)
        {
            this.Configuration = this.Core.Components.Configuration;
            this.ShowNotifyIcon = this.Configuration.GetElement<BooleanConfigurationElement>(
              NotifyIconConfiguration.SECTION,
              NotifyIconConfiguration.ENABLED_ELEMENT
            );
            this.ScalingFactor = this.Configuration.GetElement<DoubleConfigurationElement>(
              WindowsUserInterfaceConfiguration.SECTION,
              WindowsUserInterfaceConfiguration.UI_SCALING_ELEMENT
            );
            base.InitializeComponent(core);
        }

        public ICommand RestoreCommand
        {
            get
            {
                return new Command(this.Restore);
            }
        }

        public void Restore()
        {
            if (Windows.IsMainWindowCreated && Windows.ActiveWindow == Windows.MainWindow)
            {
                Windows.ActiveWindow.Show();
                if (Windows.ActiveWindow.WindowState == WindowState.Minimized)
                {
                    Windows.ActiveWindow.WindowState = WindowState.Normal;
                }
                Windows.ActiveWindow.BringToFront();
            }
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Main();
        }
    }
}
