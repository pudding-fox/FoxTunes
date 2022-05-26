using ManagedBass;
using System.Collections.Generic;

namespace FoxTunes.Interfaces
{
    public interface IBassStreamFactory : IBaseComponent
    {
        IBassStream CreateBasicStream(PlaylistItem playlistItem, BassFlags flags);

        IBassStream CreateInteractiveStream(PlaylistItem playlistItem, bool immidiate, BassFlags flags);

        bool HasActiveStream(string fileName);

        bool ReleaseActiveStreams();
    }
}
