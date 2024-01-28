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

        protected virtual async Task AddPaths(IEnumerable<string> paths, ITransactionSource transaction)
        {
            await this.AddLibraryItems(paths, transaction);
            using (var task = new SingletonReentrantTask(MetaDataPopulator.ID, SingletonReentrantTask.PRIORITY_LOW, async cancellationToken =>
            {
                await this.AddOrUpdateMetaData(cancellationToken, transaction);
                if (cancellationToken.IsCancellationRequested)
                {
                    this.Name = "Waiting..";
                    this.Description = string.Empty;
                }
            }))
            {
                await task.Run();
            }
            await this.UpdateVariousArtists(transaction);
            await this.SetLibraryItemsStatus(transaction);
        }

        protected virtual async Task AddLibraryItems(IEnumerable<string> paths, ITransactionSource transaction)
        {
            using (var libraryPopulator = new LibraryPopulator(this.Database, this.PlaybackManager, false, transaction))
            {
                await libraryPopulator.Populate(paths);
            }
        }

        protected virtual async Task AddOrUpdateMetaData(CancellationToken cancellationToken, ITransactionSource transaction)
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
        }

        protected virtual Task RemoveHierarchies(ITransactionSource transaction)
        {
            return this.Database.ExecuteAsync(this.Database.Queries.RemoveLibraryHierarchyItems);
        }

        protected virtual Task RemoveItems(LibraryItemStatus status, ITransactionSource transaction)
        {
            this.IsIndeterminate = true;
            Logger.Write(this, LogLevel.Debug, "Removing library items.");
            return this.Database.ExecuteAsync(this.Database.Queries.RemoveLibraryItems, (parameters, phase) =>
            {
                switch (phase)
                {
                    case DatabaseParameterPhase.Fetch:
                        parameters["status"] = status;
                        break;
                }
            }, transaction);
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

        protected override void OnDisposing()
        {
            this.Database.Dispose();
            base.OnDisposing();
        }
    }
}
