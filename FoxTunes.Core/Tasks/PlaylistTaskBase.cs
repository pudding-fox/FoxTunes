using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System;
using FoxDb;

namespace FoxTunes
{
    public abstract class PlaylistTaskBase : BackgroundTask
    {
        protected PlaylistTaskBase(string id, int sequence = 0)
            : base(id)
        {
            this.Sequence = sequence;
        }

        public int Sequence { get; protected set; }

        public int Offset { get; protected set; }

        public ICore Core { get; private set; }

        public IDatabaseComponent Database { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public IScriptingRuntime ScriptingRuntime { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.Database = core.Components.Database;
            this.SignalEmitter = core.Components.SignalEmitter;
            this.ScriptingRuntime = core.Components.ScriptingRuntime;
            base.InitializeComponent(core);
        }

        protected virtual void ShiftItems(ITransactionSource transaction)
        {
            Logger.Write(this, LogLevel.Debug, "Shifting playlist items at position {0} by offset {1}.", this.Sequence, this.Offset);
            this.IsIndeterminate = true;
            var query = this.Database.QueryFactory.Build();
            var sequence = this.Database.Tables.PlaylistItem.Column("Sequence");
            var status = this.Database.Tables.PlaylistItem.Column("Status");
            query.Update.SetTable(this.Database.Tables.PlaylistItem);
            query.Update.AddColumn(sequence).Right = query.Update.Fragment<IBinaryExpressionBuilder>().With(expression =>
            {
                expression.Left = expression.CreateColumn(sequence);
                expression.Operator = expression.CreateOperator(QueryOperator.Add);
                expression.Right = expression.CreateParameter("offset");
            });
            query.Filter.AddColumn(status);
            query.Filter.AddColumn(sequence).With(expression =>
            {
                expression.Operator = expression.CreateOperator(QueryOperator.GreaterOrEqual);
                expression.Right = expression.CreateParameter("sequence");
            });
            this.Database.Execute(query, parameters =>
            {
                parameters["status"] = PlaylistItemStatus.None;
                parameters["sequence"] = this.Sequence;
                parameters["offset"] = this.Offset;
            }, transaction);
        }

        protected virtual void SequenceItems(ITransactionSource transaction)
        {
            var metaDataNames = MetaDataInfo.GetMetaDataNames(this.Database, transaction);
            using (var reader = this.Database.ExecuteReader(this.Database.Queries.PlaylistSequenceBuilder(metaDataNames), parameters =>
            {
                parameters["status"] = PlaylistItemStatus.Import;
            }, transaction))
            {
                this.SequenceItems(transaction, reader);
            }
        }

        protected virtual void SequenceItems(ITransactionSource transaction, IDatabaseReader reader)
        {
            using (var playlistSequencePopulator = new PlaylistSequencePopulator(this.Database, transaction))
            {
                playlistSequencePopulator.InitializeComponent(this.Core);
                playlistSequencePopulator.Populate(reader);
            }
        }

        protected virtual void SetPlaylistItemsStatus(ITransactionSource transaction)
        {
            Logger.Write(this, LogLevel.Debug, "Setting playlist status: {0}", Enum.GetName(typeof(LibraryItemStatus), LibraryItemStatus.None));
            this.IsIndeterminate = true;
            var query = this.Database.QueryFactory.Build();
            query.Update.SetTable(this.Database.Tables.PlaylistItem);
            query.Update.AddColumn(this.Database.Tables.PlaylistItem.Column("Status"));
            this.Database.Execute(query, parameters => parameters["status"] = LibraryItemStatus.None, transaction);
        }
    }
}
