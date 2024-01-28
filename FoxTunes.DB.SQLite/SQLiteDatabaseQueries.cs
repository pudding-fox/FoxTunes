using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Templates;
using System.Collections.Generic;
using System.Data;

namespace FoxTunes
{
    public class SQLiteDatabaseQueries : DatabaseQueries
    {
        public SQLiteDatabaseQueries(IDatabase database)
            : base(database)
        {
        }

        public override IDatabaseQuery BeginSequencePlaylistItems
        {
            get
            {
                return this.Database.QueryFactory.Create(Resources.BeginSequencePlaylistItems);
            }
        }

        public override IDatabaseQuery SequencePlaylistItems(IEnumerable<string> metaDataNames)
        {
            var playlistSequenceBuilder = new PlaylistSequenceBuilder(this.Database, metaDataNames);
            return this.Database.QueryFactory.Create(
                playlistSequenceBuilder.TransformText(),
                new DatabaseQueryParameter("status", DbType.Byte, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None)
            );
        }

        public override IDatabaseQuery EndSequencePlaylistItems
        {
            get
            {
                return this.Database.QueryFactory.Create(
                    Resources.EndSequencePlaylistItems,
                    new DatabaseQueryParameter("status", DbType.Byte, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None)
                );
            }
        }

        public override IDatabaseQuery BuildLibraryHierarchies(IEnumerable<string> metaDataNames)
        {
            var libraryHierarchyBuilder = new LibraryHierarchyBuilder(this.Database, metaDataNames);
            return this.Database.QueryFactory.Create(
                libraryHierarchyBuilder.TransformText(),
                new DatabaseQueryParameter("status", DbType.Byte, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None)
            );
        }

        public override IDatabaseQuery GetLibraryHierarchyNodesWithFilter
        {
            get
            {
                return this.Database.QueryFactory.Create(
                    Resources.GetLibraryHierarchyNodesWithFilter,
                    new DatabaseQueryParameter("libraryHierarchyId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("libraryHierarchyItemId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("filter", DbType.String, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None)
                );
            }
        }

        public override IDatabaseQuery GetLibraryHierarchyMetaData
        {
            get
            {
                return this.Database.QueryFactory.Create(
                    Resources.GetLibraryHierarchyMetaData,
                    new DatabaseQueryParameter("libraryHierarchyItemId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                    new DatabaseQueryParameter("type", DbType.Byte, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None)
                );
            }
        }
    }
}
