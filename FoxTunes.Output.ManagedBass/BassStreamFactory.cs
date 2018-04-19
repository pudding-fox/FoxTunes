using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;

namespace FoxTunes
{
    public class BassStreamFactory : StandardComponent, IBassStreamFactory
    {
        public BassStreamFactory()
        {
            this.Providers = new SortedList<byte, IBassStreamProvider>();
        }

        private SortedList<byte, IBassStreamProvider> Providers { get; set; }

        public IBassOutput Output { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Output = core.Components.Output as IBassOutput;
            base.InitializeComponent(core);
        }

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
                if (!provider.CanCreateStream(this.Output, playlistItem))
                {
                    continue;
                }
                Logger.Write(this, LogLevel.Debug, "Using bass stream provider with priority {0}: {1}", provider.Priority, provider.GetType().Name);
                var channelHandle = provider.CreateStream(this.Output, playlistItem);
                if (channelHandle != 0)
                {
                    Logger.Write(this, LogLevel.Debug, "Created stream from file {0}: {1}", playlistItem.FileName, channelHandle);
                    return channelHandle;
                }
                else
                {
                    Logger.Write(this, LogLevel.Warn, "The bass stream provider failed.");
                }
            }
            throw new NotImplementedException();
        }
    }
}
