//#define PARALLEL_WRITER
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class PlaylistSequencePopulator : PopulatorBase
    {
        public readonly object SyncRoot = new object();

        public PlaylistSequencePopulator(IDatabaseComponent database, ITransactionSource transaction)
            : base(false)
        {
            this.Database = database;
            this.Transaction = transaction;
            this.Contexts = new ThreadLocal<IScriptingContext>(true);
#if PARALLEL_WRITER
            this.Writers = new global::System.Threading.ThreadLocal<PlaylistSequenceWriter>(true);
#else
            this.Semaphore = new SemaphoreSlim(1, 1);
            this.Writer = new PlaylistSequenceWriter(this.Database, this.Transaction);
#endif
        }

        public IDatabaseComponent Database { get; private set; }

        public ITransactionSource Transaction { get; private set; }

        public IScriptingRuntime ScriptingRuntime { get; private set; }

        private ThreadLocal<IScriptingContext> Contexts { get; set; }

#if PARALLEL_WRITER
        private global::System.Threading.ThreadLocal<PlaylistSequenceWriter> Writers { get; set; }
#else
        private SemaphoreSlim Semaphore { get; set; }

        private PlaylistSequenceWriter Writer { get; set; }
#endif

        public override void InitializeComponent(ICore core)
        {
            this.ScriptingRuntime = core.Components.ScriptingRuntime;
            base.InitializeComponent(core);
        }

        public Task Populate(IDatabaseReader reader, CancellationToken cancellationToken)
        {
            return AsyncParallel.ForEach(reader, async record =>
            {
                var context = this.GetOrAddContext();
                var values = this.ExecuteScript(record);
#if PARALLEL_WRITER
                var writer = this.GetOrAddWriter();
                await writer.Write(record, values);
#else
                await this.Semaphore.WaitAsync();
                try
                {
                    await this.Writer.Write(record, values);
                }
                finally
                {
                    this.Semaphore.Release();
                }
#endif
            }, cancellationToken, this.ParallelOptions);
        }

        private object[] ExecuteScript(IDatabaseReaderRecord record)
        {
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
                return context.Run(this.ScriptingRuntime.CoreScripts.PlaylistSortValues) as object[];
            }
            catch (ScriptingException e)
            {
                return new[] { e.Message };
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
        private PlaylistSequenceWriter GetOrAddWriter()
        {
            if (this.Writers.IsValueCreated)
            {
                return this.Writers.Value;
            }
            return this.Writers.Value = new PlaylistSequenceWriter(this.Database, this.Transaction);
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
