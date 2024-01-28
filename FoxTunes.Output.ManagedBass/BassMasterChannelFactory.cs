using System;

namespace FoxTunes
{
    public class BassMasterChannelFactory
    {
        public BassMasterChannel Create(BassOutput output)
        {
            switch (output.Mode)
            {
                case BassOutputMode.DirectSound:
                    return new BassMasterChannel(output);
                case BassOutputMode.ASIO:
                    return new BassAsioMasterChannel(output);
            }
            throw new NotImplementedException();
        }

        public static readonly BassMasterChannelFactory Instance = new BassMasterChannelFactory();
    }
}
