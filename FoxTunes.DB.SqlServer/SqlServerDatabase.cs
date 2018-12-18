using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace FoxTunes
{
    public class SqlServerDatabase : Database
    {
        public SqlServerDatabase()
            : base(GetProvider())
        {

        }

        public static string ConnectionString
        {
            get
            {
                return ConfigurationManager.AppSettings.Get("ConnectionString");
            }
        }

        public override IsolationLevel PreferredIsolationLevel
        {
            get
            {
                return IsolationLevel.ReadUncommitted;
            }
        }

        protected override IDatabaseQueries CreateQueries()
        {
            return new SqlServerDatabaseQueries(this);
        }

        private static IProvider GetProvider()
        {
            var builder = new SqlConnectionStringBuilder(ConnectionString);
            return new SqlServer2012Provider(builder);
        }
    }
}
