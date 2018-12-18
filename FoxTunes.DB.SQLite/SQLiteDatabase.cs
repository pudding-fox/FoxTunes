using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System.Data;
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
                return IsolationLevel.Unspecified;
            }
        }

        protected override IDatabaseQueries CreateQueries()
        {
            return new SQLiteDatabaseQueries(this);
        }

        private static IProvider GetProvider()
        {
            return new SQLiteProvider(FileName);
        }

        protected override void Dispose(bool disposing)
        {
            //Nothing to do.
        }
    }
}
