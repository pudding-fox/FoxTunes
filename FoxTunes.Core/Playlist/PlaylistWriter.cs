using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System.Data;
using System.Linq;

namespace FoxTunes
{
    public class PlaylistWriter : Disposable
    {
        public PlaylistWriter(IDatabaseComponent database, ITransactionSource transaction)
        {
            var parameters = default(IDatabaseParameters);
            this.Command = CreateCommand(database, transaction, out parameters);
            this.Parameters = parameters;
        }

        public IDbCommand Command { get; private set; }

        public IDatabaseParameters Parameters { get; private set; }

        public void Write(PlaylistItem playlistItem)
        {
            this.Parameters["directoryName"] = playlistItem.DirectoryName;
            this.Parameters["fileName"] = playlistItem.FileName;
            this.Parameters["sequence"] = playlistItem.Sequence;
            this.Parameters["status"] = PlaylistItemStatus.Import;
            this.Command.ExecuteNonQuery();
        }

        protected override void OnDisposing()
        {
            this.Command.Dispose();
            base.OnDisposing();
        }

        private static IDbCommand CreateCommand(IDatabaseComponent database, ITransactionSource transaction, out IDatabaseParameters parameters)
        {
            var query = database.QueryFactory.Build();
            query.Add.SetTable(database.Tables.PlaylistItem);
            query.Add.AddColumns(database.Tables.PlaylistItem.Columns.Except(database.Tables.PlaylistItem.PrimaryKeys));
            query.Output.AddSubQuery(database.QueryFactory.Build().With(subQuery =>
            {
                subQuery.Output.AddColumn(database.Tables.LibraryItem.Column("Id"));
                subQuery.Source.AddTable(database.Tables.LibraryItem);
                subQuery.Filter.AddColumn(database.Tables.LibraryItem.Column("FileName"));
            }));
            query.Output.AddParameters(database.Tables.PlaylistItem.Columns.Except(database.Tables.PlaylistItem.PrimaryKeys.Concat(database.Tables.PlaylistItem.Column("LibraryItem_Id"))));
            return database.CreateCommand(query.Build(), out parameters, transaction);
        }
    }
}
