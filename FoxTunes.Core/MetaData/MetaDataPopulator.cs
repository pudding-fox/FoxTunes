using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class MetaDataPopulator : PopulatorBase
    {
        public const string ID = "EA40EA65-6F49-48A6-9469-DD5FC2E36EC0";

        public readonly object SyncRoot = new object();

        public MetaDataPopulator(IDatabase database, IDatabaseQuery query, bool reportProgress, ITransactionSource transaction)
            : base(reportProgress)
        {
            this.Database = database;
            this.Transaction = transaction;
            this.Query = query;
            this.Semaphore = new SemaphoreSlim(1, 1);
            this.Writer = new MetaDataWriter(this.Database, this.Query, this.Transaction);
        }

        public IDatabase Database { get; private set; }

        public ITransactionSource Transaction { get; private set; }

        public IDatabaseQuery Query { get; private set; }

        public IMetaDataSourceFactory MetaDataSourceFactory { get; private set; }

        private SemaphoreSlim Semaphore { get; set; }

        private MetaDataWriter Writer { get; set; }

        public override void InitializeComponent(ICore core)
        {
            this.MetaDataSourceFactory = core.Factories.MetaDataSource;
            base.InitializeComponent(core);
        }

        public async Task Populate<T>(IEnumerable<T> fileDatas, CancellationToken cancellationToken) where T : IFileData
        {
            Logger.Write(this, LogLevel.Debug, "Begin populating meta data.");

            if (this.ReportProgress)
            {
                this.Name = "Populating meta data";
                this.Position = 0;
                this.Count = fileDatas.Count();
            }

            var interval = Math.Max(Convert.ToInt32(this.Count * 0.01), 1);
            var position = 0;
            var metaDataSource = this.MetaDataSourceFactory.Create();

            await AsyncParallel.ForEach(fileDatas, async fileData =>
            {
                Logger.Write(this, LogLevel.Debug, "Populating meta data for file: {0} => {1}", fileData.Id, fileData.FileName);

                var metaData = await metaDataSource.GetMetaData(fileData.FileName);

                await this.Semaphore.WaitAsync();
                try
                {
                    foreach (var metaDataItem in metaData)
                    {
                        await this.Writer.Write(fileData.Id, metaDataItem);
                    }
                }
                finally
                {
                    this.Semaphore.Release();
                }

                if (this.ReportProgress)
                {
                    if (position % interval == 0)
                    {
                        lock (this.SyncRoot)
                        {
                            this.Description = new FileInfo(fileData.FileName).Name;
                            this.Position = position;
                        }
                    }
                    Interlocked.Increment(ref position);
                }
            }, cancellationToken, this.ParallelOptions);
        }

        protected override void OnDisposing()
        {
            this.Semaphore.Dispose();
            this.Writer.Dispose();
            base.OnDisposing();
        }
    }
}
