using ManagedBass;
using System;

namespace FoxTunes.Interfaces
{
    public interface IBassStreamComponent : IDisposable
    {
        int Rate { get; }

        int Depth { get; }

        int Channels { get; }

        BassFlags Flags { get; }

        int ChannelHandle { get; }

        long BufferLength { get; }

        void Connect(IBassStreamComponent previous);

        void ClearBuffer();

        event EventHandler Invalidate;
    }
}
