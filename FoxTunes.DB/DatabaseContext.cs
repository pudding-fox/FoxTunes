using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FoxTunes
{
    public abstract class DatabaseContext : BaseComponent, IDatabaseContext
    {
        public abstract IDbConnection Connection { get; }

        public IRootDataSets Sets { get; protected set; }

        public IRootDataQueries Queries { get; protected set; }

        public abstract IDatabaseSet<T> GetSet<T>() where T : class;

        public abstract IDatabaseQuery<T> GetQuery<T>() where T : class;

        public abstract bool CanQuery<T>(T item) where T : class;

        public abstract IDatabaseQuery<TMember> GetMemberQuery<T, TMember>(T item, Expression<Func<T, TMember>> member)
            where T : class
            where TMember : class;

        public abstract IDatabaseQuery<TMember> GetMemberQuery<T, TMember>(T item, Expression<Func<T, ICollection<TMember>>> member)
            where T : class
            where TMember : class;

        public abstract void WithAutoDetectChanges(Action action);

        public abstract T WithAutoDetectChanges<T>(Func<T> func);

        public abstract int SaveChanges();

        public abstract Task<int> SaveChangesAsync();

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.IsDisposed || !disposing)
            {
                return;
            }
            this.OnDisposing();
            if (this.Disposed != null)
            {
                this.Disposed(this, EventArgs.Empty);
            }
            this.IsDisposed = true;
        }

        protected abstract void OnDisposing();

        public event EventHandler Disposed = delegate { };

        ~DatabaseContext()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            this.Dispose(true);
        }
    }
}
