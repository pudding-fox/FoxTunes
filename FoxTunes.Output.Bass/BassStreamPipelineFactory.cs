using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.Output)]
    public class BassStreamPipelineFactory : StandardComponent, IBassStreamPipelineFactory
    {
        protected virtual void OnQueryingPipeline(QueryingPipelineEventArgs e)
        {
            if (this.QueryingPipeline == null)
            {
                return;
            }
            this.QueryingPipeline(this, e);
        }

        public event QueryingPipelineEventHandler QueryingPipeline;

        protected virtual void OnCreatingPipeline(CreatingPipelineEventArgs e)
        {
            if (this.CreatingPipeline == null)
            {
                return;
            }
            this.CreatingPipeline(this, e);
        }

        public event CreatingPipelineEventHandler CreatingPipeline;

        public IBassStreamPipelineQueryResult QueryPipeline()
        {
            var e = new QueryingPipelineEventArgs();
            this.OnQueryingPipeline(e);
            return new BassStreamPipelineQueryResult(e.InputCapabilities, e.OutputCapabilities, e.OutputRates, e.OutputChannels);
        }

        public IBassStreamPipeline CreatePipeline(BassOutputStream stream)
        {
            var input = default(IBassStreamInput);
            var components = default(IEnumerable<IBassStreamComponent>);
            var output = default(IBassStreamOutput);
            this.CreatePipeline(stream, out input, out components, out output);
            var pipeline = new BassStreamPipeline(
                input,
                components,
                output
            );
            try
            {
                pipeline.Connect();
                if (Logger.IsDebugEnabled(this))
                {
                    if (components.Any())
                    {
                        Logger.Write(
                            this,
                            LogLevel.Debug,
                            "Connected pipeline: {0}",
                            string.Join(" => ", pipeline.All.Select(component => string.Format("\"{0}\"", component.GetType().Name)))
                        );
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Write(this, LogLevel.Error, "Failed to create pipeline: {0}", e.Message);
                pipeline.Dispose();
                throw;
            }
            return pipeline;
        }

        protected virtual void CreatePipeline(BassOutputStream stream, out IBassStreamInput input, out IEnumerable<IBassStreamComponent> components, out IBassStreamOutput output)
        {
            var e = new CreatingPipelineEventArgs(this.QueryPipeline(), stream);
            this.OnCreatingPipeline(e);
            input = e.Input;
            components = e.Components;
            output = e.Output;
            if (input == null)
            {
                throw new NotImplementedException("Failed to locate a suitable pipeline input.");
            }
            if (output == null)
            {
                throw new NotImplementedException("Failed to locate a suitable pipeline output.");
            }
        }
    }
}
