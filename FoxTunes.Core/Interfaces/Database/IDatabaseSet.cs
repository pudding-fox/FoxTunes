using System.Collections.Generic;
using System.Collections.ObjectModel;

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

        T Update(T item);

        IEnumerable<T> UpdateRange(IEnumerable<T> items);

        T Remove(T item);

        IEnumerable<T> RemoveRange(IEnumerable<T> items);

        T this[int index] { get; }

        int IndexOf(T item);

        ObservableCollection<T> Local { get; }
    }
}
