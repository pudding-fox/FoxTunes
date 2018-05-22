using System.Collections.Generic;

namespace FoxTunes.Interfaces
{
    public interface IBassStreamFactory : IBaseComponent
    {
        IEnumerable<IBassStreamProvider> Providers { get; }

        void Register(IBassStreamProvider provider);

        bool CreateStream(PlaylistItem playlistItem, bool immidiate, out int channelHandle);
    }
}
