using FoxTunes.Interfaces;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace FoxTunes
{
    [Serializable]
    public abstract class BaseComponent : IBaseComponent
    {
        protected static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        private volatile SemaphoreSlim Semaphore;

        private void EnsureSemaphore()
        {
            if (this.Semaphore == null)
            {
                this.Semaphore = new SemaphoreSlim(1);
            }
        }

        public virtual void InitializeComponent(ICore core)
        {
            //Nothing to do.
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged == null)
            {
                return;
            }
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        #region CriticalWrapperRoutines

        public void Interlocked(Action action)
        {
            this.Interlocked(action, Timeout.InfiniteTimeSpan);
        }

        public Task Interlocked(Func<Task> func)
        {
            return this.Interlocked(func, Timeout.InfiniteTimeSpan);
        }

        public T Interlocked<T>(Func<T> func)
        {
            return this.Interlocked(func, Timeout.InfiniteTimeSpan);
        }

        public Task<T> Interlocked<T>(Func<Task<T>> func)
        {
            return this.Interlocked(func, Timeout.InfiniteTimeSpan);
        }

        public void Interlocked(Action action, TimeSpan timeout)
        {
            this.EnsureSemaphore();
            if (!this.Semaphore.Wait(timeout))
            {
                throw new TimeoutException(string.Format("Failed to enter critical section after {0}ms", timeout.TotalMilliseconds));
            }
            try
            {
                action();
            }
            finally
            {
                this.Semaphore.Release();
            }
        }

        public async Task Interlocked(Func<Task> func, TimeSpan timeout)
        {
            this.EnsureSemaphore();
            if (!await this.Semaphore.WaitAsync(timeout))
            {
                throw new TimeoutException(string.Format("Failed to enter critical section after {0}ms", timeout.TotalMilliseconds));
            }
            try
            {
                await func();
            }
            finally
            {
                this.Semaphore.Release();
            }
        }

        public T Interlocked<T>(Func<T> func, TimeSpan timeout)
        {
            this.EnsureSemaphore();
            if (!this.Semaphore.Wait(timeout))
            {
                throw new TimeoutException(string.Format("Failed to enter critical section after {0}ms", timeout.TotalMilliseconds));
            }
            try
            {
                return func();
            }
            finally
            {
                this.Semaphore.Release();
            }
        }

        public async Task<T> Interlocked<T>(Func<Task<T>> func, TimeSpan timeout)
        {
            this.EnsureSemaphore();
            if (!await this.Semaphore.WaitAsync(timeout))
            {
                throw new TimeoutException(string.Format("Failed to enter critical section after {0}ms", timeout.TotalMilliseconds));
            }
            try
            {
                return await func();
            }
            finally
            {
                this.Semaphore.Release();
            }
        }

        #endregion
    }
}
