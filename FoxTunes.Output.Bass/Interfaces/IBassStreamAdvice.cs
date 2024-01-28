using System;

namespace FoxTunes.Interfaces
{
    public interface IBassStreamAdvice : IBaseComponent
    {
        string FileName { get; }

        TimeSpan Offset { get; }

        TimeSpan Length { get; }

        bool Wrap(IBassStreamProvider provider, int channelHandle, out IBassStream stream);
    }
}
