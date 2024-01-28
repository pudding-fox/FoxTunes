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
#if NET40
            this.Contexts = new TrackingThreadLocal<IScriptingContext>();
#else
            this.Contexts = new ThreadLocal<IScriptingContext>(true);
#endif
            this.Writer = new PlaylistSequenceWriter(this.Database, this.Transaction);
        }

        public IDatabaseComponent Database { get; private set; }

        public ITransactionSource Transaction { get; private set; }

        public IScriptingRuntime ScriptingRuntime { get; private set; }

#if NET40
        private TrackingThreadLocal<IScriptingContext> Contexts { get; set; }
#else
        private ThreadLocal<IScriptingContext> Contexts { get; set; }
#endif

        private PlaylistSequenceWriter Writer { get; set; }

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
                await this.Semaphore.WaitAsync();
                try
                {
                    await this.Writer.Write(record, values);
                }
                finally
                {
                    this.Semaphore.Release();
                }
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
