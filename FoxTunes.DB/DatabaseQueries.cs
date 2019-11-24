using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Data;

namespace FoxTunes
{
    public abstract class DatabaseQueries : BaseComponent, IDatabaseQueries
    {
        public DatabaseQueries(IDatabase database)
        {
            this.Database = database;
        }

        public IDatabase Database { get; private set; }


        public IDatabaseQuery AddLibraryHierarchyNode
        {
            get
            {
                return this.Database.QueryFactory.Create(
                    Resources.AddLibraryHierarchyNode,
                    new DatabaseQueryParameter("libraryHierarchyId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("libraryItemId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("parentId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("value", DbType.String, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("isLeaf", DbType.Boolean, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None)
                );
            }
        }

        public IDatabaseQuery AddLibraryHierarchyNodeToPlaylist
        {
            get
            {
                return this.Database.QueryFactory.Create(
                    Resources.AddLibraryHierarchyNodeToPlaylist,
                    new DatabaseQueryParameter("libraryHierarchyItemId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("sequence", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("status", DbType.Byte, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None)
                );
            }
        }

        public IDatabaseQuery AddLibraryMetaDataItem
        {
            get
            {
                return this.Database.QueryFactory.Create(
                    Resources.AddLibraryMetaDataItem,
                    new DatabaseQueryParameter("itemId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("metaDataItemId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None)
                );
            }
        }

        public IDatabaseQuery AddPlaylistMetaDataItem
        {
            get
            {
                return this.Database.QueryFactory.Create(
                    Resources.AddPlaylistMetaDataItem,
                    new DatabaseQueryParameter("itemId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("metaDataItemId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None)
                );
            }
        }

        public abstract IDatabaseQuery BuildLibraryHierarchies(IEnumerable<string> metaDataNames);

        public IDatabaseQuery CleanupLibraryHierarchyNodes
        {
            get
            {
                return this.Database.QueryFactory.Create(
                    Resources.CleanupLibraryHierarchyNodes,
                    new DatabaseQueryParameter("libraryHierarchyId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None)
                );
            }
        }

        public IDatabaseQuery ClearLibraryMetaDataItems
        {
            get
            {
                return this.Database.QueryFactory.Create(
                    Resources.ClearLibraryMetaDataItems,
                    new DatabaseQueryParameter("itemId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None)
                );
            }
        }

        public IDatabaseQuery ClearPlaylistMetaDataItems
        {
            get
            {
                return this.Database.QueryFactory.Create(
                    Resources.ClearPlaylistMetaDataItems,
                    new DatabaseQueryParameter("itemId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None)
                );
            }
        }

        public IDatabaseQuery GetIsFavorite
        {
            get
            {
                return this.Database.QueryFactory.Create(
                    Resources.GetIsFavorite,
                    new DatabaseQueryParameter("libraryHierarchyItemId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None)
                );
            }
        }

        public abstract IDatabaseQuery GetLibraryHierarchyMetaData { get; }

        public IDatabaseQuery GetLibraryHierarchyNodes
        {
            get
            {
                return this.Database.QueryFactory.Create(
                    Resources.GetLibraryHierarchyNodes,
                    new DatabaseQueryParameter("libraryHierarchyId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("libraryHierarchyItemId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("favorite", DbType.Boolean, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None)
                );
            }
        }

        public IDatabaseQuery GetLibraryHierarchyNodesWithFilter
        {
            get
            {
                return this.Database.QueryFactory.Create(
                    Resources.GetLibraryHierarchyNodesWithFilter,
                    new DatabaseQueryParameter("libraryHierarchyId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("libraryHierarchyItemId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("filter", DbType.String, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("favorite", DbType.Boolean, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None)
                );
            }
        }

        public IDatabaseQuery GetLibraryItems
        {
            get
            {
                return this.Database.QueryFactory.Create(
                    Resources.GetLibraryItems,
                    new DatabaseQueryParameter("libraryHierarchyId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("libraryHierarchyItemId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("favorite", DbType.Boolean, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None)
                );
            }
        }

        public IDatabaseQuery GetLibraryItemsWithFilter
        {
            get
            {
                return this.Database.QueryFactory.Create(
                    Resources.GetLibraryItemsWithFilter,
                    new DatabaseQueryParameter("libraryHierarchyId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("libraryHierarchyItemId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("filter", DbType.String, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("favorite", DbType.Boolean, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None)
                );
            }
        }

        public IDatabaseQuery GetOrAddMetaDataItem
        {
            get
            {
                return this.Database.QueryFactory.Create(
                    Resources.GetOrAddMetaDataItem,
                    new DatabaseQueryParameter("name", DbType.String, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("type", DbType.Byte, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("value", DbType.String, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None)
                );
            }
        }

        public IDatabaseQuery MovePlaylistItem
        {
            get
            {
                return this.Database.QueryFactory.Create(
                    Resources.MovePlaylistItem,
                    new DatabaseQueryParameter("id", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("sequence", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None)
                );
            }
        }

        public IDatabaseQuery RemoveLibraryHierarchyItems
        {
            get
            {
                return this.Database.QueryFactory.Create(
                    Resources.RemoveLibraryHierarchyItems,
                    new DatabaseQueryParameter("status", DbType.Byte, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None)
                );
            }
        }

        public IDatabaseQuery RemoveLibraryItems
        {
            get
            {
                return this.Database.QueryFactory.Create(
                    Resources.RemoveLibraryItems,
                    new DatabaseQueryParameter("status", DbType.Byte, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None)
                );
            }
        }

        public IDatabaseQuery RemovePlaylistItems
        {
            get
            {
                return this.Database.QueryFactory.Create(
                    Resources.RemovePlaylistItems,
                    new DatabaseQueryParameter("status", DbType.Byte, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None)
                );
            }
        }

        public abstract IDatabaseQuery SequencePlaylistItems(IEnumerable<string> metaDataNames);

        public IDatabaseQuery SetIsFavorite
        {
            get
            {
                return this.Database.QueryFactory.Create(
                    Resources.SetIsFavorite,
                    new DatabaseQueryParameter("libraryHierarchyItemId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("isFavorite", DbType.Boolean, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None)
                );
            }
        }

        public IDatabaseQuery UpdateLibraryHierarchyNode
        {
            get
            {
                return this.Database.QueryFactory.Create(
                    Resources.UpdateLibraryHierarchyNode,
                    new DatabaseQueryParameter("libraryHierarchyItemId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("libraryItemId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None)
                );
            }
        }
        public IDatabaseQuery UpdateLibraryVariousArtists
        {
            get
            {
                return this.Database.QueryFactory.Create(
                    Resources.UpdateLibraryVariousArtists,
                    new DatabaseQueryParameter("name", DbType.String, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("type", DbType.Byte, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("value", DbType.String, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("status", DbType.Byte, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None)
                );
            }
        }

        public IDatabaseQuery UpdatePlaylistVariousArtists
        {
            get
            {
                return this.Database.QueryFactory.Create(
                    Resources.UpdatePlaylistVariousArtists,
                    new DatabaseQueryParameter("name", DbType.String, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("type", DbType.Byte, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("value", DbType.String, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("status", DbType.Byte, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None)
                );
            }
        }
    }
}
