#pragma warning disable 612, 618
using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System.Data;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class LibraryHierarchyWriter : Disposable
    {
        public LibraryHierarchyWriter(IDatabaseComponent database, ITransactionSource transaction)
        {
            this.Command = CreateCommand(database, transaction);
        }

        public IDatabaseCommand Command { get; private set; }

        public Task Write(IDatabaseReaderRecord record, object value)
        {
            this.Command.Parameters["libraryHierarchyId"] = record["LibraryHierarchy_Id"];
            this.Command.Parameters["libraryHierarchyLevelId"] = record["LibraryHierarchyLevel_Id"];
            this.Command.Parameters["libraryItemId"] = record["LibraryItem_Id"];
            this.Command.Parameters["value"] = value;
            this.Command.Parameters["isLeaf"] = record["IsLeaf"];
            return this.Command.ExecuteNonQueryAsync();
        }

        protected override void OnDisposing()
        {
            this.Command.Dispose();
            base.OnDisposing();
        }

        private static IDatabaseCommand CreateCommand(IDatabaseComponent database, ITransactionSource transaction)
        {
            var table = database.Config.Table("LibraryHierarchy", TableFlags.None);
            table.CreateColumn(ColumnConfig.By("LibraryHierarchy_Id", Factories.Type.Create(TypeConfig.By(DbType.Int32))));
            table.CreateColumn(ColumnConfig.By("LibraryHierarchyLevel_Id", Factories.Type.Create(TypeConfig.By(DbType.Int32))));
            table.CreateColumn(ColumnConfig.By("LibraryItem_Id", Factories.Type.Create(TypeConfig.By(DbType.Int32))));
            table.CreateColumn(ColumnConfig.By("Value", Factories.Type.Create(TypeConfig.By(DbType.String))));
            table.CreateColumn(ColumnConfig.By("IsLeaf", Factories.Type.Create(TypeConfig.By(DbType.Boolean))));
            var query = database.QueryFactory.Build();
            query.Add.SetTable(table);
            query.Add.AddColumns(table.Columns);
            query.Output.AddParameters(table.Columns);
            return database.CreateCommand(
                query.Build(),
                DatabaseCommandFlags.NoCache,
                transaction
            );
        }
    }
}
