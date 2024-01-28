using FoxTunes.Interfaces;

namespace FoxTunes
{
    public class RootDataSets : IRootDataSets
    {
        public RootDataSets(IDatabaseContext databaseContext)
        {
            this.DatabaseContext = databaseContext;
            this.PlaylistItem = this.DatabaseContext.GetSet<PlaylistItem>();
            this.PlaylistColumn = this.DatabaseContext.GetSet<PlaylistColumn>();
            this.LibraryItem = this.DatabaseContext.GetSet<LibraryItem>();
            this.LibraryHierarchy = this.DatabaseContext.GetSet<LibraryHierarchy>();
        }

        public IDatabaseContext DatabaseContext { get; private set; }

        public IDatabaseSet<PlaylistItem> PlaylistItem { get; private set; }

        public IDatabaseSet<PlaylistColumn> PlaylistColumn { get; private set; }

        public IDatabaseSet<LibraryItem> LibraryItem { get; private set; }

        public IDatabaseSet<LibraryHierarchy> LibraryHierarchy { get; private set; }
    }
}
