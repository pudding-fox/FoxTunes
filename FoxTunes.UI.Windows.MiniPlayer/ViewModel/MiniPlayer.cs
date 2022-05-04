using FoxTunes.Interfaces;
using System;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class MiniPlayer : ViewModelBase
    {
        public IConfiguration Configuration { get; private set; }

        private BooleanConfigurationElement _Enabled { get; set; }

        public BooleanConfigurationElement Enabled
        {
            get
            {
                return this._Enabled;
            }
            set
            {
                this._Enabled = value;
                this.OnEnabledChanged();
            }
        }

        protected virtual void OnEnabledChanged()
        {
            if (this.EnabledChanged != null)
            {
                this.EnabledChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Enabled");
        }

        public event EventHandler EnabledChanged;
        
        public ICommand ShowCommand
        {
            get
            {
                return new Command(() => this.Enabled.Value = true);
            }
        }

        public ICommand HideCommand
        {
            get
            {
                return new Command(() => this.Enabled.Value = false);
            }
        }

        public ICommand ToggleCommand
        {
            get
            {
                return new Command(() => this.Enabled.Value = !this.Enabled.Value);
            }
        }

        protected override void InitializeComponent(ICore core)
        {
            this.Configuration = core.Components.Configuration;
            this.Enabled = this.Configuration.GetElement<BooleanConfigurationElement>(
                MiniPlayerBehaviourConfiguration.SECTION,
                MiniPlayerBehaviourConfiguration.ENABLED_ELEMENT
            );
            base.InitializeComponent(core);
        }

        protected override Freezable CreateInstanceCore()
        {
            return new MiniPlayer();
        }
    }
}
