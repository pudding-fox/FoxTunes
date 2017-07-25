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

        public DbContext DbContext { get; private set; }

        public DbSet<T> Set { get; private set; }

        public DbQuery<T> Query { get; private set; }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return ((IEnumerable<T>)this.Query).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.Query).GetEnumerator();
        }

        Type IQueryable.ElementType
        {
            get
            {
                return ((IQueryable)this.Query).ElementType;
            }
        }

        Expression IQueryable.Expression
        {
            get
            {
                return ((IQueryable)this.Query).Expression;
            }
        }

        IQueryProvider IQueryable.Provider
        {
            get
            {
                return ((IQueryable)this.Query).Provider;
            }
        }

        public void Include(string path)
        {
            this.Query = this.Query.Include(path);
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
