using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;

namespace FoxTunes
{
    public class LibraryHierarchyWriter : Disposable
    {
        public LibraryHierarchyWriter(IDatabaseComponent database, ITransactionSource transaction, IScriptingRuntime runtime)
        {
            this.Command = CreateCommand(database, transaction);
            this.Runtime = runtime;
            this.Context = runtime.CreateContext();
        }

        public IDatabaseCommand Command { get; private set; }

        public IScriptingRuntime Runtime { get; private set; }

        public IScriptingContext Context { get; private set; }

        public void Write(IDatabaseReaderRecord record)
        {
            this.Command.Parameters["libraryHierarchyId"] = record["LibraryHierarchy_Id"];
            this.Command.Parameters["libraryHierarchyLevelId"] = record["LibraryHierarchyLevel_Id"];
            this.Command.Parameters["libraryItemId"] = record["LibraryItem_Id"];
            this.Command.Parameters["displayValue"] = this.ExecuteScript(record, "DisplayScript");
            this.Command.Parameters["sortValue"] = this.ExecuteScript(record, "SortScript");
            this.Command.Parameters["isLeaf"] = record["IsLeaf"];
            this.Command.ExecuteNonQuery();
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
            this.Context.SetValue("fileName", fileName);
            this.Context.SetValue("tag", metaData);
            try
            {
                return this.Context.Run(script);
            }
            catch (ScriptingException e)
            {
                return e.Message;
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
            var table = database.Config.Table("LibraryHierarchy", TableFlags.None);
            table.Column("LibraryHierarchy_Id");
            table.Column("LibraryHierarchyLevel_Id");
            table.Column("LibraryItem_Id");
            table.Column("DisplayValue");
            table.Column("SortValue");
            table.Column("IsLeaf");
            var query = database.QueryFactory.Build();
            query.Add.SetTable(table);
            query.Add.AddColumns(table.Columns);
            query.Output.AddParameters(table.Columns);
            return database.CreateCommand(
                query.Build(),
                DatabaseCommandFlags.NoCache,
                transaction
            );
        }
    }
}
