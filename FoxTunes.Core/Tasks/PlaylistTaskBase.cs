#pragma warning disable 612, 618
using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace FoxTunes
{
    public abstract class PlaylistTaskBase : BackgroundTask
    {
        public const string ID = "4403475F-D67C-4ED8-BF1F-68D22F28066F";

        protected PlaylistTaskBase(int sequence = 0)
            : base(ID)
        {
            this.Sequence = sequence;
            this.Warnings = new Dictionary<PlaylistItem, IList<string>>();
        }

        public int Sequence { get; protected set; }

        public int Offset { get; protected set; }

        public IDictionary<PlaylistItem, IList<string>> Warnings { get; private set; }

        public ICore Core { get; private set; }

        public IDatabaseComponent Database { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public IMetaDataSourceFactory MetaDataSourceFactory { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public IScriptingRuntime ScriptingRuntime { get; private set; }

        public IMetaDataBrowser MetaDataBrowser { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.Database = core.Factories.Database.Create();
            this.PlaybackManager = core.Managers.Playback;
            this.MetaDataSourceFactory = core.Factories.MetaDataSource;
            this.SignalEmitter = core.Components.SignalEmitter;
            this.ScriptingRuntime = core.Components.ScriptingRuntime;
            this.MetaDataBrowser = core.Components.MetaDataBrowser;
            base.InitializeComponent(core);
        }

        protected virtual async Task AddPaths(IEnumerable<string> paths)
        {
            using (var task = new SingletonReentrantTask(this, ComponentSlots.Database, SingletonReentrantTask.PRIORITY_HIGH, async cancellationToken =>
             {
                 await this.AddPlaylistItems(paths, cancellationToken).ConfigureAwait(false);
                 await this.ShiftItems(QueryOperator.GreaterOrEqual, this.Sequence, this.Offset).ConfigureAwait(false);
                 await this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistUpdated)).ConfigureAwait(false);
                 if (this.MetaDataSourceFactory.Enabled)
                 {
                     await this.AddOrUpdateMetaData(cancellationToken).ConfigureAwait(false);
                 }
                 await this.SequenceItems().ConfigureAwait(false);
                 await this.SetPlaylistItemsStatus(PlaylistItemStatus.None).ConfigureAwait(false);
             }))
            {
                await task.Run().ConfigureAwait(false);
            }
        }

        protected virtual async Task AddPlaylistItems(IEnumerable<string> paths, CancellationToken cancellationToken)
        {
            //We don't know how many files we're about to enumerate.
            if (!this.IsIndeterminate)
            {
                await this.SetIsIndeterminate(true).ConfigureAwait(false);
            }
            using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
            {
                using (var playlistPopulator = new PlaylistPopulator(this.Database, this.PlaybackManager, this.Sequence, this.Offset, this.Visible, transaction))
                {
                    playlistPopulator.InitializeComponent(this.Core);
                    await this.WithSubTask(playlistPopulator,
                        () => playlistPopulator.Populate(paths, cancellationToken)
                    ).ConfigureAwait(false);
                    this.Offset = playlistPopulator.Offset;
                }
                transaction.Commit();
            }
        }

        protected virtual async Task AddOrUpdateMetaData(CancellationToken cancellationToken)
        {
            using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
            {
                using (var metaDataPopulator = new PlaylistMetaDataPopulator(this.Database, this.Visible, transaction))
                {
                    metaDataPopulator.InitializeComponent(this.Core);
                    await this.WithSubTask(metaDataPopulator,
                        () => metaDataPopulator.Populate(PlaylistItemStatus.Import, cancellationToken)
                    ).ConfigureAwait(false);
                    foreach (var pair in metaDataPopulator.Warnings)
                    {
                        if (pair.Key is PlaylistItem playlistItem)
                        {
                            this.Warnings.Add(playlistItem, pair.Value);
                        }
                    }
                }
                transaction.Commit();
            }
        }

        protected virtual async Task MoveItems(IEnumerable<PlaylistItem> playlistItems)
        {
            using (var task = new SingletonReentrantTask(this, ComponentSlots.Database, SingletonReentrantTask.PRIORITY_HIGH, async cancellationToken =>
            {
                using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
                {
                    foreach (var playlistItem in playlistItems)
                    {
                        await this.Database.ExecuteAsync(this.Database.Queries.MovePlaylistItem, (parameters, phase) =>
                        {
                            switch (phase)
                            {
                                case DatabaseParameterPhase.Fetch:
                                    parameters["id"] = playlistItem.Id;
                                    parameters["sequence"] = this.Sequence;
                                    break;
                            }
                        }, transaction).ConfigureAwait(false);
                        if (playlistItem.Sequence > this.Sequence)
                        {
                            //TODO: If the current sequence is less than the target sequence we don't have to increment the counter.
                            //TODO: I don't really understand why.
                            this.Sequence++;
                        }
                    }
                    transaction.Commit();
                }
            }))
            {
                await task.Run().ConfigureAwait(false);
            }
        }

        protected virtual async Task RemoveItems(IEnumerable<PlaylistItem> playlistItems)
        {
            using (var task = new SingletonReentrantTask(this, ComponentSlots.Database, SingletonReentrantTask.PRIORITY_HIGH, async cancellationToken =>
            {
                using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
                {
                    foreach (var playlistItem in playlistItems)
                    {
                        playlistItem.Status = PlaylistItemStatus.Remove;
                    }
                    var set = this.Database.Set<PlaylistItem>(transaction);
                    await set.AddOrUpdateAsync(playlistItems).ConfigureAwait(false);
                    if (transaction.HasTransaction)
                    {
                        transaction.Commit();
                    }
                }
            }))
            {
                await task.Run().ConfigureAwait(false);
            }
            await this.RemoveItems(PlaylistItemStatus.Remove).ConfigureAwait(false);
        }

        protected virtual async Task RemoveItems(PlaylistItemStatus status)
        {
            await this.SetIsIndeterminate(true).ConfigureAwait(false);
            Logger.Write(this, LogLevel.Debug, "Removing playlist items.");
            using (var task = new SingletonReentrantTask(this, ComponentSlots.Database, SingletonReentrantTask.PRIORITY_HIGH, async cancellationToken =>
            {
                using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
                {
                    await this.Database.ExecuteAsync(this.Database.Queries.RemovePlaylistItems, (parameters, phase) =>
                    {
                        switch (phase)
                        {
                            case DatabaseParameterPhase.Fetch:
                                parameters["status"] = status;
                                break;
                        }
                    }, transaction).ConfigureAwait(false);
                    transaction.Commit();
                }
            }))
            {
                await task.Run().ConfigureAwait(false);
            }
        }

        protected virtual async Task ShiftItems(QueryOperator @operator, int at, int by)
        {
            Logger.Write(
                this,
                LogLevel.Debug,
                "Shifting playlist items at position {0} {1} by offset {2}.",
                Enum.GetName(typeof(QueryOperator), @operator),
                at,
                by
            );
            await this.SetIsIndeterminate(true).ConfigureAwait(false);
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
            using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
            {
                await this.Database.ExecuteAsync(query, (parameters, phase) =>
                {
                    switch (phase)
                    {
                        case DatabaseParameterPhase.Fetch:
                            parameters["status"] = PlaylistItemStatus.None;
                            parameters["sequence"] = at;
                            parameters["offset"] = by;
                            break;
                    }
                }, transaction).ConfigureAwait(false);
                transaction.Commit();
            }
        }

        protected virtual async Task SequenceItems()
        {
            Logger.Write(this, LogLevel.Debug, "Sequencing playlist items.");
            await this.SetIsIndeterminate(true).ConfigureAwait(false);
            using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
            {
                await this.Database.ExecuteAsync(this.Database.Queries.SequencePlaylistItems, (parameters, phase) =>
                {
                    switch (phase)
                    {
                        case DatabaseParameterPhase.Fetch:
                            parameters["status"] = PlaylistItemStatus.Import;
                            break;
                    }
                }, transaction).ConfigureAwait(false);
                transaction.Commit();
            }
        }

        protected virtual async Task SetPlaylistItemsStatus(PlaylistItemStatus status)
        {
            Logger.Write(this, LogLevel.Debug, "Setting playlist status: {0}", Enum.GetName(typeof(LibraryItemStatus), LibraryItemStatus.None));
            await this.SetIsIndeterminate(true).ConfigureAwait(false);
            var query = this.Database.QueryFactory.Build();
            query.Update.SetTable(this.Database.Tables.PlaylistItem);
            query.Update.AddColumn(this.Database.Tables.PlaylistItem.Column("Status"));
            using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
            {
                await this.Database.ExecuteAsync(query, (parameters, phase) =>
                {
                    switch (phase)
                    {
                        case DatabaseParameterPhase.Fetch:
                            parameters["status"] = status;
                            break;
                    }
                }, transaction).ConfigureAwait(false);
                transaction.Commit();
            }
        }

        public static async Task SetPlaylistItemStatus(IDatabaseComponent database, int playlistItemId, PlaylistItemStatus status)
        {
            var builder = database.QueryFactory.Build();
            builder.Update.AddColumn(database.Tables.PlaylistItem.Column("Status"));
            builder.Update.SetTable(database.Tables.PlaylistItem);
            builder.Filter.AddColumn(database.Tables.PlaylistItem.Column("Id"));
            using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
            {
                await database.ExecuteAsync(builder, (parameters, phase) =>
                {
                    switch (phase)
                    {
                        case DatabaseParameterPhase.Fetch:
                            parameters["status"] = status;
                            parameters["id"] = playlistItemId;
                            break;
                    }
                }, transaction).ConfigureAwait(false);
                transaction.Commit();
            }
        }

        protected override void OnDisposing()
        {
            if (this.Database != null)
            {
                this.Database.Dispose();
            }
            base.OnDisposing();
        }
    }
}
