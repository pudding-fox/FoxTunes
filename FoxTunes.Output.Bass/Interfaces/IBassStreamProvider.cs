using ManagedBass;
using System;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IBassStreamProvider : IDisposable
    {
        byte Priority { get; }

        bool CanCreateStream(PlaylistItem playlistItem);

        Task<int> CreateStream(PlaylistItem playlistItem);

        Task<int> CreateStream(PlaylistItem playlistItem, BassFlags flags);

        void FreeStream(PlaylistItem playlistItem, int channelHandle);
    }
}
