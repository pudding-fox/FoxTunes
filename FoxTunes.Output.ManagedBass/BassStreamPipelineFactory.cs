using FoxTunes.Interfaces;
using ManagedBass;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes
{
    public class BassStreamPipelineFactory : BaseComponent, IBassStreamPipelineFactory
    {
        public BassStreamPipelineFactory(IBassOutput output)
        {
            this.Output = output;
        }

        public IBassOutput Output { get; private set; }

        public IBassStreamPipeline CreatePipeline(bool dsd, int rate, int channels)
        {
            var pipeline = new BassStreamPipeline(
                this.GetInput(dsd, rate, channels),
                this.GetComponents(dsd, rate, channels).ToArray(),
                this.GetOutput(dsd, rate, channels)
            );
            try
            {
                pipeline.Connect();
            }
            catch (Exception e)
            {
                Logger.Write(this, LogLevel.Error, "Failed to create pipeline: {0}", e.Message);
                pipeline.Dispose();
                throw;
            }
            return pipeline;
        }

        protected virtual IBassStreamInput GetInput(bool dsd, int rate, int channels)
        {
            var flags = this.Output.Flags;
            if (dsd)
            {
                flags |= BassFlags.DSDRaw;
            }
            return new BassGaplessStreamInput(rate, channels, flags);
        }

        protected virtual IEnumerable<IBassStreamComponent> GetComponents(bool dsd, int rate, int channels)
        {
            if (!dsd && this.Output.Resampler)
            {
                if (this.Output.EnforceRate && this.Output.Rate != rate)
                {
                    yield return new BassResamplerStreamComponent(this.Output.Rate, channels, this.Output.Flags);
                }
            }
        }

        protected virtual IBassStreamOutput GetOutput(bool dsd, int rate, int channels)
        {
            var flags = this.Output.Flags;
            if (dsd)
            {
                flags |= BassFlags.DSDRaw;
            }
            else if (this.Output.EnforceRate && this.Output.Rate != rate)
            {
                rate = this.Output.Rate;
            }
            switch (this.Output.Mode)
            {
                case BassOutputMode.DirectSound:
                    return new BassDefaultStreamOutput(rate, channels, flags);
                case BassOutputMode.ASIO:
                    return new BassAsioStreamOutput(this.Output.AsioDevice, rate, channels, flags);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
