using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System;
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

        public WriteLibraryMetaDataTask(IEnumerable<LibraryItem> libraryItems, bool writeToFiles, IEnumerable<string> names) : base(ID)
        {
            this.LibraryItems = libraryItems;
            this.Names = names;
            this.WriteToFiles = writeToFiles;
            this.Errors = new Dictionary<LibraryItem, Exception>();
        }

        public override bool Visible
        {
            get
            {
                return this.LibraryItems.Count() > 1;
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

        public IEnumerable<string> Names { get; private set; }

        public bool WriteToFiles { get; private set; }

        public IDictionary<LibraryItem, Exception> Errors { get; private set; }

        public IDatabaseComponent Database { get; private set; }

        public IMetaDataSourceFactory MetaDataSourceFactory { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public SelectionConfigurationElement Write { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Database = core.Factories.Database.Create();
            this.MetaDataSourceFactory = core.Factories.MetaDataSource;
            this.SignalEmitter = core.Components.SignalEmitter;
            this.Configuration = core.Components.Configuration;
            this.Write = this.Configuration.GetElement<SelectionConfigurationElement>(
                MetaDataBehaviourConfiguration.SECTION,
                MetaDataBehaviourConfiguration.WRITE_ELEMENT
            );
            base.InitializeComponent(core);
        }

        protected override async Task OnStarted()
        {
            if (this.Visible)
            {
                await this.SetName("Saving meta data").ConfigureAwait(false);
                await this.SetPosition(0).ConfigureAwait(false);
                await this.SetCount(this.LibraryItems.Count()).ConfigureAwait(false);
            }
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
                    if (this.IsCancellationRequested)
                    {
                        break;
                    }

                    if (this.Visible)
                    {
                        await this.SetDescription(Path.GetFileName(libraryItem.FileName)).ConfigureAwait(false);
                        await this.SetPosition(position).ConfigureAwait(false);
                    }

                    try
                    {
                        lock (libraryItem.MetaDatas)
                        {
                            foreach (var metaDataItem in libraryItem.MetaDatas.ToArray())
                            {
                                if (!string.IsNullOrEmpty(metaDataItem.Value))
                                {
                                    continue;
                                }
                                libraryItem.MetaDatas.Remove(metaDataItem);
                            }
                        }

                        await this.WriteLibraryMetaData(libraryItem).ConfigureAwait(false);
                        await LibraryTaskBase.SetLibraryItemStatus(this.Database, libraryItem.Id, LibraryItemStatus.Import).ConfigureAwait(false);

                        if (!this.WriteToFiles)
                        {
                            //Task was configured not to write to files.
                            position++;
                            continue;
                        }

                        if (MetaDataBehaviourConfiguration.GetWriteBehaviour(this.Write.Value) == WriteBehaviour.None)
                        {
                            Logger.Write(this, LogLevel.Warn, "Writing is disabled: {0}", libraryItem.FileName);
                            position++;
                            continue;
                        }

                        if (!FileSystemHelper.IsLocalFile(libraryItem.FileName))
                        {
                            Logger.Write(this, LogLevel.Debug, "File \"{0}\" is not a local file: Cannot update.", libraryItem.FileName);
                            this.Errors.Add(libraryItem, new FileNotFoundException(string.Format("File \"{0}\" is not a local file: Cannot update.", libraryItem.FileName)));
                            position++;
                            continue;
                        }

                        if (!File.Exists(libraryItem.FileName))
                        {
                            Logger.Write(this, LogLevel.Debug, "File \"{0}\" no longer exists: Cannot update.", libraryItem.FileName);
                            this.Errors.Add(libraryItem, new FileNotFoundException(string.Format("File \"{0}\" no longer exists: Cannot update.", libraryItem.FileName)));
                            position++;
                            continue;
                        }

                        await metaDataSource.SetMetaData(
                            libraryItem.FileName,
                            libraryItem.MetaDatas,
                            metaDataItem => this.Names == null || !this.Names.Any() || this.Names.Contains(metaDataItem.Name, true)
                        ).ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        this.Errors.Add(libraryItem, e);
                        position++;
                        continue;
                    }

                    position++;
                }
            }))
            {
                await task.Run().ConfigureAwait(false);
            }
        }

        protected override async Task OnCompleted()
        {
            await base.OnCompleted().ConfigureAwait(false);
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.MetaDataUpdated, this.Names)).ConfigureAwait(false);
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
                    await writer.Write(
                        libraryItem.Id,
                        libraryItem.MetaDatas,
                        metaDataItem => META_DATA_TYPE.HasFlag(metaDataItem.Type)
                    ).ConfigureAwait(false);
                }

                if (transaction.HasTransaction)
                {
                    transaction.Commit();
                }
            }
        }

        protected override void OnDisposing()
        {
            this.Database.Dispose();
            base.OnDisposing();
        }
    }
}
