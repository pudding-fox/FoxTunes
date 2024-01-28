using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace FoxTunes
{
    public abstract class EntityFrameworkDatabase : Database
    {
        protected DbContext DbContext { get; private set; }

        protected virtual DbModelBuilder CreateModelBuilder()
        {
            var builder = new DbModelBuilder();
            return builder;
        }

        protected virtual DbModel CreateDbModel()
        {
            var builder = this.CreateModelBuilder();
            return builder.Build(this.Connection);
        }

        protected virtual DbContext CreateDbContext()
        {
            var model = this.CreateDbModel();
            var compiled = model.Compile();
            return new InternalDbContext(this.Connection, compiled);
        }

        protected virtual DbSet<T> GetSet<T>() where T : class
        {
            if (this.DbContext == null)
            {
                this.DbContext = this.CreateDbContext();
            }
            return this.DbContext.Set<T>();
        }
    }
}
