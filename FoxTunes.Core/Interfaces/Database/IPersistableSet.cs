using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace FoxTunes.Interfaces
{
    public interface IPersistableSet : IOrderedQueryable, IQueryable, IEnumerable, IListSource
    {

    }

    public interface IPersistableSet<T> : IPersistableSet, IOrderedQueryable<T>, IQueryable<T>, IEnumerable<T> where T : class
    {
        ObservableCollection<T> AsObservable();
    }
}
