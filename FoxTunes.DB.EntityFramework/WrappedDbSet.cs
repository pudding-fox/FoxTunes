using FoxTunes.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace FoxTunes
{
    public class WrappedDbSet : IPersistableSet
    {
        public WrappedDbSet(DbContext dbContext, DbSet set)
        {
            this.DbContext = dbContext;
            this.Set = set;
        }

        public DbContext DbContext { get; private set; }

        public DbSet Set { get; private set; }

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

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.Set).GetEnumerator();
        }

        bool IListSource.ContainsListCollection
        {
            get
            {
                return ((IListSource)this.Set).ContainsListCollection;
            }
        }

        IList IListSource.GetList()
        {
            return ((IListSource)this.Set).GetList();
        }

        public void LoadReference(string property)
        {
            foreach (var item in this.Set)
            {
                var reference = this.DbContext.Entry(item).Reference(property);
                if (reference.IsLoaded)
                {
                    continue;
                }
                reference.Load();
            }
        }

        public void LoadCollection(string property)
        {
            foreach (var item in this.Set)
            {
                var collection = this.DbContext.Entry(item).Collection(property);
                if (collection.IsLoaded)
                {
                    continue;
                }
                collection.Load();
            }
        }
    }

    public class WrappedDbSet<T> : WrappedDbSet, IPersistableSet<T> where T : class
    {
        public WrappedDbSet(DbContext dbContext, DbSet<T> set)
            : base(dbContext, set)
        {
            this.Set = set;
        }

        new public DbSet<T> Set { get; private set; }

        public bool IsLoaded { get; private set; }

        public void Load()
        {
            this.Set.Load();
            this.IsLoaded = true;
        }

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

        public void LoadReference<TProperty>(Expression<Func<T, TProperty>> property) where TProperty : class
        {
            foreach (var item in this.Set)
            {
                var reference = this.DbContext.Entry(item).Reference(property);
                if (reference.IsLoaded)
                {
                    continue;
                }
                reference.Load();
            }
        }

        public void LoadCollection<TElement>(Expression<Func<T, ICollection<TElement>>> property) where TElement : class
        {
            foreach (var item in this.Set)
            {
                var collection = this.DbContext.Entry(item).Collection(property);
                if (collection.IsLoaded)
                {
                    continue;
                }
                collection.Load();
            }
        }

        ObservableCollection<T> IPersistableSet<T>.AsObservable()
        {
            if (!this.IsLoaded)
            {
                this.Load();
            }
            return this.Set.Local;
        }
    }
}
