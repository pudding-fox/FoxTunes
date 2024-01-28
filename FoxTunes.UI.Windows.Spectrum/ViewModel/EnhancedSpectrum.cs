using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace FoxTunes.ViewModel
{
    public class EnhancedSpectrum : ViewModelBase
    {
        public StringCollection Levels
        {
            get
            {
                var levels = new List<string>();
                for (var level = EnhancedSpectrumRenderer.DB_MIN; level <= EnhancedSpectrumRenderer.DB_MAX; level += 10)
                {
                    levels.Add(Convert.ToString(level));
                }
                return new StringCollection(levels);
            }
        }

        private StringCollection _Bands { get; set; }

        public StringCollection Bands
        {
            get
            {
                return this._Bands;
            }
            private set
            {
                this._Bands = value;
                this.OnBandsChanged();
            }
        }

        protected virtual void OnBandsChanged()
        {
            if (this.BandsChanged != null)
            {
                this.BandsChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Bands");
        }

        public event EventHandler BandsChanged;

        public override void InitializeComponent(ICore core)
        {
            core.Components.Configuration.GetElement<SelectionConfigurationElement>(
                SpectrumBehaviourConfiguration.SECTION,
                SpectrumBehaviourConfiguration.BANDS_ELEMENT
            ).ConnectValue(value =>
            {
                var bands = SpectrumBehaviourConfiguration.GetBands(value).Select(
                    band => band < 1000 ? Convert.ToString(band) : string.Format("{0:0.##}K", (float)band / 1000)
                );
                this.Bands = new StringCollection(bands);
            });
            base.InitializeComponent(core);
        }

        protected override Freezable CreateInstanceCore()
        {
            return new EnhancedSpectrum();
        }
    }
}
