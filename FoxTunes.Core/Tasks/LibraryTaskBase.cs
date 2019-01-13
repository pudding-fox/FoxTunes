#pragma warning disable 612, 618
using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System;
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

        protected virtual async Task AddPaths(IEnumerable<string> paths)
        {
            using (var task = new SingletonReentrantTask(ComponentSlots.Database, SingletonReentrantTask.PRIORITY_LOW, async cancellationToken =>
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
            using (var task = new SingletonReentrantTask(ComponentSlots.Database, SingletonReentrantTask.PRIORITY_LOW, async cancellationToken =>
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
            await this.UpdateVariousArtists();
            await this.SetLibraryItemsStatus(LibraryItemStatus.None);
        }

        protected virtual async Task AddLibraryItems(IEnumerable<string> paths, CancellationToken cancellationToken)
        {
            using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
            {
                using (var libraryPopulator = new LibraryPopulator(this.Database, this.PlaybackManager, this.Visible, transaction))
                {
                    libraryPopulator.InitializeComponent(this.Core);
                    await this.WithPopulator(libraryPopulator, async () => await libraryPopulator.Populate(paths, cancellationToken));
                }
                transaction.Commit();
            }
        }

        protected virtual async Task AddOrUpdateMetaData(CancellationToken cancellationToken)
        {
            using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
            {
                var query = this.Database
                    .AsQueryable<LibraryItem>(this.Database.Source(new DatabaseQueryComposer<LibraryItem>(this.Database), transaction))
                    .Where(libraryItem => libraryItem.Status == LibraryItemStatus.Import && !libraryItem.MetaDatas.Any());
                using (var metaDataPopulator = new MetaDataPopulator(this.Database, this.Database.Queries.AddLibraryMetaDataItems, this.Visible, transaction))
                {
                    metaDataPopulator.InitializeComponent(this.Core);
                    await this.WithPopulator(metaDataPopulator, async () => await metaDataPopulator.Populate(query, cancellationToken));
                }
                transaction.Commit();
            }
        }

        protected virtual async Task BuildHierarchies()
        {
            using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
            {
                await this.Database.ExecuteAsync(this.Database.Queries.BeginBuildLibraryHierarchies, transaction);
                transaction.Commit();
            }
            using (var task = new SingletonReentrantTask(ComponentSlots.Database, SingletonReentrantTask.PRIORITY_LOW, async cancellationToken =>
            {
                var metaDataNames = default(IEnumerable<string>);
                using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
                {
                    metaDataNames = MetaDataInfo.GetMetaDataNames(this.Database, transaction).ToArray();
                }
                using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
                {
                    using (var reader = this.Database.ExecuteReader(this.Database.Queries.BuildLibraryHierarchies(metaDataNames), null, transaction))
                    {
                        await this.AddHiearchies(reader, cancellationToken, transaction);
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
                    }
                    transaction.Commit();
                }
            }))
            {
                await task.Run();
            }
            using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
            {
                await this.Database.ExecuteAsync(this.Database.Queries.EndBuildLibraryHierarchies, transaction);
                transaction.Commit();
            }
        }

        private async Task AddHiearchies(IDatabaseReader reader, CancellationToken cancellationToken, ITransactionSource transaction)
        {
            using (var libraryHierarchyPopulator = new LibraryHierarchyPopulator(this.Database, this.Visible, transaction))
            {
                libraryHierarchyPopulator.InitializeComponent(this.Core);
                await this.WithPopulator(libraryHierarchyPopulator, async () => await libraryHierarchyPopulator.Populate(reader, cancellationToken, transaction));
            }
        }

        protected virtual async Task RemoveHierarchies()
        {
            using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
            {
                await this.Database.ExecuteAsync(this.Database.Queries.RemoveLibraryHierarchyItems, transaction);
                transaction.Commit();
            }
        }

        protected virtual async Task RemoveItems(LibraryItemStatus status)
        {
            await this.SetIsIndeterminate(true);
            Logger.Write(this, LogLevel.Debug, "Removing library items.");
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

        protected virtual async Task SetLibraryItemsStatus(LibraryItemStatus status)
        {
            await this.SetIsIndeterminate(true);
            var query = this.Database.QueryFactory.Build();
            query.Update.SetTable(this.Database.Tables.LibraryItem);
            query.Update.AddColumn(this.Database.Tables.LibraryItem.Column("Status"));
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

        protected virtual async Task SetLibraryItemsStatus(Func<LibraryItem, bool> predicate, LibraryItemStatus status)
        {
            using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
            {
                var set = this.Database.Set<LibraryItem>(transaction);
                foreach (var libraryItem in set)
                {
                    if (!predicate(libraryItem))
                    {
                        continue;
                    }
                    libraryItem.Status = status;
                    await set.AddOrUpdateAsync(libraryItem);
                }
                transaction.Commit();
            }
        }

        protected virtual async Task UpdateVariousArtists()
        {
            using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
            {
                await this.Database.ExecuteAsync(this.Database.Queries.UpdateLibraryVariousArtists, (parameters, phase) =>
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
