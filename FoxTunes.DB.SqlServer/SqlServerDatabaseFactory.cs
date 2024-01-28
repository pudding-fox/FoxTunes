using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;

namespace FoxTunes
{
    [Component("ECF542D9-ABB3-4E82-8045-7E13F1727695", ComponentSlots.Database, priority: ComponentAttribute.PRIORITY_HIGH)]
    public class SqlServerDatabaseFactory : DatabaseFactory
    {
        protected override IDatabaseComponent OnCreate()
        {
            return new SqlServerDatabase();
        }

        protected override void Configure(IDatabase database)
        {
            try
            {
                switch (database.Connection.State)
                {
                    case ConnectionState.Open:
                        break;
                }
            }
            catch (SqlException)
            {
                this.CreateDatabase(database);
                this.Core.CreateDefaultData(database);
            }
            base.Configure(database);
        }


        protected virtual void CreateDatabase(IDatabase database)
        {
            var exception = default(Exception);
            var builder = new SqlConnectionStringBuilder(SqlServerDatabase.ConnectionString);
            database.Provider.CreateDatabase(builder.InitialCatalog);
            //Sometimes the database isn't available right away.
            for (var a = 0; a < 5; a++)
            {
                try
                {
                    database.Execute(database.QueryFactory.Create(Resources.Database));
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
                database.Connection.Close();
                database.Provider.DeleteDatabase(builder.InitialCatalog);
            }
            catch
            {
                //Nothing can be done.
            }
            throw new InvalidOperationException("Failed to create the database.", exception);
        }
    }
}
