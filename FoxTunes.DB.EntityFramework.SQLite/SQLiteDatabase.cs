using FoxTunes.Interfaces;
using System;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;

namespace FoxTunes
{
    [Component("F9F07827-E4F6-49FE-A26D-E415E0211E56", ComponentSlots.Database, priority: ComponentAttribute.PRIORITY_HIGH)]
    public class SQLiteDatabase : EntityFrameworkDatabase
    {
        const SynchronizationModes SYNC_MODE = SynchronizationModes.Normal;

        const SQLiteJournalModeEnum JOURNAL_MODE = SQLiteJournalModeEnum.Default;

        const int BUSY_TIMEOUT = 60;

        const int DEFAULT_TIMEOUT = 60;

        const IsolationLevel DEFAULT_ISOLATION_LEVEL = IsolationLevel.ReadCommitted;

        private static readonly Type[] References = new[]
        {
            typeof(global::System.Data.SQLite.EF6.SQLiteProviderFactory),
            typeof(global::System.Data.SQLite.Linq.SQLiteProviderFactory)
        };

        private static readonly string DatabaseFileName = Path.Combine(
            Path.GetDirectoryName(typeof(SQLiteDatabase).Assembly.Location),
            "Database.dat"
        );

        static SQLiteDatabase()
        {
            StageInteropAssemblies();
        }

        private static void StageInteropAssemblies()
        {
            var assemblies = new[]{
                new
                {
                    DirectoryName = "x86",
                    FileName = "SQLite.Interop.dll",
                    Content = Resources.SQLite_Interop_x86
                },
                new
                {
                    DirectoryName = "x64",
                    FileName = "SQLite.Interop.dll",
                    Content = Resources.SQLite_Interop_x64
                }
            };
            var location = typeof(SQLiteDatabase).Assembly.Location;
            var baseDirectoryName = Path.GetDirectoryName(location);
            foreach (var assembly in assemblies)
            {
                var directoryName = Path.Combine(baseDirectoryName, assembly.DirectoryName);
                if (!Directory.Exists(directoryName))
                {
                    Logger.Write(typeof(SQLiteDatabase), LogLevel.Debug, "Creating interop directory: {0}", directoryName);
                    Directory.CreateDirectory(directoryName);
                }
                var fileName = Path.Combine(directoryName, assembly.FileName);
                if (!File.Exists(fileName))
                {
                    Logger.Write(typeof(SQLiteDatabase), LogLevel.Debug, "Writing interop assembly: {0}", fileName);
                    File.WriteAllBytes(fileName, assembly.Content);
                }
            }
        }

        protected virtual string ConnectionString
        {
            get
            {
                var builder = new SQLiteConnectionStringBuilder();
                builder.DataSource = DatabaseFileName;
                builder.SyncMode = SYNC_MODE;
                builder.JournalMode = JOURNAL_MODE;
                builder.BusyTimeout = BUSY_TIMEOUT;
                builder.DefaultTimeout = DEFAULT_TIMEOUT;
                builder.DefaultIsolationLevel = DEFAULT_ISOLATION_LEVEL;
                return builder.ToString();
            }
        }

        public override ICoreSQL CoreSQL
        {
            get
            {
                return SQLiteCoreSQL.Instance;
            }
        }

        private static DbConnection Connection { get; set; }

        public override DbConnection CreateConnection()
        {
            if (Connection == null)
            {
                var create = false;
                if (!File.Exists(DatabaseFileName))
                {
                    Logger.Write(this, LogLevel.Warn, "Failed to locate the database: {0}", DatabaseFileName);
                    create = true;
                }
                Logger.Write(this, LogLevel.Debug, "Connecting to database: {0}", this.ConnectionString);
                Connection = new SQLiteConnection(this.ConnectionString);
                Connection.Disposed += this.OnConnectionDisposed;
                if (create)
                {
                    this.CreateDatabase();
                }
            }
            return Connection;
        }

        private void OnConnectionDisposed(object sender, EventArgs e)
        {
            Connection = null;
        }

        private void CreateDatabase()
        {
            Logger.Write(this, LogLevel.Debug, "Creating database: {0}", this.ConnectionString);
            switch (Connection.State)
            {
                case ConnectionState.Closed:
                    Connection.Open();
                    break;
            }
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = Resources.Database;
                command.ExecuteNonQuery();
            }
        }
    }
}
