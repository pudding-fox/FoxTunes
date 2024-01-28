using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using FoxTunes.Templates;
using System.Collections.Generic;
using System.Data;

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
                return this.Database.QueryFactory.Create(
                    Resources.AddLibraryHierarchyNodeToPlaylist,
                    new DatabaseQueryParameter("libraryHierarchyItemId", DbType.Int32, ParameterDirection.Input),
                    new DatabaseQueryParameter("sequence", DbType.Int32, ParameterDirection.Input),
                    new DatabaseQueryParameter("status", DbType.Byte, ParameterDirection.Input)
                );
            }
        }

        public IDatabaseQuery AddPlaylistSequenceRecord
        {
            get
            {
                return this.Database.QueryFactory.Create(
                    Resources.AddPlaylistSequenceRecord,
                    new DatabaseQueryParameter("playlistItemId", DbType.Int32, ParameterDirection.Input),
                    new DatabaseQueryParameter("value1", DbType.Byte, ParameterDirection.Input),
                    new DatabaseQueryParameter("value2", DbType.String, ParameterDirection.Input),
                    new DatabaseQueryParameter("value3", DbType.String, ParameterDirection.Input),
                    new DatabaseQueryParameter("value4", DbType.String, ParameterDirection.Input),
                    new DatabaseQueryParameter("value5", DbType.String, ParameterDirection.Input),
                    new DatabaseQueryParameter("value6", DbType.String, ParameterDirection.Input),
                    new DatabaseQueryParameter("value7", DbType.String, ParameterDirection.Input),
                    new DatabaseQueryParameter("value8", DbType.String, ParameterDirection.Input),
                    new DatabaseQueryParameter("value9", DbType.String, ParameterDirection.Input),
                    new DatabaseQueryParameter("value10", DbType.String, ParameterDirection.Input)
                );
            }
        }

        public IDatabaseQuery AddLibraryMetaDataItems
        {
            get
            {
                return this.Database.QueryFactory.Create(
                    Resources.AddLibraryMetaDataItems,
                    new DatabaseQueryParameter("itemId", DbType.Int32, ParameterDirection.Input),
                    new DatabaseQueryParameter("name", DbType.String, ParameterDirection.Input),
                    new DatabaseQueryParameter("type", DbType.Byte, ParameterDirection.Input),
                    new DatabaseQueryParameter("numericValue", DbType.Int32, ParameterDirection.Input),
                    new DatabaseQueryParameter("textValue", DbType.String, ParameterDirection.Input),
                    new DatabaseQueryParameter("fileValue", DbType.String, ParameterDirection.Input)
                );
            }
        }

        public IDatabaseQuery AddPlaylistMetaDataItems
        {
            get
            {
                return this.Database.QueryFactory.Create(
                    Resources.AddPlaylistMetaDataItems,
                    new DatabaseQueryParameter("itemId", DbType.Int32, ParameterDirection.Input),
                    new DatabaseQueryParameter("name", DbType.String, ParameterDirection.Input),
                    new DatabaseQueryParameter("type", DbType.Byte, ParameterDirection.Input),
                    new DatabaseQueryParameter("numericValue", DbType.Int32, ParameterDirection.Input),
                    new DatabaseQueryParameter("textValue", DbType.String, ParameterDirection.Input),
                    new DatabaseQueryParameter("fileValue", DbType.String, ParameterDirection.Input)
                );
            }
        }

        public IDatabaseQuery GetLibraryHierarchyMetaDataItems
        {
            get
            {
                return this.Database.QueryFactory.Create(
                    Resources.GetLibraryHierarchyMetaDataItems,
                    new DatabaseQueryParameter("libraryHierarchyItemId", DbType.Int32, ParameterDirection.Input),
                    new DatabaseQueryParameter("type", DbType.Byte, ParameterDirection.Input)
                );
            }
        }

        public IDatabaseQuery GetLibraryHierarchyNodes
        {
            get
            {
                return this.Database.QueryFactory.Create(
                    Resources.GetLibraryHierarchyNodes,
                    new DatabaseQueryParameter("libraryHierarchyId", DbType.Int32, ParameterDirection.Input),
                    new DatabaseQueryParameter("libraryHierarchyItemId", DbType.Int32, ParameterDirection.Input)
                );
            }
        }

        public IDatabaseQuery GetLibraryHierarchyNodesWithFilter
        {
            get
            {
                return this.Database.QueryFactory.Create(
                    Resources.GetLibraryHierarchyNodesWithFilter,
                    new DatabaseQueryParameter("libraryHierarchyId", DbType.Int32, ParameterDirection.Input),
                    new DatabaseQueryParameter("libraryHierarchyItemId", DbType.Int32, ParameterDirection.Input),
                    new DatabaseQueryParameter("filter", DbType.String, ParameterDirection.Input)
                );
            }
        }

        public IDatabaseQuery VariousArtists
        {
            get
            {
                return this.Database.QueryFactory.Create(
                    Resources.VariousArtists,
                    new DatabaseQueryParameter("name", DbType.String, ParameterDirection.Input),
                    new DatabaseQueryParameter("type", DbType.Byte, ParameterDirection.Input),
                    new DatabaseQueryParameter("numericValue", DbType.Int32, ParameterDirection.Input)
                );
            }
        }

        public IDatabaseQuery PlaylistSequenceBuilder(IEnumerable<string> metaDataNames)
        {
            var playlistSequenceBuilder = new PlaylistSequenceBuilder(this.Database, metaDataNames);
            return this.Database.QueryFactory.Create(
                playlistSequenceBuilder.TransformText(),
                new DatabaseQueryParameter("status", DbType.Byte, ParameterDirection.Input)
            );
        }

        public IDatabaseQuery LibraryHierarchyBuilder(IEnumerable<string> metaDataNames)
        {
            var playlistSequenceBuilder = new LibraryHierarchyBuilder(this.Database, metaDataNames);
            return this.Database.QueryFactory.Create(playlistSequenceBuilder.TransformText());
        }
    }
}
