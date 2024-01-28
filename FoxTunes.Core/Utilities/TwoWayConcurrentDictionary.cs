using System.Collections.Concurrent;

namespace FoxTunes
{
    public class TwoWayConcurrentDictionary<T>
    {
        public TwoWayConcurrentDictionary()
        {
            this.Store1 = new ConcurrentDictionary<T, T>();
            this.Store2 = new ConcurrentDictionary<T, T>();
        }

        public ConcurrentDictionary<T, T> Store1 { get; private set; }

        public ConcurrentDictionary<T, T> Store2 { get; private set; }

        public bool TryAdd(T element1, T element2)
        {
            var success = true;
            success |= this.Store1.TryAdd(element1, element2);
            success |= this.Store2.TryAdd(element2, element1);
            return success;
        }

        public bool TryGet1(T key, out T value)
        {
            return this.Store1.TryGetValue(key, out value);
        }

        public bool TryGet2(T key, out T value)
        {
            return this.Store2.TryGetValue(key, out value);
        }

        public bool TryRemove(T key, out T value)
        {
            var success = true;
            success |= this.Store2.TryRemove(key, out value);
            success |= this.Store1.TryRemove(value, out key);
            return success;
        }
    }
}
