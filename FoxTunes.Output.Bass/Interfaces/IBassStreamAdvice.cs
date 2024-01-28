using System;

namespace FoxTunes.Interfaces
{
    public interface IBassStreamAdvice : IBaseComponent
    {
        string FileName { get; }

        bool Wrap(IBassStreamProvider provider, int channelHandle, out IBassStream stream);
    }
}
