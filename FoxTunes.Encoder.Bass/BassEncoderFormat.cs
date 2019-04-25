using System;

namespace FoxTunes
{
    public class BassEncoderFormat : MarshalByRefObject, IBassEncoderFormat
    {
        public const int DEFAULT_DEPTH = 16;

        public int Depth
        {
            get
            {
                return DEFAULT_DEPTH;
            }
        }
    }
}
