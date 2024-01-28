using FoxTunes.Interfaces;
using ManagedBass;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes
{
    public class BassStreamFactory : StandardComponent, IBassStreamFactory
    {
        public BassStreamFactory()
        {
            this.Advisors = new List<IBassStreamAdvisor>();
            this.Providers = new List<IBassStreamProvider>();
        }

        private List<IBassStreamAdvisor> Advisors { get; set; }

        private List<IBassStreamProvider> Providers { get; set; }

        IEnumerable<IBassStreamAdvisor> IBassStreamFactory.Advisors
        {
            get
            {
                return this.Advisors;
            }
        }

        IEnumerable<IBassStreamProvider> IBassStreamFactory.Providers
        {
            get
            {
                return this.Providers;
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

        public void Register(IBassStreamAdvisor advisor)
        {
            this.Advisors.Add(advisor);
            Logger.Write(this, LogLevel.Debug, "Registered bass stream advisor \"{0}\".", advisor.GetType().Name);
        }

        public void Register(IBassStreamProvider provider)
        {
            this.Providers.Add(provider);
            Logger.Write(this, LogLevel.Debug, "Registered bass stream provider \"{0}\".", provider.GetType().Name);
        }

        public IEnumerable<IBassStreamAdvice> GetAdvice(IBassStreamProvider provider, PlaylistItem playlistItem)
        {
            var advice = new List<IBassStreamAdvice>();
            foreach (var advisor in this.Advisors)
            {
                advisor.Advise(provider, playlistItem, advice);
            }
            return advice.ToArray();
        }

        public IEnumerable<IBassStreamProvider> GetProviders(PlaylistItem playlistItem)
        {
            return this.Providers.Where(
                provider => provider.CanCreateStream(playlistItem)
            ).ToArray();
        }

        public IBassStream CreateBasicStream(PlaylistItem playlistItem, BassFlags flags)
        {
            flags |= BassFlags.Decode;
            Logger.Write(this, LogLevel.Debug, "Attempting to create stream for playlist item: {0} => {1}", playlistItem.Id, playlistItem.FileName);
            var providers = this.GetProviders(playlistItem);
            foreach (var provider in providers)
            {
                Logger.Write(this, LogLevel.Debug, "Using bass stream provider \"{0}\".", provider.GetType().Name);
                var advice = this.GetAdvice(provider, playlistItem).ToArray();
                var stream = provider.CreateBasicStream(playlistItem, advice, flags);
                if (stream.ChannelHandle != 0)
                {
                    Logger.Write(this, LogLevel.Debug, "Created stream from file {0}: {1}", playlistItem.FileName, stream.ChannelHandle);
                    return stream;
                }
                Logger.Write(this, LogLevel.Debug, "Failed to create stream from file {0}: {1}", playlistItem.FileName, Enum.GetName(typeof(Errors), Bass.LastError));
                return BassStream.Error(Bass.LastError);
            }
            return BassStream.Empty;
        }

        public IBassStream CreateInteractiveStream(PlaylistItem playlistItem, bool immidiate, BassFlags flags)
        {
            flags |= BassFlags.Decode;
            if (this.Output != null && this.Output.Float)
            {
                flags |= BassFlags.Float;
            }
            Logger.Write(this, LogLevel.Debug, "Attempting to create stream for playlist item: {0} => {1}", playlistItem.Id, playlistItem.FileName);
            var providers = this.GetProviders(playlistItem);
            foreach (var provider in providers)
            {
                Logger.Write(this, LogLevel.Debug, "Using bass stream provider \"{0}\".", provider.GetType().Name);
                var advice = this.GetAdvice(provider, playlistItem).ToArray();
                //We will try twice if we get BASS_ERROR_ALREADY.
                for (var a = 0; a < 2; a++)
                {
                    var stream = provider.CreateInteractiveStream(playlistItem, advice, flags);
                    if (stream.ChannelHandle != 0)
                    {
                        Logger.Write(this, LogLevel.Debug, "Created stream from file {0}: {1}", playlistItem.FileName, stream.ChannelHandle);
                        return stream;
                    }
                    if (Bass.LastError == Errors.Already)
                    {
                        //This will happen when using a CD player.
                        //If immidiate playback was requested we need to free any active streams and try again. 
                        if (immidiate)
                        {
                            Logger.Write(this, LogLevel.Debug, "Device is in use (probably a CD player), releasing active streams.");
                            if (BassOutputStreams.Clear())
                            {
                                Logger.Write(this, LogLevel.Debug, "Active streams were released, retrying.");
                                continue;
                            }
                            else
                            {
                                Logger.Write(this, LogLevel.Debug, "Failed to release active streams.");
                            }
                        }
                    }
                    Logger.Write(this, LogLevel.Debug, "Failed to create stream from file {0}: {1}", playlistItem.FileName, Enum.GetName(typeof(Errors), Bass.LastError));
                    return BassStream.Error(Bass.LastError);
                }
            }
            return BassStream.Empty;
        }
    }
}
