using System;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IBassStreamProvider : IDisposable
    {
        byte Priority { get; }

        bool CanCreateStream(IBassOutput output, PlaylistItem playlistItem);

        Task<int> CreateStream(IBassOutput output, PlaylistItem playlistItem);

        void FreeStream(int channelHandle);
    }
}
