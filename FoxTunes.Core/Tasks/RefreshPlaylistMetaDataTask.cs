using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class RefreshPlaylistMetaDataTask : BackgroundTask
    {
        const MetaDataItemType META_DATA_TYPE = MetaDataItemType.Tag | MetaDataItemType.Image;

        public const string ID = "8DB1257E-5854-4F8F-BAE3-D59A45DEE998";

        public RefreshPlaylistMetaDataTask(IEnumerable<PlaylistItem> playlistItems)
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
            this.Name = "Refreshing meta data";
            this.Position = 0;
            this.Count = this.PlaylistItems.Count();
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
                    this.Description = Path.GetFileName(playlistItem.FileName);
                    this.Position = position;

                    if (!File.Exists(playlistItem.FileName))
                    {
                        Logger.Write(this, LogLevel.Debug, "File \"{0}\" no longer exists: Cannot refresh.", playlistItem.FileName);
                        continue;
                    }

                    playlistItem.MetaDatas = new ObservableCollection<MetaDataItem>(
                        await metaDataSource.GetMetaData(playlistItem.FileName).ConfigureAwait(false)
                    );

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
                    await writer.Write(
                        playlistItem.Id,
                        playlistItem.MetaDatas,
                        metaDataItem => META_DATA_TYPE.HasFlag(metaDataItem.Type)
                    ).ConfigureAwait(false);
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
                    await writer.Write(
                        playlistItem.LibraryItem_Id.Value,
                        playlistItem.MetaDatas,
                        metaDataItem => META_DATA_TYPE.HasFlag(metaDataItem.Type)
                    ).ConfigureAwait(false);
                }

                //Update the import date otherwise the file might be re-scanned and changes lost.
                var libraryItem = new LibraryItem()
                {
                    Id = playlistItem.LibraryItem_Id.Value
                };
                await LibraryTaskBase.SetLibraryItemImportDate(this.Database, libraryItem, DateTime.UtcNow, transaction).ConfigureAwait(false);

                if (transaction.HasTransaction)
                {
                    transaction.Commit();
                }
            }
        }

        protected override async Task OnCompleted()
        {
            await base.OnCompleted().ConfigureAwait(false);
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.MetaDataUpdated)).ConfigureAwait(false);
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
