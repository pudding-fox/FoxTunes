#pragma warning disable 612, 618
using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System;
using System.Data;
using System.Threading.Tasks;

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

        protected virtual async Task ClearItems(ITransactionSource transaction)
        {
            this.IsIndeterminate = true;
            Logger.Write(this, LogLevel.Debug, "Clearing playlist.");
            var query = this.Database.QueryFactory.Build();
            query.Delete.Touch();
            query.Source.AddTable(this.Database.Tables.PlaylistItem);
            query.Filter.AddColumn(this.Database.Tables.PlaylistItem.Column("Status"));
            await this.Database.ExecuteAsync(query, (parameters, phase) =>
            {
                switch (phase)
                {
                    case DatabaseParameterPhase.Fetch:
                        parameters["status"] = PlaylistItemStatus.None;
                        break;
                }
            }, transaction);
            await this.CleanupMetaData(transaction);
        }

        protected virtual Task ShiftItems(QueryOperator @operator, int at, int by, ITransactionSource transaction)
        {
            Logger.Write(
                this,
                LogLevel.Debug,
                "Shifting playlist items at position {0} {1} by offset {2}.",
                Enum.GetName(typeof(QueryOperator), @operator),
                at,
                by
            );
            this.IsIndeterminate = true;
            var query = this.Database.QueryFactory.Build();
            var sequence = this.Database.Tables.PlaylistItem.Column("Sequence");
            var status = this.Database.Tables.PlaylistItem.Column("Status");
            query.Update.SetTable(this.Database.Tables.PlaylistItem);
            query.Update.AddColumn(sequence).Right = query.Update.Fragment<IBinaryExpressionBuilder>().With(expression =>
            {
                expression.Left = expression.CreateColumn(sequence);
                expression.Operator = expression.CreateOperator(QueryOperator.Plus);
                expression.Right = expression.CreateParameter("offset", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None);
            });
            query.Filter.AddColumn(status);
            query.Filter.AddColumn(sequence).With(expression =>
            {
                expression.Operator = expression.CreateOperator(@operator);
                expression.Right = expression.CreateParameter("sequence", DbType.Int32, 0, 0, 0, ParameterDirection.Input, false, null, DatabaseQueryParameterFlags.None);
            });
            return this.Database.ExecuteAsync(query, (parameters, phase) =>
            {
                switch (phase)
                {
                    case DatabaseParameterPhase.Fetch:
                        parameters["status"] = PlaylistItemStatus.None;
                        parameters["sequence"] = at;
                        parameters["offset"] = by;
                        break;
                }
            }, transaction);
        }

        protected virtual async Task SequenceItems(CancellationToken cancellationToken, ITransactionSource transaction)
        {
            Logger.Write(this, LogLevel.Debug, "Sequencing playlist items.");
            this.IsIndeterminate = true;
            var metaDataNames = MetaDataInfo.GetMetaDataNames(this.Database, transaction);
            using (var reader = this.Database.ExecuteReader(this.Database.Queries.PlaylistSequenceBuilder(metaDataNames), (parameters, phase) =>
            {
                switch (phase)
                {
                    case DatabaseParameterPhase.Fetch:
                        parameters["status"] = PlaylistItemStatus.Import;
                        break;
                }
            }, transaction))
            {
                await this.SequenceItems(reader, cancellationToken, transaction);
            }
        }

        protected virtual async Task SequenceItems(IDatabaseReader reader, CancellationToken cancellationToken, ITransactionSource transaction)
        {
            using (var playlistSequencePopulator = new PlaylistSequencePopulator(this.Database, transaction))
            {
                playlistSequencePopulator.InitializeComponent(this.Core);
                await playlistSequencePopulator.Populate(reader, cancellationToken);
            }
        }

        protected virtual Task SetPlaylistItemsStatus(ITransactionSource transaction)
        {
            Logger.Write(this, LogLevel.Debug, "Setting playlist status: {0}", Enum.GetName(typeof(LibraryItemStatus), LibraryItemStatus.None));
            this.IsIndeterminate = true;
            var query = this.Database.QueryFactory.Build();
            query.Update.SetTable(this.Database.Tables.PlaylistItem);
            query.Update.AddColumn(this.Database.Tables.PlaylistItem.Column("Status"));
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
            return this.Database.ExecuteAsync(this.Database.Queries.UpdatePlaylistVariousArtists, (parameters, phase) =>
             {
                 switch (phase)
                 {
                     case DatabaseParameterPhase.Fetch:
                         parameters["name"] = CustomMetaData.VariousArtists;
                         parameters["type"] = MetaDataItemType.Tag;
                         parameters["numericValue"] = 1;
                         parameters["status"] = PlaylistItemStatus.Import;
                         break;
                 }
             }, transaction);
        }

        protected virtual Task CleanupMetaData(ITransactionSource transaction)
        {
            Logger.Write(this, LogLevel.Debug, "Cleaning up unused meta data.");
            this.IsIndeterminate = true;
            var table = this.Database.Config.Table("PlaylistItem_MetaDataItem", TableFlags.None);
            var column = table.Column("PlaylistItem_Id");
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
                        subQuery.Output.AddColumn(this.Database.Tables.PlaylistItem.PrimaryKey);
                        subQuery.Source.AddTable(this.Database.Tables.PlaylistItem);
                    }))
                );
            });
            return this.Database.ExecuteAsync(query, transaction);
        }
    }
}
