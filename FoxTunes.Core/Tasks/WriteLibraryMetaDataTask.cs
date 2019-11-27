using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class WriteLibraryMetaDataTask : BackgroundTask
    {
        const MetaDataItemType META_DATA_TYPE = MetaDataItemType.Tag | MetaDataItemType.Image;

        public const string ID = "3EDED881-5AFD-46B2-AD53-78290E535F2E";

        public WriteLibraryMetaDataTask(IEnumerable<LibraryItem> libraryItems) : base(ID)
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
            await this.SetName("Saving meta data");
            await this.SetPosition(0);
            await this.SetCount(this.LibraryItems.Count());
            await base.OnStarted();
        }

        protected override async Task OnRun()
        {
            var position = 0;
            using (var task = new SingletonReentrantTask(this, ComponentSlots.Database, SingletonReentrantTask.PRIORITY_LOW, async cancellationToken =>
            {
                var metaDataSource = this.MetaDataSourceFactory.Create();
                foreach (var libraryItem in this.LibraryItems)
                {
                    await this.SetDescription(new FileInfo(libraryItem.FileName).Name);
                    await this.SetPosition(position);

                    if (!File.Exists(libraryItem.FileName))
                    {
                        Logger.Write(this, LogLevel.Debug, "File \"{0}\" no longer exists: Cannot update.", libraryItem.FileName);
                        continue;
                    }

                    await metaDataSource.SetMetaData(libraryItem.FileName, libraryItem.MetaDatas);

                    foreach (var metaDataItem in libraryItem.MetaDatas.ToArray())
                    {
                        if (!string.IsNullOrEmpty(metaDataItem.Value))
                        {
                            continue;
                        }
                        libraryItem.MetaDatas.Remove(metaDataItem);
                    }

                    await this.WriteLibraryMetaData(libraryItem);
                    await LibraryTaskBase.SetLibraryItemStatus(this.Database, libraryItem.Id, LibraryItemStatus.Import);

                    position++;
                }
            }))
            {
                await task.Run();
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
                }, transaction);

                using (var writer = new MetaDataWriter(this.Database, this.Database.Queries.AddLibraryMetaDataItem, transaction))
                {
                    await writer.Write(libraryItem.Id, libraryItem.MetaDatas);
                }

                if (transaction.HasTransaction)
                {
                    transaction.Commit();
                }
            }
        }

        protected override async Task OnCompleted()
        {
            await base.OnCompleted();
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistUpdated));
        }

        protected override void OnDisposing()
        {
            this.Database.Dispose();
            base.OnDisposing();
        }
    }
}
