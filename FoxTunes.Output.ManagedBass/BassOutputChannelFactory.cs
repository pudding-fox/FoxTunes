using System;

namespace FoxTunes
{
    public class BassOutputChannelFactory
    {
        public BassOutputChannel Create(BassOutput output)
        {
            switch (output.Mode)
            {
                case BassOutputMode.DirectSound:
                    return new BassOutputChannel(output);
                case BassOutputMode.ASIO:
                    return new BassAsioOutputChannel(output);
            }
            throw new NotImplementedException();
        }

        public static readonly BassOutputChannelFactory Instance = new BassOutputChannelFactory();
    }
}
