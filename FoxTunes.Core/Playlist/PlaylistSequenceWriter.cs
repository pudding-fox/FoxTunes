using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class PlaylistSequenceWriter : Disposable
    {
        public PlaylistSequenceWriter(IDatabaseComponent database, IScriptingRuntime runtime, ITransactionSource transaction)
        {
            this.Command = CreateCommand(database, transaction);
            this.Runtime = runtime;
            this.Context = runtime.CreateContext();
        }

        public IDatabaseCommand Command { get; private set; }

        public IScriptingRuntime Runtime { get; private set; }

        public IScriptingContext Context { get; private set; }

        public Task Write(IDatabaseReaderRecord record)
        {
            this.Command.Parameters["playlistItemId"] = record["PlaylistItem_Id"];

            var values = this.ExecuteScript(record);
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
                this.Command.Parameters[string.Format("value{0}", a + 1)] = value;
            }

            return this.Command.ExecuteNonQueryAsync();
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
            this.Context.SetValue("fileName", fileName);
            this.Context.SetValue("tag", metaData);
            try
            {
                return this.Context.Run(this.Runtime.CoreScripts.PlaylistSortValues) as object[];
            }
            catch (ScriptingException e)
            {
                return new[] { e.Message };
            }
        }

        protected override void OnDisposing()
        {
            this.Context.Dispose();
            this.Command.Dispose();
            base.OnDisposing();
        }

        private static IDatabaseCommand CreateCommand(IDatabaseComponent database, ITransactionSource transaction)
        {
            return database.CreateCommand(
                database.Queries.AddPlaylistSequenceRecord,
                DatabaseCommandFlags.NoCache,
                transaction
            );
        }
    }
}
