#pragma warning disable 612, 618
using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class AddPathsToLibraryTask : LibraryTaskBase
    {
        public const string ID = "972222C8-8F6E-44CF-8EBE-DA4FCFD7CD80";

        public AddPathsToLibraryTask(IEnumerable<string> paths)
            : base(ID)
        {
            this.Paths = paths;
        }

        public override bool Visible
        {
            get
            {
                return true;
            }
        }

        public IEnumerable<string> Paths { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public IMetaDataSourceFactory MetaDataSourceFactory { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.PlaybackManager = core.Managers.Playback;
            this.MetaDataSourceFactory = core.Factories.MetaDataSource;
            base.InitializeComponent(core);
        }

        protected override Task OnStarted()
        {
            this.Name = "Getting file list";
            this.IsIndeterminate = true;
            return base.OnStarted();
        }

        protected override async Task OnRun()
        {
            using (var transaction = this.Database.BeginTransaction(IsolationLevel.ReadUncommitted))
            {
                await this.AddLibraryItems(transaction);
                using (var task = new SingletonReentrantTask(MetaDataPopulator.ID, SingletonReentrantTask.PRIORITY_LOW, async cancellationToken =>
                {
                    await this.AddOrUpdateMetaData(cancellationToken, transaction);
                    if (cancellationToken.IsCancellationRequested)
                    {
                        this.Name = "Waiting..";
                        this.Description = string.Empty;
                        return;
                    }
                    await this.UpdateVariousArtists(transaction);
                    await this.SetLibraryItemsStatus(transaction);
                    transaction.Commit();
                }))
                {
                    await task.Run();
                }
            }
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.LibraryUpdated));
        }

        protected virtual async Task AddLibraryItems(ITransactionSource transaction)
        {
            using (var libraryPopulator = new LibraryPopulator(this.Database, this.PlaybackManager, false, transaction))
            {
                await libraryPopulator.Populate(this.Paths);
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
    }
}
