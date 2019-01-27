#pragma warning disable 612, 618
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System;
using System.ComponentModel;
using System.Data;
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

        public Database(IProvider provider)
            : base(provider)
        {

        }

        public ICore Core { get; private set; }

        public abstract IsolationLevel PreferredIsolationLevel { get; }

        public IDatabaseTables Tables { get; private set; }

        public IDatabaseQueries Queries { get; private set; }

        public virtual void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.Tables = new DatabaseTables(this);
            this.Tables.InitializeComponent(core);
            this.Queries = this.CreateQueries();
        }

        protected abstract IDatabaseQueries CreateQueries();

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
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.FromResult(false);
#endif
            }
            return this.Error(this, new ComponentErrorEventArgs(message, exception));
        }

        [field: NonSerialized]
        public event ComponentErrorEventHandler Error;
    }
}
