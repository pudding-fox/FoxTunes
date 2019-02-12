using FoxTunes.Interfaces;
using System;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class Settings : ViewModelBase
    {
        public IConfiguration Configuration { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

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

        private bool _SettingsVisible { get; set; }

        public bool SettingsVisible
        {
            get
            {
                return this._SettingsVisible;
            }
            set
            {
                this._SettingsVisible = value;
                this.OnSettingsVisibleChanged();
            }
        }

        protected virtual void OnSettingsVisibleChanged()
        {
            if (this.SettingsVisible)
            {
                this.SignalEmitter.Send(new Signal(this, CommonSignals.SettingsUpdated));
                Windows.SettingsWindow.DataContext = this.Core;
                Windows.SettingsWindow.Closed += (sender, e) => this.SettingsVisible = false;
                Windows.SettingsWindow.Show();
            }
            else if (Windows.IsSettingsWindowCreated)
            {
                Windows.SettingsWindow.Close();
            }
            if (this.SettingsVisibleChanged != null)
            {
                this.SettingsVisibleChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("SettingsVisible");
        }

        public event EventHandler SettingsVisibleChanged;

        public ICommand ShowCommand
        {
            get
            {
                return new Command(() => this.SettingsVisible = true);
            }
        }

        public ICommand HideCommand
        {
            get
            {
                return new Command(() => this.SettingsVisible = false);
            }
        }

        public override void InitializeComponent(ICore core)
        {
            this.Configuration = this.Core.Components.Configuration;
            this.ScalingFactor = this.Configuration.GetElement<DoubleConfigurationElement>(
              WindowsUserInterfaceConfiguration.SECTION,
              WindowsUserInterfaceConfiguration.UI_SCALING_ELEMENT
            );
            this.SignalEmitter = this.Core.Components.SignalEmitter;
            base.InitializeComponent(core);
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Settings();
        }
    }
}
