using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace FoxTunes.ViewModel
{
    public class Volume : ViewModelBase
    {
        public float Value
        {
            get
            {
                if (this.Output == null)
                {
                    return 0;
                }
                return this.Output.Volume;
            }
            set
            {
                if (this.Output == null)
                {
                    return;
                }
                this.Output.Volume = value;
                this.OnValueChanged();
            }
        }

        protected virtual void OnValueChanged()
        {
            if (this.ValueChanged != null)
            {
                this.ValueChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Value");
        }

        public event EventHandler ValueChanged;

        public IOutput Output { get; private set; }

        protected override void OnCoreChanged()
        {
            this.Output = this.Core.Components.Output;
            this.Output.VolumeChanged += this.OnVolumeChanged;
            //TODO: Bad .Wait().
            this.Refresh().Wait();
            base.OnCoreChanged();
        }

        protected virtual void OnVolumeChanged(object sender, EventArgs e)
        {
            var task = this.Refresh();
        }

        protected virtual Task Refresh()
        {
            return Windows.Invoke(new Action(this.OnValueChanged));
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Volume();
        }
    }
}
