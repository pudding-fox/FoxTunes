using FoxDb.Interfaces;
using System.Collections.Generic;

namespace FoxTunes.Interfaces
{
    public interface IDatabaseQueries : IBaseComponent
    {
        IDatabaseQuery MovePlaylistItem { get; }

        IDatabaseQuery RemovePlaylistItems { get; }

        IDatabaseQuery RemoveLibraryHierarchyItems { get; }

        IDatabaseQuery RemoveLibraryItems { get; }

        IDatabaseQuery AddLibraryHierarchyNode { get; }

        IDatabaseQuery AddLibraryHierarchyNodeToPlaylist { get; }

        IDatabaseQuery AddPlaylistSequenceRecord { get; }

        IDatabaseQuery AddLibraryMetaDataItems { get; }

        IDatabaseQuery ClearLibraryMetaDataItems { get; }

        IDatabaseQuery AddPlaylistMetaDataItems { get; }

        IDatabaseQuery ClearPlaylistMetaDataItems { get; }

        IDatabaseQuery GetLibraryHierarchyMetaData { get; }

        IDatabaseQuery GetLibraryHierarchyNodes { get; }

        IDatabaseQuery GetLibraryHierarchyNodesWithFilter { get; }

        IDatabaseQuery UpdatePlaylistVariousArtists { get; }

        IDatabaseQuery UpdateLibraryVariousArtists { get; }

        IDatabaseQuery SequencePlaylistItems(IEnumerable<string> metaDataNames);

        IDatabaseQuery BuildLibraryHierarchies(IEnumerable<string> metaDataNames);
    }
}
