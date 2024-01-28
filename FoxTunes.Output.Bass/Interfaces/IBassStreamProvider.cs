using System;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IBassStreamProvider : IDisposable
    {
        byte Priority { get; }

        bool CanCreateStream(PlaylistItem playlistItem);

        Task<int> CreateStream(PlaylistItem playlistItem);

        void FreeStream(PlaylistItem playlistItem, int channelHandle);
    }
}
