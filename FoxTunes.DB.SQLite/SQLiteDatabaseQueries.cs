using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using FoxTunes.Templates;
using System.Data;

namespace FoxTunes
{
    public class SQLiteDatabaseQueries : DatabaseQueries
    {
        public SQLiteDatabaseQueries(IDatabase database)
            : base(database)
        {
        }

        public override IDatabaseQuery AddLibraryHierarchyNodeToPlaylist(string filter)
        {
            var result = default(IFilterParserResult);
            if (!string.IsNullOrEmpty(filter) && !this.FilterParser.TryParse(filter, out result))
            {
                //TODO: Warn, failed to parse filter.
                result = null;
            }
            var template = new AddLibraryHierarchyNodeToPlaylist(this.Database, result);
            return this.Database.QueryFactory.Create(
                template.TransformText(),
                new DatabaseQueryParameter("libraryHierarchyId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                new DatabaseQueryParameter("libraryHierarchyItemId", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                new DatabaseQueryParameter("sequence", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None),
                new DatabaseQueryParameter("status", DbType.Byte, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None)
            );
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
