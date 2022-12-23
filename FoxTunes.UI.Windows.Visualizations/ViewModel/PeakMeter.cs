using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes.ViewModel
{
    public class PeakMeter : ViewModelBase
    {
        public IOutput Output { get; private set; }

        private Orientation _Orientation { get; set; }

        public Orientation Orientation
        {
            get
            {
                return this._Orientation;
            }
            private set
            {
                this._Orientation = value;
                this.OnOrientationChanged();
            }
        }

        protected virtual void OnOrientationChanged()
        {
            var task = this.Refresh();
            if (this.OrientationChanged != null)
            {
                this.OrientationChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Orientation");
        }

        public event EventHandler OrientationChanged;

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

        protected override void InitializeComponent(ICore core)
        {
            this.Output = core.Components.Output;
            this.Output.CanGetDataChanged += this.OnCanGetDataChanged;
            core.Components.Configuration.GetElement<SelectionConfigurationElement>(
                PeakMeterBehaviourConfiguration.SECTION,
                PeakMeterBehaviourConfiguration.ORIENTATION
            ).ConnectValue(value => this.Orientation = PeakMeterBehaviourConfiguration.GetOrientation(value));
            base.InitializeComponent(core);
        }

        protected virtual void OnCanGetDataChanged(object sender, EventArgs e)
        {
            var task = this.Refresh();
        }

        protected virtual Task Refresh()
        {
            var channels = default(IDictionary<int, OutputChannel>);
            if (!this.Output.CanGetData || !this.Output.GetDataChannelMap(out channels))
            {
                channels = new Dictionary<int, OutputChannel>()
                {
                    { 0, OutputChannel.Left },
                    { 1, OutputChannel.Right }
                };
            }
            var channelNames = channels.Values.Select(ChannelMap.GetChannelName);
            switch (this.Orientation)
            {
                default:
                case Orientation.Horizontal:
                    channelNames = channelNames.Reverse();
                    break;
                case Orientation.Vertical:
                    //Nothing to do.
                    break;
            }
            return Windows.Invoke(() => this.Channels = new StringCollection(channelNames));
        }

        protected override Freezable CreateInstanceCore()
        {
            return new PeakMeter();
        }

        protected override void OnDisposing()
        {
            if (this.Output != null)
            {
                this.Output.CanGetDataChanged -= this.OnCanGetDataChanged;
            }
            base.OnDisposing();
        }
    }
}
