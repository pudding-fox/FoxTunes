using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class AsyncCollectionLoader<T> : ICollectionLoader<T>
    {
        public Task Load(Func<IEnumerable<T>> func, Action<ObservableCollection<T>> action)
        {
#if NET40
            return TaskEx.Run(() =>
#else
            return Task.Run(() =>
#endif
            {
                var collection = new ObservableCollection<T>(func());
                return Windows.Invoke(() => action(collection));
            });
        }

        public static ICollectionLoader<T> Instance = new AsyncCollectionLoader<T>();
    }
}
