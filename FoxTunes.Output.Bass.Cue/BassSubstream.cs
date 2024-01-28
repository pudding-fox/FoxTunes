using FoxTunes.Interfaces;
using ManagedBass;
using System;

namespace FoxTunes
{
    public class BassSubstream : BassStream
    {
        public BassSubstream(IBassStreamProvider provider, int channelHandle, int innerChannelHandle, long offset, long length) : base(provider, channelHandle, length)
        {
            this.InnerChannelHandle = innerChannelHandle;
            this.Offset = offset;
        }

        public int InnerChannelHandle { get; private set; }

        public long Offset { get; private set; }

        public override long Position
        {
            get
            {
                return Bass.ChannelGetPosition(this.InnerChannelHandle, PositionFlags.Bytes) - this.Offset;
            }
            set
            {
                BassUtils.OK(Bass.ChannelSetPosition(this.InnerChannelHandle, value + this.Offset, PositionFlags.Bytes));
                this.OnPositionChanged();
            }
        }

        public override void RegisterSyncHandlers()
        {
            var endPosition = this.Offset + this.Length;
            BassUtils.OK(Bass.ChannelSetSync(
                this.InnerChannelHandle,
                SyncFlags.Position,
                endPosition - Bass.ChannelSeconds2Bytes(this.InnerChannelHandle, ENDING_THRESHOLD),
                this.OnEnding
            ));
            if (endPosition < Bass.ChannelGetLength(this.InnerChannelHandle, PositionFlags.Bytes))
            {
                BassUtils.OK(Bass.ChannelSetSync(
                    this.InnerChannelHandle,
                    SyncFlags.Position,
                    endPosition,
                    this.OnEnded
                ));
            }
            else
            {
                BassUtils.OK(Bass.ChannelSetSync(
                    this.ChannelHandle,
                    SyncFlags.End,
                    0,
                    this.OnEnded
                ));
            }
        }

        public override bool CanReset
        {
            get
            {
                return false;
            }
        }

        public override void Reset()
        {
            throw new NotImplementedException();
        }
    }
}
