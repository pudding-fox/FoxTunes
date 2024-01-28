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
        public WrappedDbSet(DbSet set)
        {
            this.Set = set;
        }

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
    }

    public class WrappedDbSet<T> : WrappedDbSet, IPersistableSet<T> where T : class
    {
        public WrappedDbSet(DbSet<T> set)
            : base(set)
        {
            this.Set = set;
        }

        new public DbSet<T> Set { get; private set; }

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

        ObservableCollection<T> IPersistableSet<T>.AsObservable()
        {
            return this.Set.Local;
        }
    }
}
