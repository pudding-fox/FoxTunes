using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Templates;
using System.Collections.Generic;
using System.Data;

namespace FoxTunes
{
    public class SqlServerDatabaseQueries : DatabaseQueries
    {
        public SqlServerDatabaseQueries(IDatabase database)
            : base(database)
        {
        }

        public override IDatabaseQuery SequencePlaylistItems
        {
            get
            {
                var playlistSequenceBuilder = new PlaylistSequenceBuilder(this.Database);
                return this.Database.QueryFactory.Create(
                    playlistSequenceBuilder.TransformText(),
                    new DatabaseQueryParameter("status", DbType.Byte, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None)
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
