using FoxDb;
using FoxDb.Interfaces;
using FoxDb.Utility;
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

        public override ITransactionSource BeginTransaction()
        {
            //Transactions are disabled, they cause more problems than they solve here.
            //We need to move to a reader/writer lock on the database.
            //All long running tasks will need to be cancellable.
            return new NullTransactionSource(this);
        }

        public override ITransactionSource BeginTransaction(IsolationLevel isolationLevel)
        {
            //Transactions are disabled, they cause more problems than they solve here.
            //We need to move to a reader/writer lock on the database.
            //All long running tasks will need to be cancellable.
            return new NullTransactionSource(this);
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
