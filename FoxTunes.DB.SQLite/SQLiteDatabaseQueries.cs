using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using FoxTunes.Templates;
using System;
using System.Collections.Generic;

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

        public IDatabaseQuery AddLibraryHierarchyNodeToPlaylist
        {
            get
            {
                return this.Database.QueryFactory.Create(Resources.AddLibraryHierarchyNodeToPlaylist, "libraryHierarchyItemId", "sequence", "status");
            }
        }

        public IDatabaseQuery AddPlaylistSequenceRecord
        {
            get
            {
                return this.Database.QueryFactory.Create(Resources.AddPlaylistSequenceRecord, "playlistItemId", "value1", "value2", "value3", "value4", "value5", "value6", "value7", "value8", "value9", "value10");
            }
        }

        public IDatabaseQuery AddLibraryItem
        {
            get
            {
                return this.Database.QueryFactory.Create(Resources.AddLibraryItem, "directoryName", "fileName", "status");
            }
        }

        public IDatabaseQuery AddLibraryMetaDataItems
        {
            get
            {
                return this.Database.QueryFactory.Create(Resources.AddLibraryMetaDataItems, "itemId", "name", "type", "numericValue", "textValue", "fileValue");
            }
        }

        public IDatabaseQuery AddPlaylistItem
        {
            get
            {
                return this.Database.QueryFactory.Create(Resources.AddPlaylistItem, "sequence", "directoryName", "fileName", "status");
            }
        }

        public IDatabaseQuery AddPlaylistMetaDataItems
        {
            get
            {
                return this.Database.QueryFactory.Create(Resources.AddPlaylistMetaDataItems, "itemId", "name", "type", "numericValue", "textValue", "fileValue");
            }
        }

        public IDatabaseQuery ClearPlaylist
        {
            get
            {
                return this.Database.QueryFactory.Create(Resources.ClearPlaylist);
            }
        }

        public IDatabaseQuery ClearLibrary
        {
            get
            {
                return this.Database.QueryFactory.Create(Resources.CLearLibrary);
            }
        }

        public IDatabaseQuery GetPlaylistItemsWithoutMetaData
        {
            get
            {
                return this.Database.QueryFactory.Create(Resources.GetPlaylistItemsWithoutMetaData, "status");
            }
        }

        public IDatabaseQuery GetLibraryItems
        {
            get
            {
                return this.Database.QueryFactory.Create(Resources.GetLibraryItems, "status");
            }
        }

        public IDatabaseQuery GetLibraryHierarchyMetaDataItems
        {
            get
            {
                return this.Database.QueryFactory.Create(Resources.GetLibraryHierarchyMetaDataItems, "libraryHierarchyItemId", "type");
            }
        }

        public IDatabaseQuery GetPlaylistMetaDataItems
        {
            get
            {
                return this.Database.QueryFactory.Create(Resources.GetPlaylistMetaDataItems, "playlistItemId", "type");
            }
        }

        public IDatabaseQuery GetLibraryHierarchyNodes
        {
            get
            {
                return this.Database.QueryFactory.Create(Resources.GetLibraryHierarchyNodes, "libraryHierarchyId", "libraryHierarchyItemId");
            }
        }

        public IDatabaseQuery GetLibraryHierarchyNodesWithFilter
        {
            get
            {
                return this.Database.QueryFactory.Create(Resources.GetLibraryHierarchyNodesWithFilter, "libraryHierarchyId", "libraryHierarchyItemId", "filter");
            }
        }

        public IDatabaseQuery VariousArtists
        {
            get
            {
                return this.Database.QueryFactory.Create(Resources.VariousArtists, "name", "type", "numericValue");
            }
        }

        public IDatabaseQuery PlaylistSequenceBuilder(IEnumerable<string> metaDataNames)
        {
            var playlistSequenceBuilder = new PlaylistSequenceBuilder(this.Database, metaDataNames);
            return this.Database.QueryFactory.Create(playlistSequenceBuilder.TransformText(), "status");
        }

        public IDatabaseQuery LibraryHierarchyBuilder(IEnumerable<string> metaDataNames)
        {
            var playlistSequenceBuilder = new LibraryHierarchyBuilder(this.Database, metaDataNames);
            return this.Database.QueryFactory.Create(playlistSequenceBuilder.TransformText());
        }

        public IDatabaseQuery GetMetaDataNames
        {
            get
            {
                return this.Database.QueryFactory.Create(Resources.GetMetaDataNames);
            }
        }
    }
}
