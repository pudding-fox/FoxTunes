using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Windows;

namespace FoxTunes.ViewModel
{
    public class PeakMeter : ViewModelBase
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

        private StringCollection _Channels { get; set; }

        public StringCollection Channels
        {
            get
            {
                return this._Channels;
            }
            private set
            {
                this._Channels = value;
                this.OnChannelsChanged();
            }
        }

        protected virtual void OnChannelsChanged()
        {
            if (this.ChannelsChanged != null)
            {
                this.ChannelsChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Channels");
        }

        public event EventHandler ChannelsChanged;

        public override void InitializeComponent(ICore core)
        {
            //TODO: Use actual channel count.
            this.Channels = new StringCollection(new[]
            {
                "L",
                "R"
            });
            base.InitializeComponent(core);
        }

        protected override Freezable CreateInstanceCore()
        {
            return new PeakMeter();
        }
    }
}
