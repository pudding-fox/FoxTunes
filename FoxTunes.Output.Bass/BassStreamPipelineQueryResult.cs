using FoxTunes.Interfaces;
using System.Collections.Generic;

namespace FoxTunes
{
    public class BassStreamPipelineQueryResult : IBassStreamPipelineQueryResult
    {
        public BassStreamPipelineQueryResult(long inputCapabilities, long outputCapabilities, IEnumerable<int> outputRates, int outputChannels)
        {
            this.InputCapabilities = inputCapabilities;
            this.OutputCapabilities = outputCapabilities;
            this.OutputRates = outputRates;
            this.OutputChannels = outputChannels;
        }

        public long InputCapabilities { get; private set; }

        public long OutputCapabilities { get; private set; }

        public IEnumerable<int> OutputRates { get; private set; }

        public int OutputChannels { get; private set; }
    }
}
