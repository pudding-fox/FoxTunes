using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace FoxTunes.Interfaces
{
    public interface IDatabaseQuery : IOrderedQueryable, IQueryable, IEnumerable, INotifyCollectionChanged
    {
        void Include(string path);
    }

    public interface IDatabaseQuery<T> : IDatabaseQuery, IOrderedQueryable<T>, IQueryable<T>, IEnumerable<T> where T : class
    {

    }
}
