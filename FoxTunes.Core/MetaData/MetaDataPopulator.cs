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

        public MetaDataPopulator(IDatabaseComponent database, IDatabaseQuery query, bool reportProgress, ITransactionSource transaction)
            : base(reportProgress)
        {
            this.Database = database;
            this.Transaction = transaction;
            this.Query = query;
            this.Writer = new MetaDataWriter(this.Database, this.Query, this.Transaction);
        }

        public IDatabaseComponent Database { get; private set; }

        public ITransactionSource Transaction { get; private set; }

        public IDatabaseQuery Query { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public IntegerConfigurationElement Threads { get; private set; }

        public IMetaDataSourceFactory MetaDataSourceFactory { get; private set; }

        private MetaDataWriter Writer { get; set; }

        public override ParallelOptions ParallelOptions
        {
            get
            {
                return new ParallelOptions()
                {
                    MaxDegreeOfParallelism = this.Threads.Value
                };
            }
        }

        public override void InitializeComponent(ICore core)
        {
            this.Configuration = core.Components.Configuration;
            this.Threads = this.Configuration.GetElement<IntegerConfigurationElement>(
                MetaDataBehaviourConfiguration.SECTION,
                MetaDataBehaviourConfiguration.THREADS_ELEMENT
            );
            this.MetaDataSourceFactory = core.Factories.MetaDataSource;
            base.InitializeComponent(core);
        }

        public async Task Populate<T>(IEnumerable<T> fileDatas, CancellationToken cancellationToken) where T : IFileData
        {
            Logger.Write(this, LogLevel.Debug, "Begin populating meta data.");

            if (this.ReportProgress)
            {
                await this.SetName("Populating meta data");
                await this.SetPosition(0);
                await this.SetCount(fileDatas.Count());
            }

            var interval = Math.Max(Convert.ToInt32(this.Count * 0.01), 1);
            var position = 0;
            var metaDataSource = this.MetaDataSourceFactory.Create();

            await AsyncParallel.ForEach(fileDatas, async fileData =>
            {
                Logger.Write(this, LogLevel.Debug, "Populating meta data for file: {0} => {1}", fileData.Id, fileData.FileName);

                var metaData = await metaDataSource.GetMetaData(fileData.FileName);

#if NET40
                this.Semaphore.Wait();
#else
                await this.Semaphore.WaitAsync();
#endif

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
#if NET40
                        this.Semaphore.Wait();
#else
                        await this.Semaphore.WaitAsync();
#endif
                        try
                        {
                            await this.SetDescription(new FileInfo(fileData.FileName).Name);
                            await this.SetPosition(position);
                        }
                        finally
                        {
                            this.Semaphore.Release();
                        }
                    }
                    Interlocked.Increment(ref position);
                }
            }, cancellationToken, this.Threads);
        }

        protected override void OnDisposing()
        {
            this.Semaphore.Dispose();
            this.Writer.Dispose();
            base.OnDisposing();
        }
    }
}
