#if NET40
using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class AsyncSemaphore : IDisposable
    {
        protected static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        private AsyncSemaphore()
        {
            this.Tasks = new Queue<TaskCompletionSource<bool>>();
        }

        public AsyncSemaphore(int initialCount) : this()
        {
            this.Count = initialCount;
        }

        public Queue<TaskCompletionSource<bool>> Tasks { get; private set; }

        public int Count { get; private set; }

        public Task WaitAsync()
        {
            lock (this.Tasks)
            {
                if (this.Count > 0)
                {
                    this.Count--;
#if NET40
                    return TaskEx.FromResult(false);
#else
                    return Task.CompletedTask;
#endif
                }
                else
                {
                    var source = new TaskCompletionSource<bool>();
                    this.Tasks.Enqueue(source);
                    return source.Task;
                }
            }
        }

        public Task<bool> WaitAsync(int timeout)
        {
            lock (this.Tasks)
            {
                if (this.Count > 0)
                {
                    this.Count--;
#if NET40
                    return TaskEx.FromResult(true);
#else
                    return Task.CompletedTask;
#endif
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        public void Release()
        {
            lock (this.Tasks)
            {
                if (this.Tasks.Count > 0)
                {
                    var source = this.Tasks.Dequeue();
                    source.SetResult(true);
                }
                else
                {
                    this.Count++;
                }
            }
        }

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
            //Nothing to do.
        }

        ~AsyncSemaphore()
        {
            Logger.Write(this.GetType(), LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            this.Dispose(true);
        }
    }
}
#endif