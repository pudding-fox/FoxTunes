using ManagedBass;
using System;

namespace FoxTunes.Interfaces
{
    public interface IBassStreamComponent : IBaseComponent, IInitializable, IDisposable
    {
        string Name { get; }

        string Description { get; }

        int Rate { get; }

        int Channels { get; }

        BassFlags Flags { get; }

        int ChannelHandle { get; }

        long BufferLength { get; }

        bool IsActive { get; }

        void Connect(IBassStreamComponent previous);

        void ClearBuffer();

        event EventHandler Invalidate;
    }
}
