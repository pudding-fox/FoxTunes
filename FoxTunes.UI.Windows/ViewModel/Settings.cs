using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class Settings : ViewModelBase
    {
        public ISignalEmitter SignalEmitter { get; private set; }

        public IConfiguration Configuration { get; private set; }

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
            }
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
                var command = CommandFactory.Instance.CreateCommand(
                    () => Task.Run(() => this.Configuration.Save()),
                    () => this.Configuration != null
                );
                command.Tag = CommandHints.DISMISS;
                return command;
            }
        }

        public override void InitializeComponent(ICore core)
        {
            this.SignalEmitter = this.Core.Components.SignalEmitter;
            this.Configuration = this.Core.Components.Configuration;
            base.InitializeComponent(core);
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Settings();
        }
    }
}
