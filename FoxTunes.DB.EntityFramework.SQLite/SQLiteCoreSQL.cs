using FoxTunes.Interfaces;

namespace FoxTunes
{
    public class SQLiteCoreSQL : BaseComponent, ICoreSQL
    {
        public string AddImageItems
        {
            get
            {
                return Resources.AddImageItems;
            }
        }

        public string AddLibraryHierarchyNodeToPlaylist
        {
            get
            {
                return Resources.AddLibraryHierarchyNodeToPlaylist;
            }
        }

        public string AddLibraryHierarchyRecord
        {
            get
            {
                return Resources.AddLibraryHierarchyRecord;
            }
        }

        public string AddLibraryItem
        {
            get
            {
                return Resources.AddLibraryItem;
            }
        }

        public string AddMetaDataItems
        {
            get
            {
                return Resources.AddMetaDataItems;
            }
        }

        public string AddPlaylistItem
        {
            get
            {
                return Resources.AddPlaylistItem;
            }
        }

        public string AddPropertyItems
        {
            get
            {
                return Resources.AddPropertyItems;
            }
        }

        public string ClearPlaylist
        {
            get
            {
                return Resources.ClearPlaylist;
            }
        }

        public string CopyMetaDataItems
        {
            get
            {
                return Resources.CopyMetaDataItems;
            }
        }

        public string GetLibraryHierarchyMetaDataItems
        {
            get
            {
                return Resources.GetLibraryHierarchyMetaDataItems;
            }
        }

        public string GetLibraryHierarchyNodes
        {
            get
            {
                return Resources.GetLibraryHierarchyNodes;
            }
        }

        public string GetLibraryHierarchyNodesWithFilter
        {
            get
            {
                return Resources.GetLibraryHierarchyNodesWithFilter;
            }
        }

        public string SetLibraryItemStatus
        {
            get
            {
                return Resources.SetLibraryItemStatus;
            }
        }

        public string SetPlaylistItemStatus
        {
            get
            {
                return Resources.SetPlaylistItemStatus;
            }
        }

        public string ShiftPlaylistItems
        {
            get
            {
                return Resources.ShiftPlaylistItems;
            }
        }

        public string VariousArtists
        {
            get
            {
                return Resources.VariousArtists;
            }
        }

        public static readonly ICoreSQL Instance = new SQLiteCoreSQL();
    }
}
