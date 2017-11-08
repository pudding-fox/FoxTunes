using FoxTunes.Interfaces;
using ManagedBass;
using System;
using System.Timers;

namespace FoxTunes
{
    public class BassNotificationSource : BaseComponent, IDisposable
    {
        const int STOPPING_THRESHOLD = 5;

        public BassNotificationSource(BassOutputStream outputStream)
        {
            this.OutputStream = outputStream;
        }

        public BassOutputStream OutputStream { get; private set; }

        public int Interval { get; set; }

        private Timer Timer { get; set; }

        public override void InitializeComponent(ICore core)
        {
            this.Timer = new Timer();
            this.Timer.Interval = this.Interval;
            this.Timer.Elapsed += this.Timer_Elapsed;
            this.Timer.Start();
            BassUtils.OK(Bass.ChannelSetSync(
                this.OutputStream.ChannelHandle,
                SyncFlags.Position,
                this.OutputStream.Length - Bass.ChannelSeconds2Bytes(this.OutputStream.ChannelHandle, STOPPING_THRESHOLD),
                this.ChannelSync_Ending
            ));
            BassUtils.OK(Bass.ChannelSetSync(
                this.OutputStream.ChannelHandle,
                SyncFlags.End,
                0,
                this.ChannelSync_Ended
            ));
            base.InitializeComponent(core);
        }

        protected virtual void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.OnUpdated();
        }

        protected virtual void ChannelSync_Ending(int Handle, int Channel, int Data, IntPtr User)
        {
            Logger.Write(this, LogLevel.Debug, "Channel {0} sync point reached: Ending.", this.OutputStream.ChannelHandle);
            this.OnStopping();
        }

        protected virtual void ChannelSync_Ended(int Handle, int Channel, int Data, IntPtr User)
        {
            Logger.Write(this, LogLevel.Debug, "Channel {0} sync point reached: Ended.", this.OutputStream.ChannelHandle);
            this.OnStopped();
        }

        protected virtual void OnUpdated()
        {
            if (this.Updated == null)
            {
                return;
            }
            this.Updated(this, BassNotificationSourceEventArgs.Empty);
        }

        public event BassNotificationSourceEventHandler Updated = delegate { };

        protected virtual void OnStopping()
        {
            if (this.Stopping == null)
            {
                return;
            }
            this.Stopping(this, BassNotificationSourceEventArgs.Empty);
        }

        public event BassNotificationSourceEventHandler Stopping = delegate { };

        protected virtual void OnStopped()
        {
            if (this.Stopped == null)
            {
                return;
            }
            this.Stopped(this, BassNotificationSourceEventArgs.Empty);
        }

        public event BassNotificationSourceEventHandler Stopped = delegate { };

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

        protected virtual void OnDisposing()
        {
            this.Timer.Dispose();
        }

        ~BassNotificationSource()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            this.Dispose(true);
        }
    }

    public delegate void BassNotificationSourceEventHandler(object sender, BassNotificationSourceEventArgs e);

    public class BassNotificationSourceEventArgs : EventArgs
    {
        new public static BassNotificationSourceEventArgs Empty = new BassNotificationSourceEventArgs();
    }
}
