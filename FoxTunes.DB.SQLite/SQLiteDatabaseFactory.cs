using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace FoxTunes
{
    [Component("13A75018-8A24-413D-A731-C558C8FAF08F", ComponentSlots.Database, priority: ComponentAttribute.PRIORITY_HIGH)]
    public class SQLiteDatabaseFactory : DatabaseFactory
    {
        protected override bool OnTest(IDatabase database)
        {
            if (!File.Exists(SQLiteDatabase.FileName))
            {
                return false;
            }
            try
            {
                switch (database.Connection.State)
                {
                    case ConnectionState.Open:
                        return true;
                }
            }
            catch (SQLiteException)
            {
                //Nothing can be done.
            }
            return false;
        }

        protected override void OnInitialize(IDatabase database)
        {
            this.CreateDatabase(database);
        }

        protected override IDatabaseComponent OnCreate()
        {
            return new SQLiteDatabase();
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
            if (File.Exists(SQLiteDatabase.FileName))
            {
                try
                {
                    File.Delete(SQLiteDatabase.FileName);
                }
                catch
                {
                    //Nothing can be done.
                }
            }
            throw new InvalidOperationException(string.Format("Failed to create the database: {0}", exception.Message), exception);
        }
    }
}
