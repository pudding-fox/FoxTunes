using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class AddPathsToPlaylistTask : PlaylistTaskBase
    {
        public const string ID = "7B564369-A6A0-4BAF-8C99-08AF27908591";

        public AddPathsToPlaylistTask(int sequence, IEnumerable<string> paths, bool clear)
            : base(ID, sequence)
        {
            this.Paths = paths;
            this.Clear = clear;
        }

        public override bool Visible
        {
            get
            {
                return true;
            }
        }

        public IEnumerable<string> Paths { get; private set; }

        public bool Clear { get; private set; }

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
            using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
            {
                if (this.Clear)
                {
                    await this.RemoveItems(PlaylistItemStatus.None, transaction);
                }
                await this.AddPlaylistItems(transaction);
                await this.ShiftItems(QueryOperator.GreaterOrEqual, this.Sequence, this.Offset, transaction);
                using (var task = new SingletonReentrantTask(MetaDataPopulator.ID, SingletonReentrantTask.PRIORITY_HIGH, async cancellationToken =>
                {
                    await this.AddOrUpdateMetaData(cancellationToken, transaction);
                    if (cancellationToken.IsCancellationRequested)
                    {
                        this.Name = "Waiting..";
                        this.Description = string.Empty;
                        return;
                    }
                    await this.UpdateVariousArtists(transaction);
                    await this.SequenceItems(cancellationToken, transaction);
                    await this.SetPlaylistItemsStatus(transaction);
                    transaction.Commit();
                }))
                {
                    await task.Run();
                }
            }
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistUpdated));
        }

        protected virtual async Task AddPlaylistItems(ITransactionSource transaction)
        {
            using (var playlistPopulator = new PlaylistPopulator(this.Database, this.PlaybackManager, this.Sequence, this.Offset, false, transaction))
            {
                await playlistPopulator.Populate(this.Paths);
                this.Offset = playlistPopulator.Offset;
            }
        }

        protected virtual async Task AddOrUpdateMetaData(CancellationToken cancellationToken, ITransactionSource transaction)
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
        }
    }
}
