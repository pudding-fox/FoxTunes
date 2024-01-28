using System;
using System.Windows;
using FoxTunes.Interfaces;

namespace FoxTunes.ViewModel
{
    public class Settings : ViewModelBase
    {
        private string _SettingsVisible { get; set; }

        public string SettingsVisible
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

        protected override Freezable CreateInstanceCore()
        {
            return new Settings();
        }

        public static readonly Settings Instance = new Settings();
    }
}
