using System;
using System.Windows;
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

        public ICommand SaveCommand
        {
            get
            {
                return new Command(() => this.Core.Components.Configuration.Save())
                {
                    Tag = CommandHints.DISMISS
                };
            }
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Settings();
        }
    }
}
