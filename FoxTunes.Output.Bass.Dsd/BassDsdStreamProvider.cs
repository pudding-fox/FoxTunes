using FoxDb;
using FoxTunes;
using FoxTunes.Interfaces;
using ManagedBass;
using ManagedBass.Dsd;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes
{
    //This component does not technically require an output but we don't want to present anything else with DSD data.
    [ComponentDependency(Slot = ComponentSlots.Output)]
    public class BassDsdStreamProvider : BassStreamProvider
    {
        public static readonly string[] EXTENSIONS = new[]
        {
            "dsd",
            "dsf"
        };

        public BassDsdStreamProviderBehaviour Behaviour { get; private set; }

        public IBassStreamPipelineFactory BassStreamPipelineFactory { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Behaviour = ComponentRegistry.Instance.GetComponent<BassDsdStreamProviderBehaviour>();
            this.BassStreamPipelineFactory = ComponentRegistry.Instance.GetComponent<IBassStreamPipelineFactory>();
            base.InitializeComponent(core);
        }

        public override bool CanCreateStream(PlaylistItem playlistItem)
        {
            //Unfortunately there's no way to determine whether DSD direct is enabled.
            //if (this.Behaviour == null || !this.Behaviour.Enabled)
            //{
            //    return false;
            //}
            if (!EXTENSIONS.Contains(playlistItem.FileName.GetExtension(), StringComparer.OrdinalIgnoreCase))
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

        public override IBassStream CreateBasicStream(PlaylistItem playlistItem, IEnumerable<IBassStreamAdvice> advice, BassFlags flags)
        {
            flags |= BassFlags.DSDRaw;
            var fileName = this.GetFileName(playlistItem, advice);
            var channelHandle = BassDsd.CreateStream(fileName, 0, 0, flags);
            if (channelHandle != 0 && !this.IsFormatSupported(playlistItem, channelHandle))
            {
                this.FreeStream(playlistItem, channelHandle);
                channelHandle = 0;
            }
            return this.CreateInteractiveStream(channelHandle, advice, flags);
        }

        public override IBassStream CreateInteractiveStream(PlaylistItem playlistItem, IEnumerable<IBassStreamAdvice> advice, BassFlags flags)
        {
            flags |= BassFlags.DSDRaw;
            var fileName = this.GetFileName(playlistItem, advice);
            var channelHandle = default(int);
            if (this.Output != null && this.Output.PlayFromMemory)
            {
                channelHandle = BassDsdInMemoryHandler.CreateStream(fileName, 0, 0, flags);
                if (channelHandle == 0)
                {
                    Logger.Write(this, LogLevel.Warn, "Failed to load file into memory: {0}", fileName);
                    channelHandle = BassDsd.CreateStream(fileName, 0, 0, flags);
                }
            }
            else
            {
                channelHandle = BassDsd.CreateStream(fileName, 0, 0, flags);
            }
            if (channelHandle != 0 && !this.IsFormatSupported(playlistItem, channelHandle))
            {
                this.FreeStream(playlistItem, channelHandle);
                channelHandle = 0;
            }
            return this.CreateInteractiveStream(channelHandle, advice, flags);
        }

        protected virtual bool IsFormatSupported(PlaylistItem playlistItem, int channelHandle)
        {
            var query = this.BassStreamPipelineFactory.QueryPipeline();
            var channels = BassUtils.GetChannelCount(channelHandle);
            var rate = BassUtils.GetChannelDsdRate(channelHandle);
            if (query.OutputChannels < channels || !query.OutputRates.Contains(rate))
            {
                Logger.Write(this, LogLevel.Warn, "DSD format {0}:{1} is unsupported, the stream will be unloaded. This warning is expensive, please don't attempt to play unsupported DSD.", rate, channels);
                return false;
            }
            return true;
        }
    }
}
