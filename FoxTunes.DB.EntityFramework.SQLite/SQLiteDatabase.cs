using System;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;

namespace FoxTunes
{
    public class SQLiteDatabase : EntityFrameworkDatabase
    {
        private static readonly string DatabaseFileName = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Database.dat"
        );

        protected virtual string ConnectionString
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override DbConnection CreateConnection()
        {
            if (!File.Exists(DatabaseFileName))
            {
                SQLiteConnection.CreateFile(DatabaseFileName);
            }
            return new SQLiteConnection(this.ConnectionString);
        }

        public override void Save<T>(T value)
        {
            throw new NotImplementedException();
        }

        public override void Load<T>(T value)
        {
            throw new NotImplementedException();
        }
    }
}
