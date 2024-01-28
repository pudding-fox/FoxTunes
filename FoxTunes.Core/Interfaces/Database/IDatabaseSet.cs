using System;
using System.Collections.Generic;

namespace FoxTunes.Interfaces
{
    public interface IDatabaseSet<T> : IEnumerable<T> where T : IPersistableComponent
    {
        T Find(object id);

        int Count { get; }

        IEnumerable<T> Query(IDatabaseQuery query);

        IEnumerable<T> Query(IDatabaseQuery query, Action<IDbParameterCollection> parameters);

        void AddOrUpdate(T item);

        void AddOrUpdate(IEnumerable<T> items);

        void Delete(T item);

        void Delete(IEnumerable<T> items);

        void Clear();
    }
}
