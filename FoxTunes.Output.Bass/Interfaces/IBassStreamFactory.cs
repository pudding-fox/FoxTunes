using ManagedBass;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IBassStreamFactory : IBaseComponent
    {
        IEnumerable<IBassStreamAdvisor> Advisors { get; }

        IEnumerable<IBassStreamProvider> Providers { get; }

        void Register(IBassStreamAdvisor advisor);

        void Register(IBassStreamProvider provider);

        Task<IBassStream> CreateStream(PlaylistItem playlistItem, bool immidiate);

        Task<IBassStream> CreateStream(PlaylistItem playlistItem, bool immidiate, BassFlags flags);
    }
}
