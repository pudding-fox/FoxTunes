using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using FoxTunes.Templates;
using System.Collections.Generic;

namespace FoxTunes
{
    public class SQLiteDatabaseQueries : BaseComponent, IDatabaseQueries
    {
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

        public IDatabaseQuery AddLibraryMetaDataItems
        {
            get
            {
                return this.Database.QueryFactory.Create(Resources.AddLibraryMetaDataItems, "itemId", "name", "type", "numericValue", "textValue", "fileValue");
            }
        }

        public IDatabaseQuery AddPlaylistMetaDataItems
        {
            get
            {
                return this.Database.QueryFactory.Create(Resources.AddPlaylistMetaDataItems, "itemId", "name", "type", "numericValue", "textValue", "fileValue");
            }
        }

        public IDatabaseQuery GetLibraryHierarchyMetaDataItems
        {
            get
            {
                return this.Database.QueryFactory.Create(Resources.GetLibraryHierarchyMetaDataItems, "libraryHierarchyItemId", "type");
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
    }
}
