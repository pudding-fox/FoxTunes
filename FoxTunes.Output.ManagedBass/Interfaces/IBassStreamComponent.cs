using ManagedBass;
using System;

namespace FoxTunes.Interfaces
{
    public interface IBassStreamComponent : IBaseComponent, IDisposable
    {
        string Name { get; }

        string Description { get; }

        int Rate { get; }

        int Channels { get; }

        BassFlags Flags { get; }

        int ChannelHandle { get; }

        long BufferLength { get; }

        void Connect(IBassStreamComponent previous);

        void ClearBuffer();

        event EventHandler Invalidate;
    }
}
