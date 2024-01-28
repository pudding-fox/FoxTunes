using FoxTunes.Interfaces;
using ManagedBass;
using System;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class BassStream : IBassStream
    {
        public const int ENDING_THRESHOLD = 5;

        private BassStream()
        {
            this.Errors = Errors.OK;
        }

        public BassStream(IBassStreamProvider provider, int channelHandle, long length) : this()
        {
            this.Provider = provider;
            this.ChannelHandle = channelHandle;
            this.Length = length;
        }

        public IBassStreamProvider Provider { get; private set; }

        public int ChannelHandle { get; private set; }

        public long Length { get; private set; }

        public virtual long Position
        {
            get
            {
                return Bass.ChannelGetPosition(this.ChannelHandle, PositionFlags.Bytes);
            }
            set
            {
                BassUtils.OK(Bass.ChannelSetPosition(this.ChannelHandle, value, PositionFlags.Bytes));
            }
        }

        public Errors Errors { get; private set; }

        public bool IsEmpty
        {
            get
            {
                return this.ChannelHandle == 0;
            }
        }

        public virtual void RegisterSyncHandlers()
        {
            BassUtils.OK(Bass.ChannelSetSync(
                this.ChannelHandle,
                SyncFlags.Position,
                this.Length - Bass.ChannelSeconds2Bytes(this.ChannelHandle, ENDING_THRESHOLD),
                this.OnEnding
            ));
            BassUtils.OK(Bass.ChannelSetSync(
                this.ChannelHandle,
                SyncFlags.End,
                0,
                this.OnEnded
            ));
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

        public event AsyncEventHandler Ending;

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

        public event AsyncEventHandler Ended;

        public static IBassStream Error(Errors errors)
        {
            return new BassStream()
            {
                Errors = errors
            };
        }

        public static IBassStream Empty
        {
            get
            {
                return new BassStream();
            }
        }
    }
}
