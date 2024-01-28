using System.Collections.Generic;

namespace FoxTunes.Interfaces
{
    public interface IDatabaseSet : IDatabaseQuery
    {
        int Count { get; }

        void Clear();
    }

    public interface IDatabaseSet<T> : IDatabaseSet, IDatabaseQuery<T> where T : class
    {
        T Add(T item);

        IEnumerable<T> AddRange(IEnumerable<T> items);

        T this[int index] { get; }

        int IndexOf(T item);
    }
}
