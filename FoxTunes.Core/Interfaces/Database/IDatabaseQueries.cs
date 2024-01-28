using System.Collections.Generic;

namespace FoxTunes.Interfaces
{
    public interface IDatabaseQueries : IBaseComponent
    {
        IDatabaseQuery Select<T>(params string[] filters);

        IDatabaseQuery Find<T>();

        IDatabaseQuery Count<T>();

        IDatabaseQuery Insert<T>();

        IDatabaseQuery Update<T>();

        IDatabaseQuery Delete<T>();

        IDatabaseQuery AddLibraryHierarchyNodeToPlaylist { get; }

        IDatabaseQuery AddLibraryHierarchyRecord { get; }

        IDatabaseQuery AddPlaylistSequenceRecord { get; }

        IDatabaseQuery AddLibraryItem { get; }

        IDatabaseQuery AddLibraryMetaDataItems { get; }

        IDatabaseQuery AddPlaylistItem { get; }

        IDatabaseQuery AddPlaylistMetaDataItems { get; }

        IDatabaseQuery ClearPlaylist { get; }

        IDatabaseQuery CopyMetaDataItems { get; }

        IDatabaseQuery GetPlaylistItemsWithoutMetaData { get; }

        IDatabaseQuery GetLibraryItems { get; }

        IDatabaseQuery GetLibraryHierarchyMetaDataItems { get; }

        IDatabaseQuery GetPlaylistMetaDataItems { get; }

        IDatabaseQuery GetLibraryHierarchyNodes { get; }

        IDatabaseQuery GetLibraryHierarchyNodesWithFilter { get; }

        IDatabaseQuery SetLibraryItemStatus { get; }

        IDatabaseQuery SetPlaylistItemStatus { get; }

        IDatabaseQuery ShiftPlaylistItems { get; }

        IDatabaseQuery VariousArtists { get; }

        IDatabaseQuery GetFirstPlaylistItem { get; }

        IDatabaseQuery GetLastPlaylistItem { get; }

        IDatabaseQuery GetNextPlaylistItem { get; }

        IDatabaseQuery GetPreviousPlaylistItem { get; }

        IDatabaseQuery PlaylistSequenceBuilder(IEnumerable<string> metaDataNames);

        IDatabaseQuery LibraryHierarchyBuilder(IEnumerable<string> metaDataNames);

        IDatabaseQuery GetMetaDataNames { get; }
    }
}
