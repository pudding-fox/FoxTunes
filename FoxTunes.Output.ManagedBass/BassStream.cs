using FoxTunes.Interfaces;

namespace FoxTunes
{
    public class BassStream : IBassStream
    {
        private BassStream()
        {

        }

        public BassStream(IBassStreamProvider provider, int channelHandle) : this()
        {
            this.Provider = provider;
            this.ChannelHandle = channelHandle;
        }

        public IBassStreamProvider Provider { get; private set; }

        public int ChannelHandle { get; private set; }

        public bool IsEmpty
        {
            get
            {
                return this.ChannelHandle == 0;
            }
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
