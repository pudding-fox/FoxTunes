using FoxTunes.Interfaces;
using ManagedBass;
using System;

namespace FoxTunes
{
    public abstract class BassStreamComponent : BaseComponent, IBassStreamComponent
    {
        public abstract string Name { get; }

        public abstract string Description { get; }

        public abstract int ChannelHandle { get; protected set; }

        public virtual long BufferLength
        {
            get
            {
                return 0;
            }
            protected set
            {
                throw new NotImplementedException();
            }
        }

        public abstract bool IsActive { get; }

        public virtual bool GetFormat(out int rate, out int channels, out BassFlags flags)
        {
            var info = default(ChannelInfo);
            if (!Bass.ChannelGetInfo(this.ChannelHandle, out info))
            {
                rate = 0;
                channels = 0;
                flags = BassFlags.Default;
                return false;
            }
            if (info.Flags.HasFlag(BassFlags.DSDRaw))
            {
                rate = BassUtils.GetChannelDsdRate(this.ChannelHandle);
            }
            else
            {
                rate = info.Frequency;
            }
            channels = info.Channels;
            flags = info.Flags;
            return true;
        }

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

        ~BassStreamComponent()
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
