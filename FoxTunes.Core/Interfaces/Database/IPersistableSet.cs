using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace FoxTunes.Interfaces
{
    public interface IPersistableSet : IOrderedQueryable, IQueryable, IEnumerable, IListSource
    {
        void LoadReference(string path);

        void LoadCollection(string path);
    }

    public interface IPersistableSet<T> : IPersistableSet, IOrderedQueryable<T>, IQueryable<T>, IEnumerable<T> where T : class
    {
        void LoadReference<TProperty>(Expression<Func<T, TProperty>> property) where TProperty : class;

        void LoadCollection<TElement>(Expression<Func<T, ICollection<TElement>>> property) where TElement : class;

        ObservableCollection<T> AsObservable();
    }
}
