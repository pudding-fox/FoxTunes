#pragma warning disable 612, 618
using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public abstract class LibraryTaskBase : BackgroundTask
    {
        public const string ID = "B6AF297E-F334-481D-8D60-BD5BE5935BD9";

        protected LibraryTaskBase()
            : base(ID)
        {
        }

        public ICore Core { get; private set; }

        public IDatabaseComponent Database { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public IMetaDataSourceFactory MetaDataSourceFactory { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.Database = core.Factories.Database.Create();
            this.PlaybackManager = core.Managers.Playback;
            this.MetaDataSourceFactory = core.Factories.MetaDataSource;
            this.SignalEmitter = core.Components.SignalEmitter;
            base.InitializeComponent(core);
        }

        protected virtual async Task<IEnumerable<string>> GetRoots()
        {
            using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
            {
                using (var sequence = this.Database.Set<LibraryRoot>(transaction).GetAsyncEnumerator())
                {
                    var result = new List<string>();
                    while (await sequence.MoveNextAsync())
                    {
                        result.Add(sequence.Current.DirectoryName);
                    }
                    return result;
                }
            }
        }

        protected virtual async Task AddRoots(IEnumerable<string> paths)
        {
            paths = paths.Except(
                await this.GetRoots()
            ).ToArray();
            using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
            {
                var set = this.Database.Set<LibraryRoot>(transaction);
                foreach (var path in paths)
                {
                    Logger.Write(this, LogLevel.Debug, "Adding library root: {0}", path);
                    await set.AddAsync(
                        set.Create().With(
                            libraryRoot => libraryRoot.DirectoryName = path
                        )
                    );
                }
                if (transaction.HasTransaction)
                {
                    transaction.Commit();
                }
            }
        }

        protected virtual async Task ClearRoots()
        {
            using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
            {
                var set = this.Database.Set<LibraryRoot>(transaction);
                Logger.Write(this, LogLevel.Debug, "Clearing library roots.");
                await set.ClearAsync();
                transaction.Commit();
            }
        }

        protected virtual async Task AddPaths(IEnumerable<string> paths, bool buildHierarchies)
        {
            var complete = true;
            using (var task = new SingletonReentrantTask(this, ComponentSlots.Database, SingletonReentrantTask.PRIORITY_LOW, async cancellationToken =>
             {
                 await this.AddLibraryItems(paths, cancellationToken);
                 if (cancellationToken.IsCancellationRequested)
                 {
                     await this.SetName("Waiting..");
                     await this.SetDescription(string.Empty);
                 }
             }))
            {
                await task.Run();
            }
            if (this.IsCancellationRequested)
            {
                //Reset cancellation as the next phases should finish quickly.
                //Cancelling again will still work.
                this.IsCancellationRequested = false;
                complete = false;
            }
            using (var task = new SingletonReentrantTask(this, ComponentSlots.Database, SingletonReentrantTask.PRIORITY_LOW, async cancellationToken =>
            {
                await this.AddOrUpdateMetaData(cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                {
                    await this.SetName("Waiting..");
                    await this.SetDescription(string.Empty);
                }
            }))
            {
                await task.Run();
            }
            if (this.IsCancellationRequested)
            {
                //Reset cancellation as the next phases should finish quickly.
                //Cancelling again will still work.
                this.IsCancellationRequested = false;
                complete = false;
            }
            if (buildHierarchies)
            {
                await this.BuildHierarchies(LibraryItemStatus.Import);
            }
            if (complete)
            {
                await SetLibraryItemsStatus(this.Database, LibraryItemStatus.None);
            }
        }

        protected virtual async Task AddLibraryItems(IEnumerable<string> paths, CancellationToken cancellationToken)
        {
            //We don't know how many files we're about to enumerate.
            if (!this.IsIndeterminate)
            {
                await this.SetIsIndeterminate(true);
            }
            using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
            {
                using (var libraryPopulator = new LibraryPopulator(this.Database, this.PlaybackManager, this.Visible, transaction))
                {
                    libraryPopulator.InitializeComponent(this.Core);
                    await this.WithSubTask(libraryPopulator,
                        async () => await libraryPopulator.Populate(paths, cancellationToken)
                    );
                }
                transaction.Commit();
            }
        }

        protected virtual async Task AddOrUpdateMetaData(CancellationToken cancellationToken)
        {
            using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
            {
                using (var metaDataPopulator = new LibraryMetaDataPopulator(this.Database, this.Visible, transaction))
                {
                    metaDataPopulator.InitializeComponent(this.Core);
                    await this.WithSubTask(metaDataPopulator,
                        async () => await metaDataPopulator.Populate(LibraryItemStatus.Import, cancellationToken)
                    );
                }
                transaction.Commit();
            }
        }

        protected virtual async Task BuildHierarchies(LibraryItemStatus? status)
        {
            using (var task = new SingletonReentrantTask(this, ComponentSlots.Database, SingletonReentrantTask.PRIORITY_LOW, async cancellationToken =>
            {
                using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
                {
                    await this.AddHiearchies(status, cancellationToken, transaction);
                    if (cancellationToken.IsCancellationRequested)
                    {
                        await this.SetName("Waiting..");
                        await this.SetDescription(string.Empty);
                    }
                    else
                    {
                        await this.SetDescription("Finalizing");
                        await this.SetIsIndeterminate(true);
                    }
                    transaction.Commit();
                }
            }))
            {
                await task.Run();
            }
        }

        private async Task AddHiearchies(LibraryItemStatus? status, CancellationToken cancellationToken, ITransactionSource transaction)
        {
            using (var libraryHierarchyPopulator = new LibraryHierarchyPopulator(this.Database, this.Visible, transaction))
            {
                libraryHierarchyPopulator.InitializeComponent(this.Core);
                await this.WithSubTask(libraryHierarchyPopulator,
                    async () => await libraryHierarchyPopulator.Populate(status, cancellationToken)
                );
            }
        }

        protected virtual async Task RemoveHierarchies(LibraryItemStatus? status)
        {
            using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
            {
                await this.Database.ExecuteAsync(this.Database.Queries.RemoveLibraryHierarchyItems, (parameters, phase) =>
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

        protected virtual async Task RemoveItems(LibraryItemStatus? status)
        {
            using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
            {
                await this.Database.ExecuteAsync(this.Database.Queries.RemoveLibraryItems, (parameters, phase) =>
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

        public static async Task SetLibraryItemsStatus(IDatabaseComponent database, LibraryItemStatus status)
        {
            var query = database.QueryFactory.Build();
            query.Update.SetTable(database.Tables.LibraryItem);
            query.Update.AddColumn(database.Tables.LibraryItem.Column("Status"));
            using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
            {
                await database.ExecuteAsync(query, (parameters, phase) =>
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

        public static async Task SetLibraryItemStatus(IDatabaseComponent database, int libraryItemId, LibraryItemStatus status)
        {
            var builder = database.QueryFactory.Build();
            builder.Update.AddColumn(database.Tables.LibraryItem.Column("Status"));
            builder.Update.SetTable(database.Tables.LibraryItem);
            builder.Filter.AddColumn(database.Tables.LibraryItem.Column("Id"));
            using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
            {
                await database.ExecuteAsync(builder, (parameters, phase) =>
                {
                    switch (phase)
                    {
                        case DatabaseParameterPhase.Fetch:
                            parameters["status"] = status;
                            parameters["id"] = libraryItemId;
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
