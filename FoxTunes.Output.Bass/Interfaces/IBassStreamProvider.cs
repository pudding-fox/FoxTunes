using ManagedBass;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IBassStreamProvider : IBaseComponent, IDisposable
    {
        byte Priority { get; }

        BassStreamProviderFlags Flags { get; }

        bool CanCreateStream(PlaylistItem playlistItem);

        Task<IBassStream> CreateStream(PlaylistItem playlistItem, IEnumerable<IBassStreamAdvice> advice);

        Task<IBassStream> CreateStream(PlaylistItem playlistItem, BassFlags flags, IEnumerable<IBassStreamAdvice> advice);

        void FreeStream(PlaylistItem playlistItem, int channelHandle);
    }

    [Flags]
    public enum BassStreamProviderFlags : byte
    {
        None = 0,
        Serial = 1
    }
}
