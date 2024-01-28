using FoxDb;
using FoxDb.Interfaces;
using FoxDb.Utility;
using FoxTunes.Interfaces;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace FoxTunes
{
    public class SQLiteDatabase : Database
    {
        public static readonly string FileName = Path.Combine(
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
                return IsolationLevel.ReadCommitted;
            }
        }

        protected override IDatabaseQueries CreateQueries()
        {
            var queries = new SQLiteDatabaseQueries(this);
            queries.InitializeComponent(this.Core);
            return queries;
        }

        private static IProvider GetProvider()
        {
            var builder = new SQLiteConnectionStringBuilder();
            builder.DataSource = FileName;
            builder.JournalMode = SQLiteJournalModeEnum.Wal;
            return new SQLiteProvider(builder);
        }
    }
}
