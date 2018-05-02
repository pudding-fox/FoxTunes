using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IBassStreamFactory : IBaseComponent
    {
        void Register(IBassStreamProvider provider);

        bool CreateStream(PlaylistItem playlistItem, bool immidiate, out int channelHandle);
    }
}
