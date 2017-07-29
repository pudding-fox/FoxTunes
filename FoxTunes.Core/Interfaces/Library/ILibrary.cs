namespace FoxTunes.Interfaces
{
    public interface ILibrary : IBaseComponent
    {
        IDatabaseQuery<LibraryItem> LibraryItemQuery { get; }

        IDatabaseSet<LibraryItem> LibraryItemSet { get; }

        IDatabaseQuery<LibraryHierarchy> LibraryHierarchyQuery { get; }

        IDatabaseSet<LibraryHierarchy> LibraryHierarchySet { get; }
    }
}
