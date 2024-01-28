using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;

namespace FoxTunes
{
    public class KeyLock<T>
    {
        protected static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        public KeyLock()
        {
            this.Counters = new Dictionary<T, Counter>();
        }

        public Dictionary<T, Counter> Counters { get; private set; }

        protected virtual SemaphoreSlim CreateOrIncrement(T key)
        {
            var counter = default(Counter);
            lock (this.Counters)
            {
                if (this.Counters.TryGetValue(key, out counter))
                {
                    counter.Count++;
                }
                else
                {
                    counter = new Counter();
                    this.Counters[key] = counter;
                }
            }
            return counter.Semaphore;
        }

        protected virtual void Remove(T key)
        {
            var counter = default(Counter);
            lock (this.Counters)
            {
                if (!this.Counters.TryGetValue(key, out counter))
                {
                    return;
                }
                counter.Semaphore.Release();
                counter.Count--;
                if (counter.Count == 0)
                {
                    this.Counters.Remove(key);
                    counter.Dispose();
                }
            }
        }

        public IDisposable Lock(T key)
        {
            this.CreateOrIncrement(key).Wait();
            return new Releaser(this, key);
        }

        public class Counter : IDisposable
        {
            public Counter()
            {
                this.Count = 1;
                this.Semaphore = new SemaphoreSlim(1, 1);
            }

            public int Count { get; set; }

            public SemaphoreSlim Semaphore { get; private set; }

            public bool IsDisposed { get; private set; }

            public void Dispose()
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (this.IsDisposed || !disposing)
                {
                    return;
                }
                this.OnDisposing();
                this.IsDisposed = true;
            }

            protected virtual void OnDisposing()
            {
                this.Semaphore.Dispose();
            }

            ~Counter()
            {
                Logger.Write(this.GetType(), LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
                this.Dispose(true);
            }
        }

        public class Releaser : IDisposable
        {
            public Releaser(KeyLock<T> owner, T key)
            {
                this.Owner = owner;
                this.Key = key;
            }

            public KeyLock<T> Owner { get; private set; }

            public T Key { get; private set; }

            public bool IsDisposed { get; private set; }

            public void Dispose()
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (this.IsDisposed || !disposing)
                {
                    return;
                }
                this.OnDisposing();
                this.IsDisposed = true;
            }

            protected virtual void OnDisposing()
            {
                this.Owner.Remove(this.Key);
            }

            ~Releaser()
            {
                Logger.Write(this.GetType(), LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
                this.Dispose(true);
            }
        }
    }
}
