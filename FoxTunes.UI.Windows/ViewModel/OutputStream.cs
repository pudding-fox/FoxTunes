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
            this.OnDescriptionChanged();
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

        public event EventHandler PositionChanged;

        public long Length
        {
            get
            {
                return this.InnerOutputStream.Length;
            }
        }

        public string Description
        {
            get
            {
                return string.Format(
                    "{0}/{1}",
                    this.InnerOutputStream.GetDuration(this.InnerOutputStream.Position).ToString(@"mm\:ss"),
                    this.InnerOutputStream.GetDuration(this.InnerOutputStream.Length).ToString(@"mm\:ss")
                );
            }
        }

        protected virtual void OnDescriptionChanged()
        {
            if (this.DescriptionChanged != null)
            {
                this.DescriptionChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Description");
        }

        public event EventHandler DescriptionChanged;

        protected override void OnDisposing()
        {
            if (this.Timer != null)
            {
                this.Timer.Stop();
            }
            base.OnDisposing();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new OutputStream(null);
        }
    }
}
