namespace FoxTunes
{
    public class BassEncoderFormat : IBassEncoderFormat
    {
        public BassEncoderFormat(int depth, params int[] sampleRates)
        {
            this.Depth = depth;
            this.SampleRates = sampleRates;
        }

        public bool AutoDepth
        {
            get
            {
                return this.Depth == 0;
            }
        }

        public int Depth { get; private set; }

        public int[] SampleRates { get; private set; }
    }
}
