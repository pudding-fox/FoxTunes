using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace FoxTunes
{
    public class CappedDictionary<TKey, TValue>
    {
        public readonly object SyncRoot = new object();

        public CappedDictionary(int capacity)
        {
            this.Keys = new Queue(capacity);
            this.Store = new ConcurrentDictionary<TKey, TValue>(Environment.ProcessorCount, capacity);
            this.Capacity = capacity;
        }

        public Queue Keys { get; private set; }

        public ConcurrentDictionary<TKey, TValue> Store { get; private set; }

        public int Capacity { get; private set; }

        public void Add(TKey key, TValue value)
        {
            this.Store[key] = value;
            while (this.Keys.Count >= (this.Capacity - 1))
            {
                this.Store.TryRemove(this.Keys.Dequeue());
            }
            this.Keys.Enqueue(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            var result = this.Store.TryGetValue(key, out value);
            if (result)
            {
                //When a cache hit occurs, push the key to the back of queue.
                this.Keys.Remove(key);
                this.Keys.Enqueue(key);
            }
            return result;
        }

        public void Clear()
        {
            lock (SyncRoot)
            {
                this.Store.Clear();
                this.Keys.Clear();
            }
        }

        public class Queue
        {
            public readonly object SyncRoot = new object();

            public Queue(int capacity)
            {
                this.Values = new List<TKey>(capacity);
            }

            public List<TKey> Values { get; private set; }

            public void Enqueue(TKey value)
            {
                lock (this.SyncRoot)
                {
                    this.Values.Add(value);
                }
            }

            public void Remove(TKey value)
            {
                lock (this.SyncRoot)
                {
                    this.Values.Remove(value);
                }
            }

            public TKey Dequeue()
            {
                lock (this.SyncRoot)
                {
                    if (this.Values.Count > 0)
                    {
                        var value = this.Values[0];
                        this.Values.RemoveAt(0);
                        return value;
                    }
                    return default(TKey);
                }
            }

            public void Clear()
            {
                lock (SyncRoot)
                {
                    this.Values.Clear();
                }
            }

            public int Count
            {
                get
                {
                    return this.Values.Count;
                }
            }
        }
    }
}
