using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class LibraryHierarchyPopulator : PopulatorBase
    {
        public readonly object SyncRoot = new object();

        public LibraryHierarchyPopulator(IDatabaseComponent database, bool reportProgress, ITransactionSource transaction)
            : base(reportProgress)
        {
            this.Database = database;
            this.Transaction = transaction;
#if NET40
            this.Contexts = new TrackingThreadLocal<IScriptingContext>();
#else
            this.Contexts = new ThreadLocal<IScriptingContext>(true);
#endif
            this.Writer = new LibraryHierarchyWriter(this.Database, this.Transaction);
        }

        public IDatabaseComponent Database { get; private set; }

        public ITransactionSource Transaction { get; private set; }

        public IScriptingRuntime ScriptingRuntime { get; private set; }

#if NET40
        private TrackingThreadLocal<IScriptingContext> Contexts { get; set; }
#else
        private ThreadLocal<IScriptingContext> Contexts { get; set; }
#endif

        private LibraryHierarchyWriter Writer { get; set; }

        public override void InitializeComponent(ICore core)
        {
            this.ScriptingRuntime = core.Components.ScriptingRuntime;
            base.InitializeComponent(core);
        }

        public async Task Populate(IDatabaseReader reader, CancellationToken cancellationToken, ITransactionSource transaction = null)
        {
            if (this.ReportProgress)
            {
                await this.SetName("Populating library hierarchies");
                await this.SetPosition(0);
                await this.SetCount(
                    this.Database.Set<LibraryHierarchyLevel>(transaction).Count * this.Database.Set<LibraryItem>(transaction).Count
                );
            }

            var interval = Math.Max(Convert.ToInt32(this.Count * 0.01), 1);
            var position = 0;

            await AsyncParallel.ForEach(reader, async record =>
            {
                var context = this.GetOrAddContext();
                var displayValue = this.ExecuteScript(record, "DisplayScript");
                var sortValue = this.ExecuteScript(record, "SortScript");

                await this.Semaphore.WaitAsync();
                try
                {
                    await this.Writer.Write(record, displayValue, sortValue);
                }
                finally
                {
                    this.Semaphore.Release();
                }

                if (this.ReportProgress)
                {
                    if (position % interval == 0)
                    {
                        await this.Semaphore.WaitAsync();
                        try
                        {
                            await this.SetDescription(new FileInfo(record["FileName"] as string).Name);
                            await this.SetPosition(position);
                        }
                        finally
                        {
                            this.Semaphore.Release();
                        }
                    }
                    Interlocked.Increment(ref position);
                }
            }, cancellationToken, this.ParallelOptions);
        }

        private object ExecuteScript(IDatabaseReaderRecord record, string name)
        {
            var script = record[name] as string;
            if (string.IsNullOrEmpty(script))
            {
                return string.Empty;
            }
            var fileName = record["FileName"] as string;
            var metaData = new Dictionary<string, object>();
            for (var a = 0; true; a++)
            {
                var keyName = string.Format("Key_{0}", a);
                if (!record.Contains(keyName))
                {
                    break;
                }
                var key = (record[keyName] as string).ToLower();
                var valueName = string.Format("Value_{0}_Value", a);
                var value = record[valueName] == DBNull.Value ? null : record[valueName];
                metaData.Add(key, value);
            }
            var context = this.GetOrAddContext();
            context.SetValue("fileName", fileName);
            context.SetValue("tag", metaData);
            try
            {
                return context.Run(script);
            }
            catch (ScriptingException e)
            {
                return e.Message;
            }
        }

        private IScriptingContext GetOrAddContext()
        {
            if (this.Contexts.IsValueCreated)
            {
                return this.Contexts.Value;
            }
            return this.Contexts.Value = this.ScriptingRuntime.CreateContext();
        }

        protected override void OnDisposing()
        {
            foreach (var context in this.Contexts.Values)
            {
                context.Dispose();
            }
            this.Contexts.Dispose();
            this.Writer.Dispose();
            base.OnDisposing();
        }
    }
}
