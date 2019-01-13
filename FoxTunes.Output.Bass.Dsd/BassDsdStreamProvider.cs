using FoxTunes.Interfaces;
using ManagedBass;
using ManagedBass.Dsd;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class BassDsdStreamProvider : BassStreamProvider
    {
        public IBassStreamPipelineFactory BassStreamPipelineFactory { get; private set; }

        public override byte Priority
        {
            get
            {
                return PRIORITY_HIGH;
            }
        }

        public override void InitializeComponent(ICore core)
        {
            this.BassStreamPipelineFactory = ComponentRegistry.Instance.GetComponent<IBassStreamPipelineFactory>();
            base.InitializeComponent(core);
        }

        public override bool CanCreateStream(IBassOutput output, PlaylistItem playlistItem)
        {
            if (!new[]
            {
                "dsd",
                "dsf"
            }.Contains(playlistItem.FileName.GetExtension(), StringComparer.OrdinalIgnoreCase))
            {
                return false;
            }
            var query = this.BassStreamPipelineFactory.QueryPipeline();
            if ((query.OutputCapabilities & BassCapability.DSD_RAW) != BassCapability.DSD_RAW)
            {
                return false;
            }
            return true;
        }

        public override async Task<int> CreateStream(IBassOutput output, PlaylistItem playlistItem)
        {
            var flags = BassFlags.Decode | BassFlags.DSDRaw;
            var channelHandle = default(int);
            await this.Semaphore.WaitAsync();
            try
            {
                if (output.PlayFromMemory)
                {
                    var buffer = await this.GetBuffer(playlistItem);
                    channelHandle = BassDsd.CreateStream(buffer, 0, buffer.Length, flags);
                    if (channelHandle != 0)
                    {
                        if (!this.Streams.TryAdd(new BassStreamProviderKey(playlistItem.FileName, channelHandle), buffer))
                        {
                            Logger.Write(this, LogLevel.Warn, "Failed to pin handle of buffer for file \"{0}\". Playback may fail.", playlistItem.FileName);
                        }
                    }
                }
                else
                {
                    channelHandle = BassDsd.CreateStream(playlistItem.FileName, 0, 0, flags);
                }
                if (channelHandle != 0)
                {
                    var query = this.BassStreamPipelineFactory.QueryPipeline();
                    var channels = BassUtils.GetChannelCount(channelHandle);
                    var rate = BassUtils.GetChannelDsdRate(channelHandle);
                    if (query.OutputChannels < channels || !query.OutputRates.Contains(rate))
                    {
                        Logger.Write(this, LogLevel.Warn, "DSD format {0}:{1} is unsupported, the stream will be unloaded. This warning is expensive, please don't attempt to play unsupported DSD.", rate, channels);
                        this.FreeStream(playlistItem, channelHandle);
                        channelHandle = 0;
                    }
                }
                return channelHandle;
            }
            finally
            {
                this.Semaphore.Release();
            }
        }
    }
}
