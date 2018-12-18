using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System;
using System.IO;

namespace FoxTunes
{
    [Component("13A75018-8A24-413D-A731-C558C8FAF08F", ComponentSlots.Database)]
    public class SQLiteDatabaseFactory : DatabaseFactory
    {
        public IDatabaseComponent Database { get; private set; }

        protected override IDatabaseComponent OnCreate()
        {
            if (this.Database == null)
            {
                this.Database = new SQLiteDatabase();
            }
            return this.Database;
        }

        protected override void Configure(IDatabase database)
        {
            if (!File.Exists(SQLiteDatabase.FileName))
            {
                this.CreateDatabase(database);
            }
            base.Configure(database);
        }

        protected virtual void CreateDatabase(IDatabase database)
        {
            var exception = default(Exception);
            database.Provider.CreateDatabase(SQLiteDatabase.FileName);
            try
            {
                database.Execute(database.QueryFactory.Create(Resources.Database));
                return;
            }
            catch (Exception e)
            {
                exception = e;
                try
                {
                    database.Connection.Close();
                    database.Provider.DeleteDatabase(SQLiteDatabase.FileName);
                }
                catch
                {
                    //Nothing can be done.
                }
            }
            throw new InvalidOperationException("Failed to create the database.", exception);
        }
    }
}
