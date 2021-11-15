namespace FoxTunes.Interfaces
{
    public interface IStandardManagers
    {
        IPlaybackManager Playback { get; }

        IPlaylistManager Playlist { get; }

        IPlaylistColumnManager PlaylistColumn { get; }

        ILibraryManager Library { get; }

        IHierarchyManager Hierarchy { get; }

        IMetaDataManager MetaData { get; }

        IFileActionHandlerManager FileActionHandler { get; }
    }
}
