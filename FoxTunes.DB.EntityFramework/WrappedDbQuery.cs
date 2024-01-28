using FoxTunes.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;

namespace FoxTunes
{
    public class WrappedDbQuery<T> : IDatabaseQuery<T> where T : class
    {
        public WrappedDbQuery(DbContext dbContext, DbSet<T> set)
        {
            this.DbContext = dbContext;
            this.Set = set;
            this.Query = set;
        }

        public WrappedDbQuery(DbContext dbContext, DbSet<T> set, IQueryable<T> query)
        {
            this.DbContext = dbContext;
            this.Set = set;
            this.Query = query;
        }

        public DbContext DbContext { get; private set; }

        public DbSet<T> Set { get; private set; }

        public IQueryable<T> Query { get; private set; }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return this.Query.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.Query).GetEnumerator();
        }

        Type IQueryable.ElementType
        {
            get
            {
                return this.Query.ElementType;
            }
        }

        Expression IQueryable.Expression
        {
            get
            {
                return this.Query.Expression;
            }
        }

        IQueryProvider IQueryable.Provider
        {
            get
            {
                return this.Query.Provider;
            }
        }

        public void Include(string path)
        {
            this.Query = this.Query.Include(path);
        }

        public void Include<TProperty>(Expression<Func<T, TProperty>> path)
        {
            this.Query = this.Query.Include(path) as DbQuery<T>;
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add
            {
                this.Set.Local.CollectionChanged += value;
            }
            remove
            {
                this.Set.Local.CollectionChanged -= value;
            }
        }
    }
}
