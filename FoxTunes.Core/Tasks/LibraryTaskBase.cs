#pragma warning disable 612, 618
using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System.Threading.Tasks;

namespace FoxTunes
{
    public abstract class LibraryTaskBase : BackgroundTask
    {
        protected LibraryTaskBase(string id)
            : base(id)
        {
        }

        public IDatabaseComponent Database { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Database = core.Components.Database;
            this.SignalEmitter = core.Components.SignalEmitter;
            base.InitializeComponent(core);
        }

        protected virtual Task SetLibraryItemsStatus(ITransactionSource transaction)
        {
            this.IsIndeterminate = true;
            var query = this.Database.QueryFactory.Build();
            query.Update.SetTable(this.Database.Tables.LibraryItem);
            query.Update.AddColumn(this.Database.Tables.LibraryItem.Column("Status"));
            return this.Database.ExecuteAsync(query, (parameters, phase) =>
            {
                switch (phase)
                {
                    case DatabaseParameterPhase.Fetch:
                        parameters["status"] = LibraryItemStatus.None;
                        break;
                }
            }, transaction);
        }

        protected virtual Task UpdateVariousArtists(ITransactionSource transaction)
        {
            return this.Database.ExecuteAsync(this.Database.Queries.UpdateLibraryVariousArtists, (parameters, phase) =>
            {
                switch (phase)
                {
                    case DatabaseParameterPhase.Fetch:
                        parameters["name"] = CustomMetaData.VariousArtists;
                        parameters["type"] = MetaDataItemType.Tag;
                        parameters["numericValue"] = 1;
                        parameters["status"] = LibraryItemStatus.Import;
                        break;
                }
            }, transaction);
        }

        protected virtual Task CleanupMetaData(ITransactionSource transaction)
        {
            var table = this.Database.Config.Table("LibraryItem_MetaDataItem", TableFlags.None);
            var column = table.Column("LibraryItem_Id");
            var query = this.Database.QueryFactory.Build();
            query.Delete.Touch();
            query.Source.AddTable(table);
            query.Filter.Add().With(expression =>
            {
                expression.Left = expression.CreateColumn(column);
                expression.Operator = expression.CreateOperator(QueryOperator.Not);
                expression.Right = expression.CreateUnary(
                    QueryOperator.In,
                    expression.CreateSubQuery(this.Database.QueryFactory.Build().With(subQuery =>
                    {
                        subQuery.Output.AddColumn(this.Database.Tables.LibraryItem.PrimaryKey);
                        subQuery.Source.AddTable(this.Database.Tables.LibraryItem);
                    }))
                );
            });
            return this.Database.ExecuteAsync(query, transaction);
        }
    }
}
