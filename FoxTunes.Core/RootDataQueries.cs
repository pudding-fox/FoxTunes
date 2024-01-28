using FoxTunes.Interfaces;

namespace FoxTunes
{
    public class RootDataQueries : IRootDataQueries
    {
        public RootDataQueries(IDatabaseContext databaseContext)
        {
            this.DatabaseContext = databaseContext;
            this.PlaylistItem = this.DatabaseContext.GetQuery<PlaylistItem>();
            this.PlaylistItem.Include("MetaDatas");
            this.PlaylistItem.Include("Properties");
            this.PlaylistItem.Include("Images");
            this.PlaylistColumn = this.DatabaseContext.GetQuery<PlaylistColumn>();
            this.LibraryItem = this.DatabaseContext.GetQuery<LibraryItem>();
            this.LibraryItem.Include("MetaDatas");
            this.LibraryItem.Include("Properties");
            this.LibraryItem.Include("Images");
            this.LibraryItem.Include("Statistics");
            this.LibraryHierarchy = this.DatabaseContext.GetQuery<LibraryHierarchy>();
            this.LibraryHierarchy.Include("Levels");
            this.LibraryHierarchy.Include("Items");
        }

        public IDatabaseContext DatabaseContext { get; private set; }

        public IDatabaseQuery<PlaylistItem> PlaylistItem { get; private set; }

        public IDatabaseQuery<PlaylistColumn> PlaylistColumn { get; private set; }

        public IDatabaseQuery<LibraryItem> LibraryItem { get; private set; }

        public IDatabaseQuery<LibraryHierarchy> LibraryHierarchy { get; private set; }
    }
}
