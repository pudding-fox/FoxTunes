using FoxTunes.Interfaces;
using System;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class Settings : ViewModelBase
    {
        public Settings()
        {
            Windows.SettingsWindowCreated += this.OnSettingsWindowCreated;
            Windows.SettingsWindowClosed += this.OnSettingsWindowClosed;
        }

        public ISignalEmitter SignalEmitter { get; private set; }

        public bool SettingsVisible
        {
            get
            {
                return Windows.IsSettingsWindowCreated;
            }
            set
            {
                if (value)
                {
                    this.SignalEmitter.Send(new Signal(this, CommonSignals.SettingsUpdated));
                    Windows.SettingsWindow.DataContext = this.Core;
                    Windows.SettingsWindow.Show();
                }
                else if (Windows.IsSettingsWindowCreated)
                {
                    Windows.SettingsWindow.Close();
                }
            }
        }

        protected virtual void OnSettingsVisibleChanged()
        {
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

        public ICommand ToggleCommand
        {
            get
            {
                return new Command(() => this.SettingsVisible = !this.SettingsVisible);
            }
        }

        public override void InitializeComponent(ICore core)
        {
            this.SignalEmitter = this.Core.Components.SignalEmitter;
            base.InitializeComponent(core);
        }

        protected virtual void OnSettingsWindowCreated(object sender, EventArgs e)
        {
            this.OnSettingsVisibleChanged();
        }

        protected virtual void OnSettingsWindowClosed(object sender, EventArgs e)
        {
            this.OnSettingsVisibleChanged();
        }

        protected override void OnDisposing()
        {
            Windows.SettingsWindowCreated -= this.OnSettingsWindowCreated;
            Windows.SettingsWindowClosed -= this.OnSettingsWindowClosed;
            base.OnDisposing();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Settings();
        }
    }
}
