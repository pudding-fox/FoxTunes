using FoxTunes.Interfaces;
using System;
using System.Windows;
using System.Windows.Threading;

namespace FoxTunes.ViewModel
{
    public class OutputStream : ViewModelBase
    {
        private static readonly TimeSpan UPDATE_INTERVAL = TimeSpan.FromSeconds(1);

        private OutputStream()
        {
            this.Timer = new DispatcherTimer(DispatcherPriority.Background);
            this.Timer.Interval = UPDATE_INTERVAL;
            this.Timer.Tick += this.OnTick;
            this.Timer.Start();
        }

        public OutputStream(IOutputStream outputStream) : this()
        {
            this.InnerOutputStream = outputStream;
        }

        public DispatcherTimer Timer { get; private set; }

        public IOutputStream InnerOutputStream { get; private set; }

        protected virtual void OnTick(object sender, EventArgs e)
        {
            this.OnPositionChanged();
        }

        public long Position
        {
            get
            {
                return this.InnerOutputStream.Position;
            }
            set
            {
                this.InnerOutputStream.Position = value;
                this.OnPositionChanged();
            }
        }

        protected virtual void OnPositionChanged()
        {
            if (this.PositionChanged != null)
            {
                this.PositionChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Position");
        }

        public event EventHandler PositionChanged = delegate { };

        public long Length
        {
            get
            {
                return this.InnerOutputStream.Length;
            }
        }

        protected override Freezable CreateInstanceCore()
        {
            return new OutputStream(null);
        }
    }
}
