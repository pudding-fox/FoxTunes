using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Transactions;

namespace FoxTunes
{
    public class EntityFrameworkDatabaseContext : DatabaseContext
    {
        public EntityFrameworkDatabaseContext(DbContext dbContext)
        {
            this.DbContext = dbContext;
            this.Sets = new RootDataSets(this);
            this.Queries = new RootDataQueries(this);
        }

        protected DbContext DbContext { get; private set; }

        protected ObjectContext ObjectContext
        {
            get
            {
                return (this.DbContext as IObjectContextAdapter).ObjectContext;
            }
        }

        protected IDbConnection Connection
        {
            get
            {
                return this.DbContext.Database.Connection;
            }
        }

        protected virtual void EnsureConnected()
        {
            switch (this.Connection.State)
            {
                case ConnectionState.Closed:
                    this.Connection.Open();
                    break;
            }
        }

        public override IDatabaseSet<T> GetSet<T>()
        {
            Logger.Write(this, LogLevel.Debug, "Creating wrapped database set: {0}", typeof(T).Name);
            return new WrappedDbSet<T>(this.DbContext, this.DbContext.Set<T>());
        }

        public override IDatabaseQuery<T> GetQuery<T>()
        {
            Logger.Write(this, LogLevel.Debug, "Creating wrapped database query: {0}", typeof(T).Name);
            return new WrappedDbQuery<T>(this.DbContext, this.DbContext.Set<T>());
        }

        public override bool CanQuery<T>(T item)
        {
            var entry = this.DbContext.Entry(item);
            return entry.State != EntityState.Added && entry.State != EntityState.Detached;
        }

        public override IDatabaseQuery<TMember> GetMemberQuery<T, TMember>(T item, Expression<Func<T, TMember>> member)
        {
            if (!this.CanQuery(item))
            {
                throw new InvalidOperationException("Item is untracked, cannot query.");
            }
            Logger.Write(this, LogLevel.Debug, "Creating wrapped database member query: {0} => {1}", typeof(T).Name, typeof(TMember).Name);
            var entry = this.DbContext.Entry(item);
            return new WrappedDbQuery<TMember>(
                this.DbContext,
                this.DbContext.Set<TMember>(),
                entry.Reference(member).Query()
            );
        }

        public override IDatabaseQuery<TMember> GetMemberQuery<T, TMember>(T item, Expression<Func<T, ICollection<TMember>>> member)
        {
            if (!this.CanQuery(item))
            {
                throw new InvalidOperationException("Item is untracked, cannot query.");
            }
            Logger.Write(this, LogLevel.Debug, "Creating wrapped database member collection query: {0} => {1}", typeof(T).Name, typeof(TMember).Name);
            var entry = this.DbContext.Entry(item);
            return new WrappedDbQuery<TMember>(
                this.DbContext,
                this.DbContext.Set<TMember>(),
                entry.Collection(member).Query()
            );
        }

        public override int Execute(string commandText, IDictionary<string, object> parameters = null)
        {
            this.EnsureConnected();
            using (var command = this.Connection.CreateCommand(commandText, parameters))
            {
                return command.ExecuteNonQuery();
            }
        }

        public override T Execute<T>(string commandText, IDictionary<string, object> parameters = null)
        {
            throw new NotImplementedException();
        }

        public override void WithAutoDetectChanges(Action action)
        {
            Logger.Write(this, LogLevel.Debug, "Begin executing action with auto detect changes enabled.");
            this.DbContext.Configuration.AutoDetectChangesEnabled = true;
            try
            {
                action();
            }
            finally
            {
                Logger.Write(this, LogLevel.Debug, "End executing action with auto detect changes enabled.");
                this.DbContext.Configuration.AutoDetectChangesEnabled = false;
            }
        }

        public override T WithAutoDetectChanges<T>(Func<T> func)
        {
            Logger.Write(this, LogLevel.Debug, "Begin executing action with auto detect changes enabled.");
            this.DbContext.Configuration.AutoDetectChangesEnabled = true;
            try
            {
                return func();
            }
            finally
            {
                Logger.Write(this, LogLevel.Debug, "End executing action with auto detect changes enabled.");
                this.DbContext.Configuration.AutoDetectChangesEnabled = false;
            }
        }

        public override int SaveChanges()
        {
            if (this.DbContext == null)
            {
                return 0;
            }
            using (var transaction = this.BeginTransaction())
            {
                var count = this.DbContext.SaveChanges();
                transaction.Complete();
                Logger.Write(this, LogLevel.Debug, "Saved {0} changes to database.", count);
                return count;
            }
        }

        public override async Task<int> SaveChangesAsync()
        {
            if (this.DbContext == null)
            {
                return 0;
            }
            using (var transaction = this.BeginTransaction())
            {
                var count = await this.DbContext.SaveChangesAsync();
                transaction.Complete();
                Logger.Write(this, LogLevel.Debug, "Saved {0} changes to database.", count);
                return count;
            }
        }

        private TransactionScope BeginTransaction()
        {
            var options = new TransactionOptions();
            return new TransactionScope(TransactionScopeOption.Required, options);
        }

        protected override void OnDisposing()
        {
            this.DbContext.Dispose();
        }
    }
}
