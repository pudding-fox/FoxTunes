using FoxDb.Interfaces;

namespace FoxTunes.Interfaces
{
    public interface IDatabaseTables : IBaseComponent
    {
        ITableConfig MetaDataItem { get; }

        ITableConfig PlaylistItem { get; }

        ITableConfig PlaylistColumn { get; }

        ITableConfig LibraryItem { get; }

        ITableConfig LibraryHierarchy { get; }

        ITableConfig LibraryHierarchyLevel { get; }
    }
}
