using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;

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

        protected virtual void CleanupMetaData(ITransactionSource transaction)
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
            this.Database.Execute(query, transaction);
        }
    }
}
