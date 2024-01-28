using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;

namespace FoxTunes
{
    public class BassStreamFactory : BaseComponent, IBassStreamFactory
    {
        public BassStreamFactory()
        {
            this.Providers = new SortedList<byte, IBassStreamProvider>();
        }

        public BassStreamFactory(IBassOutput output)
            : this()
        {
            this.Output = output;
            this.Register(new BassDefaultStreamProvider(output));
            this.Register(new BassDsdStreamProvider(output));
        }

        private SortedList<byte, IBassStreamProvider> Providers { get; set; }

        public IBassOutput Output { get; private set; }

        public void Register(IBassStreamProvider provider)
        {
            this.Providers.Add(provider.Priority, provider);
            Logger.Write(this, LogLevel.Debug, "Registered bass stream provider with priority {0}: {1}", provider.Priority, provider.GetType().Name);
        }

        public int CreateStream(PlaylistItem playlistItem)
        {
            Logger.Write(this, LogLevel.Debug, "Attempting to create stream for playlist item: {0} => {1}", playlistItem.Id, playlistItem.FileName);
            foreach (var provider in this.Providers.Values)
            {
                if (!provider.CanCreateStream(playlistItem))
                {
                    continue;
                }
                Logger.Write(this, LogLevel.Debug, "Using bass stream provider with priority {0}: {1}", provider.Priority, provider.GetType().Name);
                var channelHandle = provider.CreateStream(playlistItem);
                if (channelHandle != 0)
                {
                    Logger.Write(this, LogLevel.Debug, "Created stream from file {0}: {1}", playlistItem.FileName, channelHandle);
                    return channelHandle;
                }
            }
            throw new NotImplementedException();
        }
    }
}
