using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace FoxTunes
{
    [Component("13A75018-8A24-413D-A731-C558C8FAF08F", ComponentSlots.Database)]
    public class SQLiteDatabase : Database
    {
        private static readonly string DatabaseFileName = Path.Combine(
            Path.GetDirectoryName(typeof(SQLiteDatabase).Assembly.Location),
            "Database.db"
        );

        public SQLiteDatabase()
            : base(GetProvider())
        {

        }

        public override IsolationLevel PreferredIsolationLevel
        {
            get
            {
                return IsolationLevel.Unspecified;
            }
        }

        protected override void Configure()
        {
            if (!File.Exists(DatabaseFileName))
            {
                this.CreateDatabase();
            }
            base.Configure();
        }

        protected override IDatabaseQueries CreateQueries()
        {
            return new SQLiteDatabaseQueries(this);
        }

        protected virtual void CreateDatabase()
        {
            this.Execute(this.QueryFactory.Create(Resources.Database));
        }

        private static IProvider GetProvider()
        {
            var builder = new SQLiteConnectionStringBuilder();
            builder.DataSource = DatabaseFileName;
            builder.JournalMode = SQLiteJournalModeEnum.Wal;
            //builder.SyncMode = SynchronizationModes.Off;
            return new SQLiteProvider(DatabaseFileName);
        }
    }
}
