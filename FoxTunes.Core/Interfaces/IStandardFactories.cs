namespace FoxTunes.Interfaces
{
    public interface IStandardFactories
    {
        IPlaylistItemFactory PlaylistItem { get; }

        ILibraryItemFactory LibraryItem { get; }

        IMetaDataSourceFactory MetaDataSource { get; }
    }
}
