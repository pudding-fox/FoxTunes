using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System;
using System.ComponentModel;
using System.Data.SQLite;
using System.IO;

namespace FoxTunes
{
    [Component("13A75018-8A24-413D-A731-C558C8FAF08F", ComponentSlots.Database)]
    public class SQLiteDatabase : Database, IDatabaseComponent
    {
        protected static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        private static readonly string DatabaseFileName = Path.Combine(
            Path.GetDirectoryName(typeof(SQLiteDatabase).Assembly.Location),
            "Database.db"
        );

        public SQLiteDatabase() : base(GetProvider())
        {
            if (!File.Exists(DatabaseFileName))
            {
                this.CreateDatabase();
            }
        }

        public IDatabaseSets Sets { get; private set; }

        public IDatabaseQueries Queries { get; private set; }

        public virtual void InitializeComponent(ICore core)
        {
            this.Sets = new DatabaseSets();
            this.Sets.InitializeComponent(core);
            this.Queries = new SQLiteDatabaseQueries(this);
        }

        protected virtual void CreateDatabase()
        {
            this.Execute(new DatabaseQuery(Resources.Database));
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged == null)
            {
                return;
            }
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected virtual void OnError(string message)
        {
            this.OnError(new Exception(message));
        }

        protected virtual void OnError(Exception exception)
        {
            this.OnError(exception.Message, exception);
        }

        protected virtual void OnError(string message, Exception exception)
        {
            Logger.Write(this, LogLevel.Error, message, exception);
            if (this.Error == null)
            {
                return;
            }
            this.Error(this, new ComponentOutputErrorEventArgs(message, exception));
        }

        [field: NonSerialized]
        public event ComponentOutputErrorEventHandler Error = delegate { };

        private static IProvider GetProvider()
        {
            var builder = new SQLiteConnectionStringBuilder();
            builder.DataSource = DatabaseFileName;
            builder.JournalMode = SQLiteJournalModeEnum.Wal;
            return new SQLiteProvider(DatabaseFileName);
        }
    }
}
