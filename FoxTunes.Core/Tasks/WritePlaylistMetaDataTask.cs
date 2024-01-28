using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class WritePlaylistMetaDataTask : BackgroundTask
    {
        const MetaDataItemType META_DATA_TYPE = MetaDataItemType.Tag | MetaDataItemType.Image;

        public const string ID = "8DB1257E-5854-4F8F-BAE3-D59A45DEE998";

        public WritePlaylistMetaDataTask(IEnumerable<PlaylistItem> playlistItems)
            : base(ID)
        {
            this.PlaylistItems = playlistItems;
        }

        public override bool Visible
        {
            get
            {
                return true;
            }
        }

        public override bool Cancellable
        {
            get
            {
                return true;
            }
        }

        public IEnumerable<PlaylistItem> PlaylistItems { get; private set; }

        public IDatabaseComponent Database { get; private set; }

        public IMetaDataSourceFactory MetaDataSourceFactory { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Database = core.Factories.Database.Create();
            this.MetaDataSourceFactory = core.Factories.MetaDataSource;
            this.SignalEmitter = core.Components.SignalEmitter;
            base.InitializeComponent(core);
        }

        protected override async Task OnStarted()
        {
            await this.SetName("Saving meta data").ConfigureAwait(false);
            await this.SetPosition(0).ConfigureAwait(false);
            await this.SetCount(this.PlaylistItems.Count()).ConfigureAwait(false);
            await base.OnStarted().ConfigureAwait(false);
        }

        protected override async Task OnRun()
        {
            var position = 0;
            using (var task = new SingletonReentrantTask(this, ComponentSlots.Database, SingletonReentrantTask.PRIORITY_LOW, async cancellationToken =>
            {
                var metaDataSource = this.MetaDataSourceFactory.Create();
                foreach (var playlistItem in this.PlaylistItems)
                {
                    await this.SetDescription(new FileInfo(playlistItem.FileName).Name).ConfigureAwait(false);
                    await this.SetPosition(position).ConfigureAwait(false);

                    if (!File.Exists(playlistItem.FileName))
                    {
                        Logger.Write(this, LogLevel.Debug, "File \"{0}\" no longer exists: Cannot update.", playlistItem.FileName);
                        continue;
                    }

                    await metaDataSource.SetMetaData(playlistItem.FileName, playlistItem.MetaDatas).ConfigureAwait(false);

                    foreach (var metaDataItem in playlistItem.MetaDatas.ToArray())
                    {
                        if (!string.IsNullOrEmpty(metaDataItem.Value))
                        {
                            continue;
                        }
                        playlistItem.MetaDatas.Remove(metaDataItem);
                    }

                    if (!playlistItem.LibraryItem_Id.HasValue)
                    {
                        await this.WritePlaylistMetaData(playlistItem).ConfigureAwait(false);
                    }
                    else
                    {
                        await this.WriteLibraryMetaData(playlistItem).ConfigureAwait(false);
                        await LibraryTaskBase.SetLibraryItemStatus(this.Database, playlistItem.LibraryItem_Id.Value, LibraryItemStatus.Import).ConfigureAwait(false);
                    }

                    position++;
                }
            }))
            {
                await task.Run().ConfigureAwait(false);
            }
        }

        private async Task WritePlaylistMetaData(PlaylistItem playlistItem)
        {
            using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
            {
                await this.Database.ExecuteAsync(this.Database.Queries.ClearPlaylistMetaDataItems, (parameters, phase) =>
                {
                    switch (phase)
                    {
                        case DatabaseParameterPhase.Fetch:
                            parameters["itemId"] = playlistItem.Id;
                            parameters["type"] = META_DATA_TYPE;
                            break;
                    }
                }, transaction).ConfigureAwait(false);

                using (var writer = new MetaDataWriter(this.Database, this.Database.Queries.AddPlaylistMetaDataItem, transaction))
                {
                    await writer.Write(playlistItem.Id, playlistItem.MetaDatas).ConfigureAwait(false);
                }

                if (transaction.HasTransaction)
                {
                    transaction.Commit();
                }
            }
        }

        private async Task WriteLibraryMetaData(PlaylistItem playlistItem)
        {
            using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
            {
                await this.Database.ExecuteAsync(this.Database.Queries.ClearLibraryMetaDataItems, (parameters, phase) =>
                {
                    switch (phase)
                    {
                        case DatabaseParameterPhase.Fetch:
                            parameters["itemId"] = playlistItem.LibraryItem_Id.Value;
                            parameters["type"] = META_DATA_TYPE;
                            break;
                    }
                }, transaction).ConfigureAwait(false);

                using (var writer = new MetaDataWriter(this.Database, this.Database.Queries.AddLibraryMetaDataItem, transaction))
                {
                    await writer.Write(playlistItem.LibraryItem_Id.Value, playlistItem.MetaDatas).ConfigureAwait(false);
                }

                if (transaction.HasTransaction)
                {
                    transaction.Commit();
                }
            }
        }

        protected override async Task OnCompleted()
        {
            await base.OnCompleted().ConfigureAwait(false);
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistUpdated)).ConfigureAwait(false);
        }

        protected override void OnDisposing()
        {
            this.Database.Dispose();
            base.OnDisposing();
        }
    }
}
