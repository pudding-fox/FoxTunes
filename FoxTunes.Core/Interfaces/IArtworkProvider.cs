namespace FoxTunes.Interfaces
{
    public interface IArtworkProvider : IStandardComponent
    {
        MetaDataItem Find(PlaylistItem playlistItem, ArtworkType type);

        MetaDataItem Find(string path, ArtworkType type);
    }
}
