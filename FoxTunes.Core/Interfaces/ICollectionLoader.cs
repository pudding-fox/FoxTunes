using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface ICollectionLoader<T>
    {
        Task Load(Func<IEnumerable<T>> func, Action<ObservableCollection<T>> action);
    }
}
