using FoxTunes.Interfaces;
using System;

namespace FoxTunes
{
    public class BassOutputChannelFactory : BaseComponent
    {
        public BassOutputChannel Create(BassOutput output)
        {
            var outputChannel = default(BassOutputChannel);
            switch (output.Mode)
            {
                case BassOutputMode.DirectSound:
                    outputChannel = new BassOutputChannel(output);
                    break;
                case BassOutputMode.ASIO:
                    outputChannel = new BassAsioOutputChannel(output);
                    break;
                default:
                    throw new NotImplementedException();
            }
            Logger.Write(this, LogLevel.Debug, "Created {0}", outputChannel.GetType().Name);
            return outputChannel;
        }

        public static readonly BassOutputChannelFactory Instance = new BassOutputChannelFactory();
    }
}
