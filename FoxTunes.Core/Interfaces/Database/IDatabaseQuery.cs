using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;

namespace FoxTunes.Interfaces
{
    public interface IDatabaseQuery : IOrderedQueryable, IQueryable, IEnumerable, INotifyCollectionChanged
    {

    }

    public interface IDatabaseQuery<T> : IDatabaseQuery, IOrderedQueryable<T>, IQueryable<T>, IEnumerable<T> where T : class
    {
        IDatabaseQuery<T> Detach();

        IDatabaseQuery<T> Include(string path);

        IDatabaseQuery<T> Include<TProperty>(Expression<Func<T, TProperty>> path);
    }
}
