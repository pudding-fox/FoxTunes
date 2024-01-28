using FoxTunes.Interfaces;
using ManagedBass;
using ManagedBass.Dsd;
using System;
using System.Linq;

namespace FoxTunes
{
    public class BassDsdStreamProvider : BassStreamProvider
    {
        public override byte Priority
        {
            get
            {
                return PRIORITY_HIGH;
            }
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
            var query = ComponentRegistry.Instance.GetComponent<IBassStreamPipelineFactory>().QueryPipeline();
            if ((query.OutputCapabilities & BassCapability.DSD_RAW) != BassCapability.DSD_RAW)
            {
                return false;
            }
            return true;
        }

        public override int CreateStream(IBassOutput output, PlaylistItem playlistItem)
        {
            var flags = BassFlags.Decode | BassFlags.DSDRaw;
            var channelHandle = BassDsd.CreateStream(playlistItem.FileName, 0, 0, flags);
            if (channelHandle != 0)
            {
                var query = ComponentRegistry.Instance.GetComponent<IBassStreamPipelineFactory>().QueryPipeline();
                var channels = BassUtils.GetChannelCount(channelHandle);
                var rate = BassUtils.GetChannelDsdRate(channelHandle);
                if (query.OutputChannels < channels || !query.OutputRates.Contains(rate))
                {
                    Logger.Write(this, LogLevel.Warn, "DSD format {0}:{1} is unsupported, the stream will be unloaded. This warning is expensive, please don't attempt to play unsupported DSD.", rate, channels);
                    output.FreeStream(channelHandle);
                    channelHandle = 0;
                }
            }
            return channelHandle;
        }
    }
}
