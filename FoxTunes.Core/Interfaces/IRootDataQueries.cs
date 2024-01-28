namespace FoxTunes.Interfaces
{
    public interface IRootDataQueries
    {
        IDatabaseQuery<PlaylistItem> PlaylistItem { get; }

        IDatabaseQuery<PlaylistColumn> PlaylistColumn { get; }

        IDatabaseQuery<LibraryItem> LibraryItem { get; }

        IDatabaseQuery<LibraryHierarchy> LibraryHierarchy { get; }
    }
}
