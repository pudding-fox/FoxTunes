using System;
using System.Windows;
using FoxTunes.Interfaces;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class Settings : ViewModelBase
    {
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
            if (this.SettingsVisibleChanged != null)
            {
                this.SettingsVisibleChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("SettingsVisible");
        }

        public event EventHandler SettingsVisibleChanged = delegate { };

        public ICommand SaveCommand
        {
            get
            {
                return new Command<IConfiguration>(
                    configuration => configuration.Save(),
                    configuration => configuration != null
                );
            }
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Settings();
        }

        public static readonly Settings Instance = new Settings();
    }
}
