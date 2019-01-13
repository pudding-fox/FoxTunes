using FoxTunes.Interfaces;
using System;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class Main : ViewModelBase
    {
        public IConfiguration Configuration { get; private set; }

        private BooleanConfigurationElement _ShowLibrary { get; set; }

        public BooleanConfigurationElement ShowLibrary
        {
            get
            {
                return this._ShowLibrary;
            }
            set
            {
                this._ShowLibrary = value;
                this.OnShowLibraryChanged();
            }
        }

        protected virtual void OnShowLibraryChanged()
        {
            if (this.ShowLibraryChanged != null)
            {
                this.ShowLibraryChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("ShowLibrary");
        }

        public event EventHandler ShowLibraryChanged = delegate { };

        private BooleanConfigurationElement _ShowArtwork { get; set; }

        public BooleanConfigurationElement ShowArtwork
        {
            get
            {
                return this._ShowArtwork;
            }
            set
            {
                this._ShowArtwork = value;
                this.OnShowArtworkChanged();
            }
        }

        protected virtual void OnShowArtworkChanged()
        {
            if (this.ShowArtworkChanged != null)
            {
                this.ShowArtworkChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("ShowArtwork");
        }

        public event EventHandler ShowArtworkChanged = delegate { };

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

        public event EventHandler ShowNotifyIconChanged = delegate { };

        private TextConfigurationElement _ScalingFactor { get; set; }

        public TextConfigurationElement ScalingFactor
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

        public event EventHandler ScalingFactorChanged = delegate { };

        public override void InitializeComponent(ICore core)
        {
            this.Configuration = this.Core.Components.Configuration;
            this.ShowLibrary = this.Configuration.GetElement<BooleanConfigurationElement>(
               WindowsUserInterfaceConfiguration.SECTION,
               WindowsUserInterfaceConfiguration.SHOW_LIBRARY_ELEMENT
            );
            this.ShowArtwork = this.Configuration.GetElement<BooleanConfigurationElement>(
              WindowsUserInterfaceConfiguration.SECTION,
              WindowsUserInterfaceConfiguration.SHOW_ARTWORK_ELEMENT
           );
            this.ShowNotifyIcon = this.Configuration.GetElement<BooleanConfigurationElement>(
              NotifyIconConfiguration.SECTION,
              NotifyIconConfiguration.ENABLED_ELEMENT
            );
            this.ScalingFactor = this.Configuration.GetElement<TextConfigurationElement>(
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
