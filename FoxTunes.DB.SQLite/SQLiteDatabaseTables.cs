using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;

namespace FoxTunes
{
    public class SQLiteDatabaseTables : BaseComponent, IDatabaseTables
    {
        public SQLiteDatabaseTables(IDatabase database)
        {
            this.PlaylistItem = database.Config.GetTable(TableConfig.By(typeof(PlaylistItem), TableFlags.None));
            this.PlaylistColumn = database.Config.GetTable(TableConfig.By(typeof(PlaylistColumn), TableFlags.None));
            this.LibraryItem = database.Config.GetTable(TableConfig.By(typeof(LibraryItem), TableFlags.None));
            this.LibraryHierarchy = database.Config.GetTable(TableConfig.By(typeof(LibraryHierarchy), TableFlags.None));
            this.LibraryHierarchyLevel = database.Config.GetTable(TableConfig.By(typeof(LibraryHierarchyLevel), TableFlags.None));
        }

        public ITableConfig PlaylistItem { get; private set; }

        public ITableConfig PlaylistColumn { get; private set; }

        public ITableConfig LibraryItem { get; private set; }

        public ITableConfig LibraryHierarchy { get; private set; }

        public ITableConfig LibraryHierarchyLevel { get; private set; }
    }
}
