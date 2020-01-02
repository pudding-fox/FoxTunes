using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class RefreshLibraryMetaDataTask : BackgroundTask
    {
        const MetaDataItemType META_DATA_TYPE = MetaDataItemType.Tag | MetaDataItemType.Image;

        public const string ID = "C595F0CE-9DF7-43F2-9B1A-32D2F2C97F22";

        public RefreshLibraryMetaDataTask(IEnumerable<LibraryItem> libraryItems) : base(ID)
        {
            this.LibraryItems = libraryItems;
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

        public IEnumerable<LibraryItem> LibraryItems { get; private set; }

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
            await this.SetName("Refreshing meta data").ConfigureAwait(false);
            await this.SetPosition(0).ConfigureAwait(false);
            await this.SetCount(this.LibraryItems.Count()).ConfigureAwait(false);
            await base.OnStarted().ConfigureAwait(false);
        }

        protected override async Task OnRun()
        {
            var position = 0;
            using (var task = new SingletonReentrantTask(this, ComponentSlots.Database, SingletonReentrantTask.PRIORITY_LOW, async cancellationToken =>
            {
                var metaDataSource = this.MetaDataSourceFactory.Create();
                foreach (var libraryItem in this.LibraryItems)
                {
                    await this.SetDescription(new FileInfo(libraryItem.FileName).Name).ConfigureAwait(false);
                    await this.SetPosition(position).ConfigureAwait(false);

                    if (!File.Exists(libraryItem.FileName))
                    {
                        Logger.Write(this, LogLevel.Debug, "File \"{0}\" no longer exists: Cannot refresh.", libraryItem.FileName);
                        continue;
                    }

                    libraryItem.MetaDatas = new ObservableCollection<MetaDataItem>(
                        await metaDataSource.GetMetaData(libraryItem.FileName)
.ConfigureAwait(false));

                    await this.WriteLibraryMetaData(libraryItem).ConfigureAwait(false);
                    await LibraryTaskBase.SetLibraryItemStatus(this.Database, libraryItem.Id, LibraryItemStatus.Import).ConfigureAwait(false);

                    position++;
                }
            }))
            {
                await task.Run().ConfigureAwait(false);
            }
        }

        private async Task WriteLibraryMetaData(LibraryItem libraryItem)
        {
            using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
            {
                await this.Database.ExecuteAsync(this.Database.Queries.ClearLibraryMetaDataItems, (parameters, phase) =>
                {
                    switch (phase)
                    {
                        case DatabaseParameterPhase.Fetch:
                            parameters["itemId"] = libraryItem.Id;
                            parameters["type"] = META_DATA_TYPE;
                            break;
                    }
                }, transaction).ConfigureAwait(false);

                using (var writer = new MetaDataWriter(this.Database, this.Database.Queries.AddLibraryMetaDataItem, transaction))
                {
                    await writer.Write(libraryItem.Id, libraryItem.MetaDatas).ConfigureAwait(false);
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
