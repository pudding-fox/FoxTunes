using ManagedBass;
using System.Collections.Generic;

namespace FoxTunes.Interfaces
{
    public interface IBassStreamFactory : IBaseComponent
    {
        IEnumerable<IBassStreamAdvisor> Advisors { get; }

        IEnumerable<IBassStreamProvider> Providers { get; }

        void Register(IBassStreamAdvisor advisor);

        void Register(IBassStreamProvider provider);

        IBassStream CreateBasicStream(PlaylistItem playlistItem, BassFlags flags);

        IBassStream CreateInteractiveStream(PlaylistItem playlistItem, bool immidiate, BassFlags flags);

        bool HasActiveStream(string fileName);

        bool ReleaseActiveStreams();
    }
}
