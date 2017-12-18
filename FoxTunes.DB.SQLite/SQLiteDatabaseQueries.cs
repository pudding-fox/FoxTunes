using FoxTunes.Interfaces;
using FoxTunes.Templates;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes
{
    public class SQLiteDatabaseQueries : BaseComponent, IDatabaseQueries
    {
        private static readonly Dictionary<Type, string> Tables = new Dictionary<Type, string>()
        {
            { typeof(PlaylistItem), "PlaylistItems" },
            { typeof(PlaylistColumn), "PlaylistColumns" },
            { typeof(LibraryItem), "LibraryItems" },
            { typeof(LibraryHierarchy), "LibraryHierarchies" },
            { typeof(LibraryHierarchyLevel), "LibraryHierarchyLevels" }
        };

        public SQLiteDatabaseQueries(IDatabase database)
        {
            this.Database = database;
        }

        public IDatabase Database { get; private set; }

        public IDatabaseQuery Count<T>()
        {
            var count = new Count(Tables[typeof(T)]);
            return new DatabaseQuery(count.TransformText());
        }

        public IDatabaseQuery Find<T>()
        {
            var find = new Find(Tables[typeof(T)]);
            return new DatabaseQuery(find.TransformText(), "id");
        }

        public IDatabaseQuery Select<T>(params string[] filters)
        {
            var select = new Select(Tables[typeof(T)], filters);
            return new DatabaseQuery(select.TransformText(), filters);
        }

        public IDatabaseQuery Insert<T>()
        {
            var fieldNames = SQLiteSchema.GetFieldNames(this.Database, Tables[typeof(T)]);
            var insert = new Insert(Tables[typeof(T)], fieldNames);
            return new DatabaseQuery(insert.TransformText(), fieldNames.ToArray());
        }

        public IDatabaseQuery Update<T>()
        {
            var fieldNames = SQLiteSchema.GetFieldNames(this.Database, Tables[typeof(T)]);
            var update = new Update(Tables[typeof(T)], fieldNames);
            return new DatabaseQuery(update.TransformText(), fieldNames.ToArray());
        }

        public IDatabaseQuery Delete<T>()
        {
            var delete = new Delete(Tables[typeof(T)]);
            return new DatabaseQuery(delete.TransformText(), "id");
        }

        public IDatabaseQuery AddLibraryHierarchyNodeToPlaylist
        {
            get
            {
                return new DatabaseQuery(Resources.AddLibraryHierarchyNodeToPlaylist, "libraryHierarchyItemId", "sequence", "status");
            }
        }

        public IDatabaseQuery AddLibraryHierarchyRecord
        {
            get
            {
                return new DatabaseQuery(Resources.AddLibraryHierarchyRecord, "libraryHierarchyId", "libraryHierarchyLevelId", "libraryItemId", "displayValue", "sortValue", "isLeaf");
            }
        }

        public IDatabaseQuery AddPlaylistSequenceRecord
        {
            get
            {
                return new DatabaseQuery(Resources.AddPlaylistSequenceRecord, "playlistItemId", "value1", "value2", "value3", "value4", "value5", "value6", "value7", "value8", "value9", "value10");
            }
        }

        public IDatabaseQuery AddLibraryItem
        {
            get
            {
                return new DatabaseQuery(Resources.AddLibraryItem, "directoryName", "fileName", "status");
            }
        }

        public IDatabaseQuery AddLibraryMetaDataItems
        {
            get
            {
                return new DatabaseQuery(Resources.AddLibraryMetaDataItems, "itemId", "name", "type", "numericValue", "textValue", "fileValue");
            }
        }

        public IDatabaseQuery AddPlaylistItem
        {
            get
            {
                return new DatabaseQuery(Resources.AddPlaylistItem, "sequence", "directoryName", "fileName", "status");
            }
        }

        public IDatabaseQuery AddPlaylistMetaDataItems
        {
            get
            {
                return new DatabaseQuery(Resources.AddPlaylistMetaDataItems, "itemId", "name", "type", "numericValue", "textValue", "fileValue");
            }
        }

        public IDatabaseQuery ClearPlaylist
        {
            get
            {
                return new DatabaseQuery(Resources.ClearPlaylist);
            }
        }

        public IDatabaseQuery CopyMetaDataItems
        {
            get
            {
                return new DatabaseQuery(Resources.CopyMetaDataItems, "status");
            }
        }

        public IDatabaseQuery GetPlaylistItemsWithoutMetaData
        {
            get
            {
                return new DatabaseQuery(Resources.GetPlaylistItemsWithoutMetaData, "status");
            }
        }

        public IDatabaseQuery GetLibraryItems
        {
            get
            {
                return new DatabaseQuery(Resources.GetLibraryItems, "status");
            }
        }

        public IDatabaseQuery GetLibraryHierarchyMetaDataItems
        {
            get
            {
                return new DatabaseQuery(Resources.GetLibraryHierarchyMetaDataItems, "libraryHierarchyItemId", "type");
            }
        }

        public IDatabaseQuery GetPlaylistMetaDataItems
        {
            get
            {
                return new DatabaseQuery(Resources.GetPlaylistMetaDataItems, "playlistItemId", "type");
            }
        }

        public IDatabaseQuery GetLibraryHierarchyNodes
        {
            get
            {
                return new DatabaseQuery(Resources.GetLibraryHierarchyNodes, "libraryHierarchyId", "libraryHierarchyItemId");
            }
        }

        public IDatabaseQuery GetLibraryHierarchyNodesWithFilter
        {
            get
            {
                return new DatabaseQuery(Resources.GetLibraryHierarchyNodesWithFilter, "libraryHierarchyId", "libraryHierarchyItemId", "filter");
            }
        }

        public IDatabaseQuery SetLibraryItemStatus
        {
            get
            {
                return new DatabaseQuery(Resources.SetLibraryItemStatus, "status");
            }
        }

        public IDatabaseQuery SetPlaylistItemStatus
        {
            get
            {
                return new DatabaseQuery(Resources.SetPlaylistItemStatus, "status");
            }
        }

        public IDatabaseQuery ShiftPlaylistItems
        {
            get
            {
                return new DatabaseQuery(Resources.ShiftPlaylistItems, "status", "sequence", "offset");
            }
        }

        public IDatabaseQuery VariousArtists
        {
            get
            {
                return new DatabaseQuery(Resources.VariousArtists, "name", "type", "numericValue");
            }
        }

        public IDatabaseQuery GetFirstPlaylistItem
        {
            get
            {
                return new DatabaseQuery(Resources.GetFirstPlaylistItem);
            }
        }

        public IDatabaseQuery GetLastPlaylistItem
        {
            get
            {
                return new DatabaseQuery(Resources.GetLastPlaylistItem);
            }
        }

        public IDatabaseQuery GetNextPlaylistItem
        {
            get
            {
                return new DatabaseQuery(Resources.GetNextPlaylistItem, "sequence");
            }
        }

        public IDatabaseQuery GetPreviousPlaylistItem
        {
            get
            {
                return new DatabaseQuery(Resources.GetPreviousPlaylistItem, "sequence");
            }
        }

        public IDatabaseQuery PlaylistSequenceBuilder(IEnumerable<string> metaDataNames)
        {
            var playlistSequenceBuilder = new PlaylistSequenceBuilder(metaDataNames);
            return new DatabaseQuery(playlistSequenceBuilder.TransformText(), "status");
        }

        public IDatabaseQuery LibraryHierarchyBuilder(IEnumerable<string> metaDataNames)
        {
            var playlistSequenceBuilder = new LibraryHierarchyBuilder(metaDataNames);
            return new DatabaseQuery(playlistSequenceBuilder.TransformText());
        }

        public IDatabaseQuery GetMetaDataNames
        {
            get
            {
                return new DatabaseQuery(Resources.GetMetaDataNames);
            }
        }
    }
}
