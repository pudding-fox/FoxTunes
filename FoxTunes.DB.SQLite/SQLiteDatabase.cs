#pragma warning disable 612, 618
using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System;
using System.ComponentModel;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;

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
            this.Configure();
        }

        public IDatabaseSets Sets { get; private set; }

        public IDatabaseTables Tables { get; private set; }

        public IDatabaseQueries Queries { get; private set; }

        public virtual void InitializeComponent(ICore core)
        {
            this.Sets = new DatabaseSets();
            this.Sets.InitializeComponent(core);
            this.Tables = new SQLiteDatabaseTables(this);
            this.Queries = new SQLiteDatabaseQueries(this);
        }

        protected virtual void CreateDatabase()
        {
            this.Execute(this.QueryFactory.Create(Resources.Database));
        }

        protected virtual void Configure()
        {
            this.Config.Table<PlaylistItem>().With(table =>
            {
                table.Relation(item => item.MetaDatas).With(relation =>
                {
                    relation.Expression.Left = relation.Expression.Clone();
                    relation.Expression.Operator = relation.Expression.CreateOperator(QueryOperator.OrElse);
                    relation.Expression.Right = relation.CreateConstraint().With(constraint =>
                    {
                        constraint.Left = relation.CreateConstraint(
                            this.Config.Table<PlaylistItem>().Column("LibraryItem_Id"),
                            this.Config.Table<LibraryItem, MetaDataItem>().Column("LibraryItem_Id")
                        );
                        constraint.Operator = constraint.CreateOperator(QueryOperator.AndAlso);
                        constraint.Right = relation.CreateConstraint(
                            this.Config.Table<LibraryItem, MetaDataItem>().Column("MetaDataItem_Id"),
                            this.Config.Table<MetaDataItem>().Column("Id")
                        );
                    });
                });
            });
        }

        protected virtual void OnPropertyChanging(string propertyName)
        {
            if (this.PropertyChanging == null)
            {
                return;
            }
            this.PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
        }

        [field: NonSerialized]
        public event PropertyChangingEventHandler PropertyChanging = delegate { };

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

        protected virtual Task OnError(string message, Exception exception)
        {
            Logger.Write(this, LogLevel.Error, message, exception);
            if (this.Error == null)
            {
                return Task.CompletedTask;
            }
            return this.Error(this, new ComponentErrorEventArgs(message, exception));
        }

        [field: NonSerialized]
        public event ComponentErrorEventHandler Error;

        private static IProvider GetProvider()
        {
            var builder = new SQLiteConnectionStringBuilder();
            builder.DataSource = DatabaseFileName;
            builder.JournalMode = SQLiteJournalModeEnum.Wal;
            //builder.SyncMode = SynchronizationModes.Off;
            return new SQLiteProvider(DatabaseFileName);
        }
    }
}
