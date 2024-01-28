using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IBassStreamFactory : IBaseComponent
    {
        IEnumerable<IBassStreamProvider> Providers { get; }

        void Register(IBassStreamProvider provider);

        Task<IBassStream> CreateStream(PlaylistItem playlistItem, bool immidiate);
    }
}
