using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IArtworkProvider : IStandardComponent
    {
        Task<MetaDataItem> Find(PlaylistItem playlistItem, ArtworkType type);

        Task<MetaDataItem> Find(string path, ArtworkType type);
    }
}
