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

        public BassStream(IBassStreamProvider provider, int channelHandle) : this()
        {
            this.Provider = provider;
            this.ChannelHandle = channelHandle;
        }

        public IBassStreamProvider Provider { get; private set; }

        public int ChannelHandle { get; private set; }

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
