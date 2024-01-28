using FoxTunes.Interfaces;
using System;
using System.Windows;

namespace FoxTunes.ViewModel
{
    public class Oscilloscope : ViewModelBase
    {
        private bool _DropShadow { get; set; }

        public bool DropShadow
        {
            get
            {
                return this._DropShadow;
            }
            private set
            {
                this._DropShadow = value;
                this.OnDropShadowChanged();
            }
        }

        protected virtual void OnDropShadowChanged()
        {
            if (this.DropShadowChanged != null)
            {
                this.DropShadowChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("DropShadow");
        }

        public event EventHandler DropShadowChanged;

        protected override void InitializeComponent(ICore core)
        {
            core.Components.Configuration.GetElement<BooleanConfigurationElement>(
                OscilloscopeBehaviourConfiguration.SECTION,
                OscilloscopeBehaviourConfiguration.DROP_SHADOW_ELEMENT
            ).ConnectValue(value => this.DropShadow = value);
            base.InitializeComponent(core);
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Oscilloscope();
        }
    }
}
