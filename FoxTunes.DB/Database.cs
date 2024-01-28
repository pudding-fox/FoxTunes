using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FoxTunes
{
    public abstract class Database : StandardComponent, IDatabase
    {
        protected virtual DbConnection Connection { get; set; }

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

        public override void InitializeComponent(ICore core)
        {
            this.Connection = this.CreateConnection();
            base.InitializeComponent(core);
        }

        public abstract DbConnection CreateConnection();

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
            this.IsDisposed = true;
        }

        protected virtual void OnDisposing()
        {
            this.Connection.Dispose();
        }
    }
}
