using System;

namespace FoxTunes
{
    public interface IBassEncoder
    {
        AppDomain Domain { get; }

        void Encode(EncoderItem[] encoderItems);

        void Cancel();
    }
}
