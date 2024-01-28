using FoxDb.Interfaces;

namespace FoxTunes.Interfaces
{
    public interface IDatabaseQueries : IBaseComponent
    {
        IDatabaseQuery AddLibraryHierarchyNode { get; }

        IDatabaseQuery AddLibraryHierarchyNodeToPlaylist { get; }

        IDatabaseQuery AddLibraryMetaDataItem { get; }

        IDatabaseQuery AddPlaylistMetaDataItem { get; }

        IDatabaseQuery ClearLibraryMetaDataItems { get; }

        IDatabaseQuery ClearPlaylistMetaDataItems { get; }

        IDatabaseQuery GetLibraryMetaData { get; }

        IDatabaseQuery GetLibraryHierarchyMetaData { get; }

        IDatabaseQuery GetLibraryHierarchyNodes { get; }

        IDatabaseQuery GetLibraryItems { get; }

        IDatabaseQuery GetOrAddMetaDataItem { get; }

        IDatabaseQuery MovePlaylistItem { get; }

        IDatabaseQuery RemoveLibraryHierarchyItems { get; }

        IDatabaseQuery RemoveLibraryItems { get; }

        IDatabaseQuery RemovePlaylistItems { get; }

        IDatabaseQuery SequencePlaylistItems { get; }

        IDatabaseQuery UpdateLibraryHierarchyNode { get; }

        IDatabaseQuery UpdateLibraryVariousArtists { get; }

        IDatabaseQuery UpdatePlaylistVariousArtists { get; }
    }
}
