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
    public class PlaylistSequencePopulator : PopulatorBase
    {
        public readonly object SyncRoot = new object();

        private PlaylistSequencePopulator(bool reportProgress)
            : base(reportProgress)
        {
            this.Command = new ThreadLocal<PlaylistSequencePopulatorCommand>(true);
        }

        public PlaylistSequencePopulator(IDatabase database, IDatabaseContext databaseContext, IDbTransaction transaction, bool reportProgress)
            : this(reportProgress)
        {
            this.Database = database;
            this.DatabaseContext = databaseContext;
            this.Transaction = transaction;
        }

        public IDatabase Database { get; private set; }

        public IDatabaseContext DatabaseContext { get; private set; }

        public IDbTransaction Transaction { get; private set; }

        public IScriptingRuntime ScriptingRuntime { get; private set; }

        private ThreadLocal<PlaylistSequencePopulatorCommand> Command { get; set; }

        public override void InitializeComponent(ICore core)
        {
            this.ScriptingRuntime = core.Components.ScriptingRuntime;
            base.InitializeComponent(core);
        }

        public void Populate(EnumerableDataReader reader)
        {
            if (this.ReportProgress)
            {
                this.Name = "Populating library hierarchies";
                this.Position = 0;
                this.Count = (
                    this.DatabaseContext.GetQuery<LibraryHierarchyLevel>().Detach().Count() * this.DatabaseContext.GetQuery<LibraryItem>().Detach().Count()
                );
            }

            var interval = Math.Max(Convert.ToInt32(this.Count * 0.01), 1);
            var position = 0;

            Parallel.ForEach(reader, this.ParallelOptions, record =>
            {
                var command = this.GetOrAddCommand();
                var values = this.ExecuteScript(command.ScriptingContext, record);

                command.Parameters["playlistItemId"] = record["PlaylistItem_Id"];

                for (var a = 0; a < 9; a++)
                {
                    var value = default(object);
                    if (a < values.Length)
                    {
                        value = values[a];
                    }
                    else
                    {
                        value = DBNull.Value;
                    }
                    command.Parameters[string.Format("value{0}", a + 1)] = value;
                }

                command.Command.ExecuteNonQuery();

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

        private object[] ExecuteScript(IScriptingContext scriptingContext, EnumerableDataReader.EnumerableDataReaderRow record)
        {
            var fileName = record["FileName"] as string;
            var metaData = new Dictionary<string, object>();
            for (var a = 0; true; a++)
            {
                var keyName = string.Format("Key_{0}", a);
                if (!record.ContainsKey(keyName))
                {
                    break;
                }
                var key = (record[keyName] as string).ToLower();
                var valueName = string.Format("Value_{0}_Value", a);
                var value = record[valueName] == DBNull.Value ? null : record[valueName];
                metaData.Add(key, value);
            }
            scriptingContext.SetValue("fileName", fileName);
            scriptingContext.SetValue("tag", metaData);
            try
            {
                return scriptingContext.Run(this.ScriptingRuntime.CoreScripts.PlaylistSortValues) as object[];
            }
            catch (ScriptingException e)
            {
                return new[] { e.Message };
            }
        }

        private PlaylistSequencePopulatorCommand GetOrAddCommand()
        {
            if (this.Command.IsValueCreated)
            {
                return this.Command.Value;
            }
            return this.Command.Value = new PlaylistSequencePopulatorCommand(this.ScriptingRuntime, this.Database, this.DatabaseContext, this.Transaction);
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

        private class PlaylistSequencePopulatorCommand : BaseComponent
        {
            public PlaylistSequencePopulatorCommand(IScriptingRuntime scriptingRuntime, IDatabase database, IDatabaseContext databaseContext, IDbTransaction transaction)
            {
                this.ScriptingContext = scriptingRuntime.CreateContext();
                var parameters = default(IDbParameterCollection);
                this.Command = databaseContext.Connection.CreateCommand(
                    database.CoreSQL.AddPlaylistSequenceRecord,
                    new[] { "playlistItemId", "value1", "value2", "value3", "value4", "value5", "value6", "value7", "value8", "value9", "value10" },
                    out parameters
                );
                this.Command.Transaction = transaction;
                this.Parameters = parameters;
            }

            public IScriptingContext ScriptingContext { get; private set; }

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
                this.ScriptingContext.Dispose();
                this.Command.Dispose();
            }

            ~PlaylistSequencePopulatorCommand()
            {
                Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
                this.Dispose(true);
            }
        }
    }
}
