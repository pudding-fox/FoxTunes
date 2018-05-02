using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class PlaylistSequencePopulator : PopulatorBase
    {
        public readonly object SyncRoot = new object();

        private PlaylistSequencePopulator(bool reportProgress)
            : base(reportProgress)
        {
            this.Writers = new ThreadLocal<PlaylistSequenceWriter>(true);
        }

        public PlaylistSequencePopulator(IDatabaseComponent database, ITransactionSource transaction)
            : this(false)
        {
            this.Database = database;
            this.Transaction = transaction;
        }

        public IDatabaseComponent Database { get; private set; }

        public ITransactionSource Transaction { get; private set; }

        public IScriptingRuntime ScriptingRuntime { get; private set; }

        private ThreadLocal<PlaylistSequenceWriter> Writers { get; set; }

        public override void InitializeComponent(ICore core)
        {
            this.ScriptingRuntime = core.Components.ScriptingRuntime;
            base.InitializeComponent(core);
        }

        public void Populate(IDatabaseReader reader)
        {
            Parallel.ForEach(reader, this.ParallelOptions, record =>
            {
                var writer = this.GetOrAddWriter();
                writer.Write(record);
            });
        }

        private PlaylistSequenceWriter GetOrAddWriter()
        {
            if (this.Writers.IsValueCreated)
            {
                return this.Writers.Value;
            }
            return this.Writers.Value = new PlaylistSequenceWriter(this.Database, this.Transaction, this.ScriptingRuntime);
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
