using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading;

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

        protected override void Configure()
        {
            try
            {
                switch (this.Connection.State)
                {
                    case ConnectionState.Open:
                        break;
                }
            }
            catch (SqlException)
            {
                this.CreateDatabase();
            }
            base.Configure();
        }

        protected override IDatabaseQueries CreateQueries()
        {
            return new SqlServerDatabaseQueries(this);
        }

        protected virtual void CreateDatabase()
        {
            var exception = default(Exception);
            var builder = new SqlConnectionStringBuilder(ConnectionString);
            this.Provider.CreateDatabase(builder.InitialCatalog);
            //Sometimes the database isn't available right away.
            for (var a = 0; a < 5; a++)
            {
                try
                {
                    this.Execute(this.QueryFactory.Create(Resources.Database));
                    return;
                }
                catch (Exception e)
                {
                    exception = e;
                    Thread.Sleep(500);
                }
            }
            try
            {
                this.Connection.Close();
                this.Provider.DeleteDatabase(builder.InitialCatalog);
            }
            catch
            {
                //Nothing can be done.
            }
            throw new InvalidOperationException("Failed to create the database.", exception);
        }

        private static IProvider GetProvider()
        {
            var builder = new SqlConnectionStringBuilder(ConnectionString);
            return new SqlServer2012Provider(builder);
        }
    }
}
