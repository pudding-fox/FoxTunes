using System;

namespace FoxTunes
{
    public interface IBassEncoder
    {
        AppDomain Domain { get; }

        void Encode(string[] fileNames, IBassEncoderSettings settings);
    }
}
