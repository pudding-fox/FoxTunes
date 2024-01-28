namespace FoxTunes.Interfaces
{
    public interface IRootDataSets
    {
        IDatabaseSet<PlaylistItem> PlaylistItem { get; }

        IDatabaseSet<PlaylistColumn> PlaylistColumn { get; }

        IDatabaseSet<LibraryItem> LibraryItem { get; }

        IDatabaseSet<LibraryHierarchy> LibraryHierarchy { get; }
    }
}
