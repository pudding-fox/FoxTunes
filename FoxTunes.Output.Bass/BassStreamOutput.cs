using FoxTunes.Interfaces;
using ManagedBass;
using System;

namespace FoxTunes
{
    public abstract class BassStreamOutput : BaseComponent, IBassStreamOutput
    {
        public const uint FFT256 = 0x80000000;

        public const uint FFT512 = 0x80000001;

        public const uint FFT1024 = 0x80000002;

        public abstract string Name { get; }

        public abstract string Description { get; }

        public abstract int Rate { get; protected set; }

        public abstract int Channels { get; protected set; }

        public abstract BassFlags Flags { get; protected set; }

        public abstract int ChannelHandle { get; protected set; }

        public virtual long BufferLength
        {
            get
            {
                return 0;
            }
        }


        public virtual bool CanControlVolume
        {
            get
            {
                return false;
            }
        }

        public virtual float Volume
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public abstract bool CheckFormat(int rate, int channels);

        public abstract void Connect(IBassStreamComponent previous);

        public virtual void ClearBuffer()
        {
            //Nothing to do.
        }

        protected virtual void OnInvalidate()
        {
            if (this.Invalidate == null)
            {
                return;
            }
            this.Invalidate(this, EventArgs.Empty);
        }

        public event EventHandler Invalidate;

        public abstract bool IsPlaying { get; protected set; }

        public abstract bool IsPaused { get; protected set; }

        public abstract bool IsStopped { get; protected set; }

        public abstract int Latency { get; }

        public abstract void Play();

        public abstract void Pause();

        public abstract void Resume();

        public abstract void Stop();

        public abstract int GetData(float[] buffer);

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.IsDisposed || !disposing)
            {
                return;
            }
            this.OnDisposing();
            this.IsDisposed = true;
        }

        protected abstract void OnDisposing();

        ~BassStreamOutput()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            try
            {
                this.Dispose(true);
            }
            catch
            {
                //Nothing can be done, never throw on GC thread.
            }
        }
    }
}
