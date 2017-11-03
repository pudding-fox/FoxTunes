namespace FoxTunes.Interfaces
{
    public interface ICoreSQL : IBaseComponent
    {
        string AddImageItems { get; }

        string AddLibraryHierarchyNodeToPlaylist { get; }

        string AddLibraryHierarchyRecord { get; }

        string AddPlaylistSequenceRecord { get; }

        string AddLibraryItem { get; }

        string AddMetaDataItems { get; }

        string AddPlaylistItem { get; }

        string AddPropertyItems { get; }

        string ClearPlaylist { get; }

        string CopyMetaDataItems { get; }

        string GetLibraryHierarchyMetaDataItems { get; }

        string GetLibraryHierarchyNodes { get; }

        string GetLibraryHierarchyNodesWithFilter { get; }

        string SetLibraryItemStatus { get; }

        string SetPlaylistItemStatus { get; }

        string ShiftPlaylistItems { get; }

        string VariousArtists { get; }
    }
}
