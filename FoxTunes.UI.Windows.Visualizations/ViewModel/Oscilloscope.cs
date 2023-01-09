using System;
using System.Windows;

namespace FoxTunes.ViewModel
{
    public class Oscilloscope : ConfigurableViewModelBase
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

        protected override void OnConfigurationChanged()
        {
            if (this.Configuration != null)
            {
                this.Configuration.GetElement<BooleanConfigurationElement>(
                    OscilloscopeConfiguration.SECTION,
                    OscilloscopeConfiguration.DROP_SHADOW_ELEMENT
                ).ConnectValue(value => this.DropShadow = value);
            }
            base.OnConfigurationChanged();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Oscilloscope();
        }
    }
}
