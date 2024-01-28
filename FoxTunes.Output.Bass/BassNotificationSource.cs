using FoxTunes.Interfaces;
using ManagedBass;
using System;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class BassNotificationSource : BaseComponent
    {
        const int ENDING_THRESHOLD = 5;

        public BassNotificationSource(BassOutputStream outputStream)
        {
            this.OutputStream = outputStream;
        }

        public BassOutputStream OutputStream { get; private set; }

        public long EndingPosition
        {
            get
            {
                return this.OutputStream.Length - Bass.ChannelSeconds2Bytes(this.OutputStream.ChannelHandle, ENDING_THRESHOLD);
            }
        }

        public override void InitializeComponent(ICore core)
        {
            BassUtils.OK(Bass.ChannelSetSync(
                this.OutputStream.ChannelHandle,
                SyncFlags.Position,
                this.EndingPosition,
                this.OnEnding
            ));
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
#if NET40
            var task = TaskEx.Run(this.OnEnding);
#else
            var task = Task.Run(this.OnEnding);
#endif
        }

        public virtual Task OnEnding()
        {
            if (this.Ending == null)
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            var e = new AsyncEventArgs();
            this.Ending(this, e);
            return e.Complete();
        }

        public event AsyncEventHandler Ending = delegate { };

        protected virtual void OnEnded(int Handle, int Channel, int Data, IntPtr User)
        {
            //Critical: Don't block in this call back, it glitches playback.
#if NET40
            var task = TaskEx.Run(this.OnEnded);
#else
            var task = Task.Run(this.OnEnded);
#endif
        }

        public virtual Task OnEnded()
        {
            if (this.Ended == null)
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            var e = new AsyncEventArgs();
            this.Ended(this, e);
            return e.Complete();
        }

        public event AsyncEventHandler Ended = delegate { };
    }
}
