using FoxDb.Interfaces;

namespace FoxTunes.Interfaces
{
    public interface IDatabaseSets : IBaseComponent
    {
        IDatabaseSet<PlaylistItem> PlaylistItem { get; }

        IDatabaseSet<PlaylistColumn> PlaylistColumn { get; }

        IDatabaseSet<LibraryItem> LibraryItem { get; }

        IDatabaseSet<LibraryHierarchy> LibraryHierarchy { get; }

        IDatabaseSet<LibraryHierarchyLevel> LibraryHierarchyLevel { get; }
    }
}