using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class LibraryHierarchyPopulator : PopulatorBase
    {
        public readonly object SyncRoot = new object();

        private LibraryHierarchyPopulator(bool reportProgress)
            : base(reportProgress)
        {
            this.Writers = new ThreadLocal<LibraryHierarchyWriter>(true);
        }

        public LibraryHierarchyPopulator(IDatabaseComponent database, bool reportProgress, ITransactionSource transaction)
            : this(reportProgress)
        {
            this.Database = database;
            this.Transaction = transaction;
        }

        public IDatabaseComponent Database { get; private set; }

        public ITransactionSource Transaction { get; private set; }

        public IScriptingRuntime ScriptingRuntime { get; private set; }

        private ThreadLocal<LibraryHierarchyWriter> Writers { get; set; }

        public override void InitializeComponent(ICore core)
        {
            this.ScriptingRuntime = core.Components.ScriptingRuntime;
            base.InitializeComponent(core);
        }

        public void Populate(IDatabaseReader reader)
        {
            if (this.ReportProgress)
            {
                this.Name = "Populating library hierarchies";
                this.Position = 0;
                this.Count = (
                    this.Database.Set<LibraryHierarchyLevel>().Count * this.Database.Set<LibraryItem>().Count
                );
            }

            var interval = Math.Max(Convert.ToInt32(this.Count * 0.01), 1);
            var position = 0;

            Parallel.ForEach(reader, this.ParallelOptions, record =>
            {
                var writer = this.GetOrAddWriter();
                writer.Write(record);

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
            });
        }

        private LibraryHierarchyWriter GetOrAddWriter()
        {
            if (this.Writers.IsValueCreated)
            {
                return this.Writers.Value;
            }
            return this.Writers.Value = new LibraryHierarchyWriter(this.Database, this.ScriptingRuntime, this.Transaction);
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
