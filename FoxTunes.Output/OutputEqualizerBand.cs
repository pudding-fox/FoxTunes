using FoxTunes.Interfaces;
using System;

namespace FoxTunes
{
    public abstract class OutputEqualizerBand : BaseComponent, IOutputEqualizerBand
    {
        protected OutputEqualizerBand(int position)
        {
            this.Position = position;
        }

        public int Position { get; private set; }

        public abstract float MinCenter { get; }

        public abstract float MaxCenter { get; }

        public abstract float Center { get; set; }

        protected virtual void OnCenterChanged()
        {
            if (this.CenterChanged != null)
            {
                this.CenterChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Center");
        }

        public event EventHandler CenterChanged;

        public abstract float MinWidth { get; }

        public abstract float MaxWidth { get; }

        public abstract float Width { get; set; }

        protected virtual void OnWidthChanged()
        {
            if (this.WidthChanged != null)
            {
                this.WidthChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Width");
        }

        public event EventHandler WidthChanged;

        public abstract float MinValue { get; }

        public abstract float MaxValue { get; }

        public abstract float Value { get; set; }

        protected virtual void OnValueChanged()
        {
            if (this.ValueChanged != null)
            {
                this.ValueChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Value");
        }

        public event EventHandler ValueChanged;
    }
}
