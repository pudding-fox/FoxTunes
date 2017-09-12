using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IDatabaseSet : IDatabaseQuery
    {
        int Count { get; }

        void Clear();
    }

    public interface IDatabaseSet<T> : IDatabaseSet, IDatabaseQuery<T> where T : class
    {
        void Load();

        Task LoadAsync();

        T Attach(T item);

        IEnumerable<T> AttachRange(IEnumerable<T> items);

        T Detach(T item);

        IEnumerable<T> DetachRange(IEnumerable<T> items);

        T Find(params object[] keyValues);

        T Add(T item);

        IEnumerable<T> AddRange(IEnumerable<T> items);

        T Update(T item);

        IEnumerable<T> UpdateRange(IEnumerable<T> items);

        T SetCurrentValues(T item, params object[] keyValues);

        T Remove(T item);

        IEnumerable<T> RemoveRange(IEnumerable<T> items);

        T this[int index] { get; }

        int IndexOf(T item);

        IEnumerable<T> Query(string sql, params object[] parameters);

        ObservableCollection<T> Local { get; }
    }
}
