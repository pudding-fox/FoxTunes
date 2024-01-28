#pragma warning disable 612, 618
using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public abstract class LibraryTaskBase : BackgroundTask
    {
        protected LibraryTaskBase(string id)
            : base(id)
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
                    this.Name = "Waiting..";
                    this.Description = string.Empty;
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
                    this.Name = "Waiting..";
                    this.Description = string.Empty;
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
                using (var libraryPopulator = new LibraryPopulator(this.Database, this.PlaybackManager, true, transaction))
                {
                    libraryPopulator.InitializeComponent(this.Core);
                    libraryPopulator.NameChanged += (sender, e) => this.Name = libraryPopulator.Name;
                    libraryPopulator.DescriptionChanged += (sender, e) => this.Description = libraryPopulator.Description;
                    libraryPopulator.PositionChanged += (sender, e) => this.Position = libraryPopulator.Position;
                    libraryPopulator.CountChanged += (sender, e) => this.Count = libraryPopulator.Count;
                    await libraryPopulator.Populate(paths, cancellationToken);
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
                using (var metaDataPopulator = new MetaDataPopulator(this.Database, this.Database.Queries.AddLibraryMetaDataItems, true, transaction))
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
                            this.Name = "Waiting..";
                            this.Description = string.Empty;
                        }
                        else
                        {
                            this.Description = "Finalizing";
                            this.IsIndeterminate = true;
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
            using (var libraryHierarchyPopulator = new LibraryHierarchyPopulator(this.Database, true, transaction))
            {
                libraryHierarchyPopulator.InitializeComponent(this.Core);
                libraryHierarchyPopulator.NameChanged += (sender, e) => this.Name = libraryHierarchyPopulator.Name;
                libraryHierarchyPopulator.DescriptionChanged += (sender, e) => this.Description = libraryHierarchyPopulator.Description;
                libraryHierarchyPopulator.PositionChanged += (sender, e) => this.Position = libraryHierarchyPopulator.Position;
                libraryHierarchyPopulator.CountChanged += (sender, e) => this.Count = libraryHierarchyPopulator.Count;
                await libraryHierarchyPopulator.Populate(reader, cancellationToken, transaction);
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
            this.IsIndeterminate = true;
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
            this.IsIndeterminate = true;
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
