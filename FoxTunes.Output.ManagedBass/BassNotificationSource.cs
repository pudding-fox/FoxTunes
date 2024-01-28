using FoxTunes.Interfaces;
using ManagedBass;
using System;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class BassNotificationSource : BaseComponent
    {
        const int STOPPING_THRESHOLD = 5;

        public BassNotificationSource(BassOutputStream outputStream)
        {
            this.OutputStream = outputStream;
        }

        public BassOutputStream OutputStream { get; private set; }

        public long EndingPosition
        {
            get
            {
                return this.OutputStream.Length - Bass.ChannelSeconds2Bytes(this.OutputStream.ChannelHandle, STOPPING_THRESHOLD);
            }
        }

        public override void InitializeComponent(ICore core)
        {
            Logger.Write(this, LogLevel.Debug, "Creating \"Ending\" channel sync {0} seconds from the end for channel: {1}", STOPPING_THRESHOLD, this.OutputStream.ChannelHandle);
            BassUtils.OK(Bass.ChannelSetSync(
                this.OutputStream.ChannelHandle,
                SyncFlags.Position,
                this.EndingPosition,
                this.OnEnding
            ));
            Logger.Write(this, LogLevel.Debug, "Creating \"End\" channel sync for channel: {0}", this.OutputStream.ChannelHandle);
            BassUtils.OK(Bass.ChannelSetSync(
                this.OutputStream.ChannelHandle,
                SyncFlags.End,
                0,
                this.OnEnded
            ));
            base.InitializeComponent(core);
        }

        protected virtual void OnEnding(int Handle, int Channel, int Data, IntPtr User)
        {
            //Critical: Don't block in this call back, it glitches playback.
            var task = Task.Run(new Action(this.Ending));
        }

        public virtual void Ending()
        {
            Logger.Write(this, LogLevel.Debug, "Channel {0} sync point reached: \"Ending\".", this.OutputStream.ChannelHandle);
            var task = this.OnStopping();
        }

        protected virtual void OnEnded(int Handle, int Channel, int Data, IntPtr User)
        {
            //Critical: Don't block in this call back, it glitches playback.
            var task = Task.Run(new Action(this.Ended));
        }

        public virtual void Ended()
        {
            Logger.Write(this, LogLevel.Debug, "Channel {0} sync point reached: \"Ended\".", this.OutputStream.ChannelHandle);
            var task = this.OnStopped();
        }

        protected virtual Task OnStopping()
        {
            if (this.Stopping == null)
            {
                return Task.CompletedTask;
            }
            var e = new AsyncEventArgs();
            this.Stopping(this, e);
            return e.Complete();
        }

        public event AsyncEventHandler Stopping = delegate { };

        protected virtual Task OnStopped()
        {
            if (this.Stopped == null)
            {
                return Task.CompletedTask;
            }
            var e = new AsyncEventArgs();
            this.Stopped(this, e);
            return e.Complete();
        }

        public event AsyncEventHandler Stopped = delegate { };
    }
}
