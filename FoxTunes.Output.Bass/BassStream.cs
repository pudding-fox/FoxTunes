using FoxTunes.Interfaces;
using ManagedBass;

namespace FoxTunes
{
    public class BassStream : IBassStream
    {
        private BassStream()
        {
            this.Errors = Errors.OK;
        }

        public BassStream(IBassStreamProvider provider, int channelHandle, long offset, long length) : this()
        {
            this.Provider = provider;
            this.ChannelHandle = channelHandle;
            this.Offset = offset;
            this.Length = length;
        }

        public IBassStreamProvider Provider { get; private set; }

        public int ChannelHandle { get; private set; }

        public long Offset { get; private set; }

        public long Length { get; private set; }

        public long Position
        {
            get
            {
                return Bass.ChannelGetPosition(this.ChannelHandle, PositionFlags.Bytes) - this.Offset;
            }
            set
            {
                BassUtils.OK(Bass.ChannelSetPosition(this.ChannelHandle, value + this.Offset, PositionFlags.Bytes));
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
