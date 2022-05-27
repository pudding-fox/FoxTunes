using ManagedBass;
using System;

namespace FoxTunes.Interfaces
{
    public interface IBassStreamComponent : IBaseComponent, IInitializable, IDisposable
    {
        string Name { get; }

        string Description { get; }

        int ChannelHandle { get; }

        BassFlags Flags { get; }

        long BufferLength { get; }

        bool IsActive { get; }

        bool GetFormat(out int rate, out int channels, out BassFlags flags);

        void Connect(IBassStreamComponent previous);

        void ClearBuffer();

        event EventHandler Invalidate;
    }
}
