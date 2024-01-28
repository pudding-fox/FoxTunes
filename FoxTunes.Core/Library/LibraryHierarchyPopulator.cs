//#define PARALLEL_WRITER
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
            this.Contexts = new ThreadLocal<IScriptingContext>(true);
#if PARALLEL_WRITER
            this.Writers = new ThreadLocal<LibraryHierarchyWriter>(true);
#else
            this.Semaphore = new SemaphoreSlim(1, 1);
            this.Writer = new LibraryHierarchyWriter(this.Database, this.Transaction);
#endif
        }

        public IDatabaseComponent Database { get; private set; }

        public ITransactionSource Transaction { get; private set; }

        public IScriptingRuntime ScriptingRuntime { get; private set; }

        private ThreadLocal<IScriptingContext> Contexts { get; set; }

#if PARALLEL_WRITER
        private ThreadLocal<LibraryHierarchyWriter> Writers { get; set; }
#else
        private SemaphoreSlim Semaphore { get; set; }

        private LibraryHierarchyWriter Writer { get; set; }
#endif

        public override void InitializeComponent(ICore core)
        {
            this.ScriptingRuntime = core.Components.ScriptingRuntime;
            base.InitializeComponent(core);
        }

        public Task Populate(IDatabaseReader reader, CancellationToken cancellationToken, ITransactionSource transaction = null)
        {
            if (this.ReportProgress)
            {
                this.Name = "Populating library hierarchies";
                this.Position = 0;
                this.Count = (
                    this.Database.Set<LibraryHierarchyLevel>(transaction).Count * this.Database.Set<LibraryItem>(transaction).Count
                );
            }

            var interval = Math.Max(Convert.ToInt32(this.Count * 0.01), 1);
            var position = 0;

            return AsyncParallel.ForEach(reader, async record =>
            {
                var context = this.GetOrAddContext();
                var displayValue = this.ExecuteScript(record, "DisplayScript");
                var sortValue = this.ExecuteScript(record, "SortScript");
#if PARALLEL_WRITER
                var writer = this.GetOrAddWriter();
                await writer.Write(record, displayValue, sortValue);
#else
                await this.Semaphore.WaitAsync();
                try
                {
                    await this.Writer.Write(record, displayValue, sortValue);
                }
                finally
                {
                    this.Semaphore.Release();
                }
#endif
                if (this.ReportProgress)
                {
                    if (position % interval == 0)
                    {
                        lock (this.SyncRoot)
                        {
                            var fileName = record["FileName"] as string;
                            this.Description = new FileInfo(fileName).Name;
                            this.Position = position;
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

#if PARALLEL_WRITER
        private LibraryHierarchyWriter GetOrAddWriter()
        {
            if (this.Writers.IsValueCreated)
            {
                return this.Writers.Value;
            }
            return this.Writers.Value = new LibraryHierarchyWriter(this.Database, this.Transaction);
        }
#endif

        protected override void OnDisposing()
        {
            foreach (var context in this.Contexts.Values)
            {
                context.Dispose();
            }
            this.Contexts.Dispose();
#if PARALLEL_WRITER
            foreach (var writer in this.Writers.Values)
            {
                writer.Dispose();
            }
            this.Writers.Dispose();
#else
            this.Writer.Dispose();
#endif
            base.OnDisposing();
        }
    }
}
