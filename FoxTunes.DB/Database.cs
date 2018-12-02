#pragma warning disable 612, 618
using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace FoxTunes
{
    public abstract class Database : global::FoxDb.Database, IDatabaseComponent
    {
        protected static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        public Database(IProvider provider) : base(provider)
        {

        }

        public Database(IProvider provider, IConfig config) : this(provider)
        {
            config.CopyTo(this.Config);
            this.IsConfigured = true;
        }

        public ICore Core { get; private set; }

        public IDatabaseTables Tables { get; private set; }

        public IDatabaseQueries Queries { get; private set; }

        public bool IsConfigured { get; private set; }

        public virtual void InitializeComponent(ICore core)
        {
            this.Configure();
            this.Core = core;
            this.Tables = new DatabaseTables(this);
            this.Tables.InitializeComponent(core);
            this.Queries = this.CreateQueries();
        }

        protected abstract IDatabaseQueries CreateQueries();

        protected virtual void Configure()
        {
            if (this.IsConfigured)
            {
                return;
            }
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
            this.IsConfigured = true;
        }

        public abstract IDatabaseComponent New();

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
    }
}
