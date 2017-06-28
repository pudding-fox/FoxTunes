namespace FoxTunes.Interfaces
{
    public interface IStandardFactories
    {
        IPlaylistItemFactory PlaylistItem { get; }

        IMetaDataSourceFactory MetaDataSource { get; }
    }
}
