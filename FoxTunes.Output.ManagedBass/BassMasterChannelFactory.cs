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
                case BassOutputMode.WASAPI:
                    return new BassWasapiMasterChannel(output);
            }
            throw new NotImplementedException();
        }

        public static readonly BassMasterChannelFactory Instance = new BassMasterChannelFactory();
    }
}
