using FoxTunes.Interfaces;
using ManagedBass;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FoxTunes
{
    public class BassStreamFactory : StandardComponent, IBassStreamFactory
    {
        const int CREATE_ATTEMPTS = 5;

        const int CREATE_ATTEMPT_INTERVAL = 400;

        public BassStreamFactory()
        {
            this.Providers = new SortedList<byte, IBassStreamProvider>(new PriorityComparer());
        }

        private SortedList<byte, IBassStreamProvider> Providers { get; set; }

        IEnumerable<IBassStreamProvider> IBassStreamFactory.Providers
        {
            get
            {
                return this.Providers.Values;
            }
        }

        public IBassOutput Output { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Output = core.Components.Output as IBassOutput;
            this.SignalEmitter = core.Components.SignalEmitter;
            base.InitializeComponent(core);
        }

        public void Register(IBassStreamProvider provider)
        {
            this.Providers.Add(provider.Priority, provider);
            Logger.Write(this, LogLevel.Debug, "Registered bass stream provider with priority {0}: {1}", provider.Priority, provider.GetType().Name);
        }

        public bool CreateStream(PlaylistItem playlistItem, bool immidiate, out int channelHandle)
        {
            Logger.Write(this, LogLevel.Debug, "Attempting to create stream for playlist item: {0} => {1}", playlistItem.Id, playlistItem.FileName);
            foreach (var provider in this.Providers.Values)
            {
                if (!provider.CanCreateStream(this.Output, playlistItem))
                {
                    continue;
                }
                Logger.Write(this, LogLevel.Debug, "Using bass stream provider with priority {0}: {1}", provider.Priority, provider.GetType().Name);
                for (var attempt = 0; attempt < CREATE_ATTEMPTS; attempt++)
                {
                    channelHandle = provider.CreateStream(this.Output, playlistItem);
                    if (channelHandle != 0)
                    {
                        Logger.Write(this, LogLevel.Debug, "Created stream from file {0}: {1}", playlistItem.FileName, channelHandle);
                        return true;
                    }
                    else
                    {
                        if (Bass.LastError == Errors.Already)
                        {
                            if (!immidiate || !this.FreeActiveStreams())
                            {
                                channelHandle = 0;
                                return false;
                            }
                        }
                    }
                    Thread.Sleep(CREATE_ATTEMPT_INTERVAL);
                }
                Logger.Write(this, LogLevel.Warn, "The bass stream provider failed.");
            }
            channelHandle = 0;
            return false;
        }

        protected virtual bool FreeActiveStreams()
        {
            var streams = BassOutputStream.ActiveStreams.ToArray();
            foreach (var stream in streams)
            {
                try
                {
                    stream.Dispose();
                }
                catch
                {
                    //Nothing can be done.
                }
            }
            return BassOutputStream.ActiveStreams.Count == 0;
        }

        /// <summary>
        /// We allow duplicate priorities, the order of duplicates is undefined.
        /// </summary>
        private class PriorityComparer : IComparer<byte>
        {
            public int Compare(byte x, byte y)
            {
                var result = x.CompareTo(y);
                if (result == 0)
                {
                    return 1;
                }
                return result;
            }
        }
    }
}
