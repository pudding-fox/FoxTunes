using FoxTunes.Interfaces;
using ManagedBass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class BassStreamFactory : StandardComponent, IBassStreamFactory
    {
        public BassStreamFactory()
        {
            this.Semaphore = new SemaphoreSlim(1, 1);
            this.Advisors = new SortedList<byte, IBassStreamAdvisor>(new PriorityComparer());
            this.Providers = new SortedList<byte, IBassStreamProvider>(new PriorityComparer());
        }

        public SemaphoreSlim Semaphore { get; private set; }

        private SortedList<byte, IBassStreamAdvisor> Advisors { get; set; }

        private SortedList<byte, IBassStreamProvider> Providers { get; set; }

        IEnumerable<IBassStreamAdvisor> IBassStreamFactory.Advisors
        {
            get
            {
                return this.Advisors.Values;
            }
        }

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

        public void Register(IBassStreamAdvisor advisor)
        {
            this.Advisors.Add(advisor.Priority, advisor);
            Logger.Write(this, LogLevel.Debug, "Registered bass stream advisor with priority {0}: {1}", advisor.Priority, advisor.GetType().Name);
        }

        public void Register(IBassStreamProvider provider)
        {
            this.Providers.Add(provider.Priority, provider);
            Logger.Write(this, LogLevel.Debug, "Registered bass stream provider with priority {0}: {1}", provider.Priority, provider.GetType().Name);
        }

        public IEnumerable<IBassStreamAdvice> GetAdvice(IBassStreamProvider provider, PlaylistItem playlistItem)
        {
            foreach (var advisor in this.Advisors.Values)
            {
                var advice = default(IBassStreamAdvice);
                if (advisor.Advice(provider, playlistItem, out advice))
                {
                    yield return advice;
                }
            }
        }

        public IEnumerable<IBassStreamProvider> GetProviders(PlaylistItem playlistItem)
        {
            return this.Providers.Values.Where(provider => provider.CanCreateStream(playlistItem));
        }

        public async Task<IBassStream> CreateStream(PlaylistItem playlistItem, bool immidiate)
        {
            Logger.Write(this, LogLevel.Debug, "Attempting to create stream for playlist item: {0} => {1}", playlistItem.Id, playlistItem.FileName);
#if NET40
            this.Semaphore.Wait();
#else
            await this.Semaphore.WaitAsync().ConfigureAwait(false);
#endif
            try
            {
                var providers = this.GetProviders(playlistItem).ToArray();
                foreach (var provider in providers)
                {
                    var advice = this.GetAdvice(provider, playlistItem).ToArray();
                    //We will try twice if we get BASS_ERROR_ALREADY.
                    for (var a = 0; a < 2; a++)
                    {
                        Logger.Write(this, LogLevel.Debug, "Using bass stream provider with priority {0}: {1}", provider.Priority, provider.GetType().Name);
                        var stream = await provider.CreateStream(playlistItem, advice).ConfigureAwait(false);
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
                        break;
                    }
                }
            }
            finally
            {
                this.Semaphore.Release();
            }
            return BassStream.Empty;
        }

        public async Task<IBassStream> CreateStream(PlaylistItem playlistItem, bool immidiate, BassFlags flags)
        {
#if NET40
            this.Semaphore.Wait();
#else
            await this.Semaphore.WaitAsync().ConfigureAwait(false);
#endif
            try
            {
                var providers = this.GetProviders(playlistItem).ToArray();
                foreach (var provider in providers)
                {
                    var advice = this.GetAdvice(provider, playlistItem).ToArray();
                    var stream = await provider.CreateStream(playlistItem, flags, advice).ConfigureAwait(false);
                    if (stream.ChannelHandle != 0)
                    {
                        return stream;
                    }
                    else
                    {
                        return BassStream.Error(Bass.LastError);
                    }
                }
            }
            finally
            {
                this.Semaphore.Release();
            }
            return BassStream.Empty;
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
