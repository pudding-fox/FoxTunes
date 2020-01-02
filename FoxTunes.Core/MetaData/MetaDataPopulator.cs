using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace FoxTunes
{
    public abstract class MetaDataPopulator : PopulatorBase
    {
        public const string ID = "EA40EA65-6F49-48A6-9469-DD5FC2E36EC0";

        protected MetaDataPopulator(IDatabaseComponent database, IDatabaseQuery query, bool reportProgress, ITransactionSource transaction)
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

        public IFileData Current { get; private set; }

        private volatile int position = 0;

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
                await this.SetName("Populating meta data").ConfigureAwait(false);
                await this.SetPosition(0).ConfigureAwait(false);
                await this.SetCount(fileDatas.Count()).ConfigureAwait(false);
                if (this.Count <= 100)
                {
                    this.Timer.Interval = FAST_INTERVAL;
                }
                else if (this.Count < 1000)
                {
                    this.Timer.Interval = NORMAL_INTERVAL;
                }
                else
                {
                    this.Timer.Interval = LONG_INTERVAL;
                }
                this.Timer.Start();
            }

            var metaDataSource = this.MetaDataSourceFactory.Create();

            await AsyncParallel.ForEach(fileDatas, async fileData =>
            {
                Logger.Write(this, LogLevel.Debug, "Populating meta data for file: {0} => {1}", fileData.Id, fileData.FileName);

                var metaData = await metaDataSource.GetMetaData(fileData.FileName).ConfigureAwait(false);

#if NET40
                this.Semaphore.Wait();
#else
                await this.Semaphore.WaitAsync().ConfigureAwait(false);
#endif

                try
                {
                    foreach (var metaDataItem in metaData)
                    {
                        await this.Writer.Write(fileData.Id, metaDataItem).ConfigureAwait(false);
                    }
                }
                finally
                {
                    this.Semaphore.Release();
                }

                if (this.ReportProgress)
                {
                    this.Current = fileData;
                    Interlocked.Increment(ref this.position);
                }
            }, cancellationToken, this.ParallelOptions).ConfigureAwait(false);
        }

        protected override async void OnElapsed(object sender, ElapsedEventArgs e)
        {
            var count = this.position - this.Position;
            if (count != 0)
            {
                switch (this.Timer.Interval)
                {
                    case NORMAL_INTERVAL:
                        count *= 2;
                        break;
                    case FAST_INTERVAL:
                        count *= 10;
                        break;
                }
                var eta = this.GetEta(count);
                await this.SetName(string.Format("Populating meta data: {0} remaining @ {1} items/s", eta, count)).ConfigureAwait(false);
                if (this.Current != null)
                {
                    await this.SetDescription(new FileInfo(this.Current.FileName).Name).ConfigureAwait(false);
                }
                await this.SetPosition(this.position).ConfigureAwait(false);
            }
            base.OnElapsed(sender, e);
        }

        protected override void OnDisposing()
        {
            this.Semaphore.Dispose();
            this.Writer.Dispose();
            base.OnDisposing();
        }
    }
}
