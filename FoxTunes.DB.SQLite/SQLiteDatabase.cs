using FoxTunes.Interfaces;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace FoxTunes
{
    public class SQLiteDatabase : Database
    {
        private static readonly string DatabaseFileName = Path.Combine(
            Path.GetDirectoryName(typeof(SQLiteDatabase).Assembly.Location),
            "Database.dat"
        );

        public SQLiteDatabase()
        {
            this.Queries = new SQLiteDatabaseQueries(this);
            this.Connection = CreateConnection();
        }

        private string ConnectionString
        {
            get
            {
                var builder = new SQLiteConnectionStringBuilder();
                builder.DataSource = DatabaseFileName;
                return builder.ToString();
            }
        }

        private IDbConnection CreateConnection()
        {
            var create = false;
            if (!File.Exists(DatabaseFileName))
            {
                Logger.Write(this, LogLevel.Warn, "Failed to locate the database: {0}", DatabaseFileName);
                create = true;
            }
            Logger.Write(this, LogLevel.Debug, "Connecting to database: {0}", this.ConnectionString);
            var connection = new SQLiteConnection(this.ConnectionString);
            switch (connection.State)
            {
                case ConnectionState.Closed:
                    connection.Open();
                    break;
            }
            if (create)
            {
                this.CreateDatabase(connection);
            }
            return connection;
        }

        private void CreateDatabase(IDbConnection connection)
        {
            Logger.Write(this, LogLevel.Debug, "Creating database: {0}", this.ConnectionString);
            using (var command = connection.CreateCommand())
            {
                command.CommandText = Resources.Database;
                command.ExecuteNonQuery();
            }
        }

        public override IDatabaseQueries Queries { get; protected set; }

        public override IDbConnection Connection { get; protected set; }
    }
}
