using FoxTunes.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace FoxTunes
{
    public class WrappedDbSet<T> : IDatabaseSet<T> where T : class
    {
        public WrappedDbSet(DbContext dbContext, DbSet<T> set)
        {
            this.DbContext = dbContext;
            this.Set = set;
        }

        public DbContext DbContext { get; private set; }

        public DbSet<T> Set { get; private set; }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return ((IEnumerable<T>)this.Set).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.Set).GetEnumerator();
        }

        Type IQueryable.ElementType
        {
            get
            {
                return ((IQueryable)this.Set).ElementType;
            }
        }

        Expression IQueryable.Expression
        {
            get
            {
                return ((IQueryable)this.Set).Expression;
            }
        }

        IQueryProvider IQueryable.Provider
        {
            get
            {
                return ((IQueryable)this.Set).Provider;
            }
        }

        public void Include(string path)
        {
            throw new NotImplementedException();
        }

        public void Include<TProperty>(Expression<Func<T, TProperty>> path)
        {
            throw new NotImplementedException();
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

        public T Add(T item)
        {
            return this.Set.Add(item);
        }

        public IEnumerable<T> AddRange(IEnumerable<T> items)
        {
            return this.Set.AddRange(items);
        }

        public T Update(T item)
        {
            this.DbContext.Entry(item).State = EntityState.Modified;
            return item;
        }

        public IEnumerable<T> UpdateRange(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                this.Update(item);
            }
            return items;
        }

        public T Remove(T item)
        {
            return this.Set.Remove(item);
        }

        public IEnumerable<T> RemoveRange(IEnumerable<T> items)
        {
            return this.Set.RemoveRange(items);
        }

        public int IndexOf(T item)
        {
            return this.Set.Local.IndexOf(item);
        }

        public T this[int index]
        {
            get
            {
                return this.Set.Local[index];
            }
        }

        public int Count
        {
            get
            {
                return this.Set.Local.Count;
            }
        }

        public void Clear()
        {
            this.Set.Local.Clear();
        }

        public ObservableCollection<T> Local
        {
            get
            {
                return this.Set.Local;
            }
        }
    }
}
