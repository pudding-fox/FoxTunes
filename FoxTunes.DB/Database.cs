using FoxTunes.Interfaces;
using System;
using System.Data;
using System.Data.Common;

namespace FoxTunes
{
    public abstract class Database : StandardComponent, IDatabase
    {
        protected virtual DbConnection Connection { get; set; }

        public abstract IPersistableSet GetSet(Type type);

        public abstract IPersistableSet<T> GetSet<T>() where T : class;

        public abstract int SaveChanges();

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
