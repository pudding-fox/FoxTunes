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
        public readonly object SyncRoot = new object();

        private MetaDataPopulator(bool reportProgress)
            : base(reportProgress)
        {
            this.Writers = new ThreadLocal<MetaDataWriter>(true);
        }

        public MetaDataPopulator(IDatabase database, IDatabaseQuery query, bool reportProgress, ITransactionSource transaction)
            : this(reportProgress)
        {
            this.Database = database;
            this.Transaction = transaction;
            this.Query = query;
        }

        public IDatabase Database { get; private set; }

        public ITransactionSource Transaction { get; private set; }

        public IDatabaseQuery Query { get; private set; }

        public IMetaDataSourceFactory MetaDataSourceFactory { get; private set; }

        private ThreadLocal<MetaDataWriter> Writers { get; set; }

        public override void InitializeComponent(ICore core)
        {
            this.MetaDataSourceFactory = core.Factories.MetaDataSource;
            base.InitializeComponent(core);
        }

        public void Populate<T>(IEnumerable<T> fileDatas) where T : IFileData
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

            Parallel.ForEach(fileDatas, this.ParallelOptions, fileData =>
            {
                Logger.Write(this, LogLevel.Debug, "Populating meta data for file: {0} => {1}", fileData.Id, fileData.FileName);

                var writer = this.GetOrAddWriter();
                var metaDataSource = this.MetaDataSourceFactory.Create(fileData.FileName);

                foreach (var metaDataItem in metaDataSource.MetaDatas)
                {
                    writer.Write(fileData.Id, metaDataItem);
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
            });
        }

        private MetaDataWriter GetOrAddWriter()
        {
            if (this.Writers.IsValueCreated)
            {
                return this.Writers.Value;
            }
            return this.Writers.Value = new MetaDataWriter(this.Database, this.Query, this.Transaction);
        }

        protected override void OnDisposing()
        {
            foreach (var writer in this.Writers.Values)
            {
                writer.Dispose();
            }
            this.Writers.Dispose();
            base.OnDisposing();
        }
    }
}
