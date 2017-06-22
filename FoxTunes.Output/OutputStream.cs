using FoxTunes.Interfaces;
using System;

namespace FoxTunes
{
    public abstract class OutputStream : BaseComponent, IOutputStream
    {
        public abstract long Position { get; set; }

        protected virtual void OnPositionChanged()
        {
            if (this.PositionChanged != null)
            {
                this.PositionChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Position");
        }

        public event EventHandler PositionChanged = delegate { };

        public abstract long Length { get; }

        public abstract int BlockAlign { get; }

        public abstract bool Paused { get; set; }

        public abstract void Play();

        public abstract void Stop();

        protected virtual void OnStopped(bool manual)
        {
            if (this.Stopped == null)
            {
                return;
            }
            this.Stopped(this, new StoppedEventArgs(manual));
        }

        public event StoppedEventHandler Stopped = delegate { };

        public abstract void Dispose();
    }
}
