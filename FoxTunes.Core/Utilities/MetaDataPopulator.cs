using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
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
            this.Command = new ThreadLocal<MetaDataPopulatorCommand>(true);
        }

        public MetaDataPopulator(IDatabase database, IDbTransaction transaction, IDatabaseQuery query, bool reportProgress)
            : this(reportProgress)
        {
            this.Database = database;
            this.Transaction = transaction;
            this.Query = query;
        }

        public IDatabase Database { get; private set; }

        public IDbTransaction Transaction { get; private set; }

        public IDatabaseQuery Query { get; private set; }

        public IMetaDataSourceFactory MetaDataSourceFactory { get; private set; }

        private ThreadLocal<MetaDataPopulatorCommand> Command { get; set; }

        public override void InitializeComponent(ICore core)
        {
            this.MetaDataSourceFactory = core.Factories.MetaDataSource;
            base.InitializeComponent(core);
        }

        public void Populate(IEnumerable<IFileData> fileDatas)
        {
            Logger.Write(this, LogLevel.Debug, "Begin populating meta data.");

            if (this.ReportProgress)
            {
                this.Name = "Populating meta data";
                this.Position = 0;
                if (fileDatas is ICountable)
                {
                    this.Count = (fileDatas as ICountable).Count;
                }
                else
                {
                    this.Count = fileDatas.Count();
                }
            }

            var interval = Math.Max(Convert.ToInt32(this.Count * 0.01), 1);
            var position = 0;

            Parallel.ForEach(fileDatas, this.ParallelOptions, fileData =>
            {
                Logger.Write(this, LogLevel.Debug, "Populating meta data for file: {0} => {1}", fileData.Id, fileData.FileName);

                var command = this.GetOrAddCommands();
                var metaDataSource = this.MetaDataSourceFactory.Create(fileData.FileName);

                command.Parameters["itemId"] = fileData.Id;

                foreach (var metaDataItem in metaDataSource.MetaDatas)
                {
                    command.Parameters["name"] = metaDataItem.Name;
                    command.Parameters["type"] = metaDataItem.Type;
                    command.Parameters["numericValue"] = metaDataItem.NumericValue;
                    command.Parameters["textValue"] = metaDataItem.TextValue;
                    command.Parameters["fileValue"] = metaDataItem.FileValue;
                    command.Command.ExecuteNonQuery();
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

        private MetaDataPopulatorCommand GetOrAddCommands()
        {
            if (this.Command.IsValueCreated)
            {
                return this.Command.Value;
            }
            return this.Command.Value = new MetaDataPopulatorCommand(this.Database, this.Transaction, this.Query);
        }

        protected override void OnDisposing()
        {
            foreach (var command in this.Command.Values)
            {
                command.Dispose();
            }
            this.Command.Dispose();
            base.OnDisposing();
        }

        private class MetaDataPopulatorCommand : BaseComponent
        {
            public MetaDataPopulatorCommand(IDatabase database, IDbTransaction transaction, IDatabaseQuery query)
            {
                var parameters = default(IDbParameterCollection);
                this.Command = database.CreateCommand(query, out parameters, transaction);
                this.Parameters = parameters;
            }

            public IDbCommand Command { get; private set; }

            public IDbParameterCollection Parameters { get; private set; }

            public bool IsDisposed { get; private set; }

            public void Dispose()
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (this.IsDisposed || !disposing)
                {
                    return;
                }
                this.OnDisposing();
                this.IsDisposed = true;
            }

            protected virtual void OnDisposing()
            {
                this.Command.Dispose();
            }

            ~MetaDataPopulatorCommand()
            {
                Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
                this.Dispose(true);
            }
        }
    }
}
