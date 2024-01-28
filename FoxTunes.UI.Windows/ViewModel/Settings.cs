using FoxTunes.Interfaces;
using System;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class Settings : ViewModelBase
    {
        public ISignalEmitter SignalEmitter { get; private set; }

        public bool SettingsVisible
        {
            get
            {
                return Windows.Registrations.IsVisible(SettingsWindow.ID);
            }
            set
            {
                if (value)
                {
                    Windows.Registrations.Show(SettingsWindow.ID);
                }
                else
                {
                    Windows.Registrations.Hide(SettingsWindow.ID);
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

        protected override void InitializeComponent(ICore core)
        {
            Windows.Registrations.AddCreated(SettingsWindow.ID, this.OnWindowCreated);
            Windows.Registrations.AddClosed(SettingsWindow.ID, this.OnWindowClosed);
            this.SignalEmitter = core.Components.SignalEmitter;
            base.InitializeComponent(core);
        }

        protected virtual void OnWindowCreated(object sender, EventArgs e)
        {
            this.OnSettingsVisibleChanged();
        }

        protected virtual void OnWindowClosed(object sender, EventArgs e)
        {
            this.OnSettingsVisibleChanged();
        }

        protected override void OnDisposing()
        {
            Windows.Registrations.RemoveCreated(SettingsWindow.ID, this.OnWindowCreated);
            Windows.Registrations.RemoveClosed(SettingsWindow.ID, this.OnWindowClosed);
            base.OnDisposing();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Settings();
        }
    }
}
