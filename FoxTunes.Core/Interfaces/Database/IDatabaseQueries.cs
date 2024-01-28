using FoxDb.Interfaces;
using System.Collections.Generic;

namespace FoxTunes.Interfaces
{
    public interface IDatabaseQueries : IBaseComponent
    {
        IDatabaseQuery AddLibraryHierarchyNodeToPlaylist { get; }

        IDatabaseQuery AddPlaylistSequenceRecord { get; }

        IDatabaseQuery AddLibraryMetaDataItems { get; }

        IDatabaseQuery AddPlaylistMetaDataItems { get; }

        IDatabaseQuery GetLibraryHierarchyMetaDataItems { get; }

        IDatabaseQuery GetLibraryHierarchyNodes { get; }

        IDatabaseQuery GetLibraryHierarchyNodesWithFilter { get; }

        IDatabaseQuery VariousArtists { get; }

        IDatabaseQuery PlaylistSequenceBuilder(IEnumerable<string> metaDataNames);

        IDatabaseQuery LibraryHierarchyBuilder(IEnumerable<string> metaDataNames);
    }
}
