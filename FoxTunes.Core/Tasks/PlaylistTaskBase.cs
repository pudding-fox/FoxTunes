#pragma warning disable 612, 618
using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
        }

        public int Sequence { get; protected set; }

        public int Offset { get; protected set; }

        public ICore Core { get; private set; }

        public IDatabaseComponent Database { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public IMetaDataSourceFactory MetaDataSourceFactory { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public IScriptingRuntime ScriptingRuntime { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.Database = core.Factories.Database.Create();
            this.PlaybackManager = core.Managers.Playback;
            this.MetaDataSourceFactory = core.Factories.MetaDataSource;
            this.SignalEmitter = core.Components.SignalEmitter;
            this.ScriptingRuntime = core.Components.ScriptingRuntime;
            base.InitializeComponent(core);
        }

        protected virtual async Task AddPaths(IEnumerable<string> paths)
        {
            using (var task = new SingletonReentrantTask(ComponentSlots.Database, SingletonReentrantTask.PRIORITY_HIGH, async cancellationToken =>
            {
                await this.AddItems(paths);
                await this.ShiftItems(QueryOperator.GreaterOrEqual, this.Sequence, this.Offset);
                await this.AddOrUpdateMetaData(cancellationToken);
                await this.UpdateVariousArtists();
                await this.SequenceItems();
                await this.SetPlaylistItemsStatus(PlaylistItemStatus.None);
            }))
            {
                await task.Run();
            }
        }

        protected virtual async Task AddItems(IEnumerable<string> paths)
        {
            using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
            {
                using (var playlistPopulator = new PlaylistPopulator(this.Database, this.PlaybackManager, this.Sequence, this.Offset, true, transaction))
                {
                    playlistPopulator.InitializeComponent(this.Core);
                    playlistPopulator.NameChanged += (sender, e) => this.Name = playlistPopulator.Name;
                    playlistPopulator.DescriptionChanged += (sender, e) => this.Description = playlistPopulator.Description;
                    playlistPopulator.PositionChanged += (sender, e) => this.Position = playlistPopulator.Position;
                    playlistPopulator.CountChanged += (sender, e) => this.Count = playlistPopulator.Count;
                    await playlistPopulator.Populate(paths);
                    this.Offset = playlistPopulator.Offset;
                }
                transaction.Commit();
            }
        }

        protected virtual async Task AddOrUpdateMetaData(CancellationToken cancellationToken)
        {
            using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
            {
                var query = this.Database
                   .AsQueryable<PlaylistItem>(this.Database.Source(new DatabaseQueryComposer<PlaylistItem>(this.Database), transaction))
                   .Where(playlistItem => playlistItem.Status == PlaylistItemStatus.Import && !playlistItem.MetaDatas.Any());
                using (var metaDataPopulator = new MetaDataPopulator(this.Database, this.Database.Queries.AddPlaylistMetaDataItems, true, transaction))
                {
                    metaDataPopulator.InitializeComponent(this.Core);
                    metaDataPopulator.NameChanged += (sender, e) => this.Name = metaDataPopulator.Name;
                    metaDataPopulator.DescriptionChanged += (sender, e) => this.Description = metaDataPopulator.Description;
                    metaDataPopulator.PositionChanged += (sender, e) => this.Position = metaDataPopulator.Position;
                    metaDataPopulator.CountChanged += (sender, e) => this.Count = metaDataPopulator.Count;
                    await metaDataPopulator.Populate(query, cancellationToken);
                }
                transaction.Commit();
            }
        }

        protected virtual async Task RemoveItems(IEnumerable<PlaylistItem> playlistItems)
        {
            using (var task = new SingletonReentrantTask(ComponentSlots.Database, SingletonReentrantTask.PRIORITY_HIGH, async cancellationToken =>
            {
                using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
                {
                    foreach (var playlistItem in playlistItems)
                    {
                        playlistItem.Status = PlaylistItemStatus.Remove;
                    }
                    var set = this.Database.Set<PlaylistItem>(transaction);
                    await set.AddOrUpdateAsync(playlistItems);
                    transaction.Commit();
                }
            }))
            {
                await task.Run();
            }
            await this.RemoveItems(PlaylistItemStatus.Remove);
        }

        protected virtual async Task RemoveItems(PlaylistItemStatus status)
        {
            this.IsIndeterminate = true;
            Logger.Write(this, LogLevel.Debug, "Removing playlist items.");
            using (var task = new SingletonReentrantTask(ComponentSlots.Database, SingletonReentrantTask.PRIORITY_HIGH, async cancellationToken =>
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
                    }, transaction);
                    transaction.Commit();
                }
            }))
            {
                await task.Run();
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
                }, transaction);
                transaction.Commit();
            }
        }

        protected virtual async Task SequenceItems()
        {
            Logger.Write(this, LogLevel.Debug, "Sequencing playlist items.");
            this.IsIndeterminate = true;
            using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
            {
                var metaDataNames = MetaDataInfo.GetMetaDataNames(this.Database, transaction);
                await this.Database.ExecuteAsync(this.Database.Queries.BeginSequencePlaylistItems, transaction);
                using (var reader = this.Database.ExecuteReader(this.Database.Queries.SequencePlaylistItems(metaDataNames), (parameters, phase) =>
                {
                    switch (phase)
                    {
                        case DatabaseParameterPhase.Fetch:
                            parameters["status"] = PlaylistItemStatus.Import;
                            break;
                    }
                }, transaction))
                {
                    await this.SequenceItems(reader, transaction);
                }
                await this.Database.ExecuteAsync(this.Database.Queries.EndSequencePlaylistItems, (parameters, phase) =>
                {
                    switch (phase)
                    {
                        case DatabaseParameterPhase.Fetch:
                            parameters["status"] = PlaylistItemStatus.Import;
                            break;
                    }
                }, transaction);
                transaction.Commit();
            }
        }

        protected virtual async Task SequenceItems(IDatabaseReader reader, ITransactionSource transaction)
        {
            using (var playlistSequencePopulator = new PlaylistSequencePopulator(this.Database, transaction))
            {
                playlistSequencePopulator.InitializeComponent(this.Core);
                await playlistSequencePopulator.Populate(reader, CancellationToken.None);
            }
        }

        protected virtual async Task SetPlaylistItemsStatus(PlaylistItemStatus status)
        {
            Logger.Write(this, LogLevel.Debug, "Setting playlist status: {0}", Enum.GetName(typeof(LibraryItemStatus), LibraryItemStatus.None));
            this.IsIndeterminate = true;
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
                }, transaction);
                transaction.Commit();
            }
        }

        protected virtual async Task UpdateVariousArtists()
        {
            using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
            {
                await this.Database.ExecuteAsync(this.Database.Queries.UpdatePlaylistVariousArtists, (parameters, phase) =>
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
                transaction.Commit();
            }
        }

        protected override void OnDisposing()
        {
            this.Database.Dispose();
            base.OnDisposing();
        }
    }
}
