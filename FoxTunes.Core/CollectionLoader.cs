using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class CollectionLoader<T> : ICollectionLoader<T>
    {
        public Task Load(Func<IEnumerable<T>> func, Action<ObservableCollection<T>> action)
        {
            action(new ObservableCollection<T>(func()));
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public static ICollectionLoader<T> Instance = new CollectionLoader<T>();
    }
}
