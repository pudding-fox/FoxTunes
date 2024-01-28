namespace FoxTunes
{
    public class BassEncoderFormat : IBassEncoderFormat
    {
        public BassEncoderFormat(int depth)
        {
            this.Depth = depth;
        }

        public bool AutoDepth
        {
            get
            {
                return this.Depth == 0;
            }
        }

        public int Depth { get; private set; }
    }
}
