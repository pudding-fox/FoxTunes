using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class PendingQueue<T> : IEnumerable<T>
    {
        private volatile bool Completing = false;

        private PendingQueue()
        {
            this.Queue = new Queue<T>();
#if NET40
            this.Semaphore = new AsyncSemaphore(1);
#else
            this.Semaphore = new SemaphoreSlim(1, 1);
#endif
        }

        public PendingQueue(int timeout)
            : this()
        {
            this.Timeout = timeout;
        }

        public PendingQueue(TimeSpan timeout)
            : this(Convert.ToInt32(timeout.TotalMilliseconds))
        {

        }

        public Queue<T> Queue { get; private set; }

#if NET40
        public AsyncSemaphore Semaphore { get; private set; }
#else
        public SemaphoreSlim Semaphore { get; private set; }
#endif

        public int Timeout { get; private set; }

        public async Task Enqueue(T value)
        {
            await this.Semaphore.WaitAsync();
            try
            {
                this.Queue.Enqueue(value);
            }
            finally
            {
                this.Semaphore.Release();
            }
            await this.BeginComplete();
        }

        protected async Task BeginComplete()
        {
#if NET40
            await TaskEx.Delay(this.Timeout);
#else
            await Task.Delay(this.Timeout);
#endif
            if (this.Completing)
            {
                return;
            }
            this.Completing = true;
            await this.Semaphore.WaitAsync();
            try
            {
                await this.OnComplete();
                this.Queue.Clear();
            }
            finally
            {
                this.Semaphore.Release();
            }
            this.Completing = false;
        }

        protected virtual Task OnComplete()
        {
            if (this.Complete == null)
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            var e = new PendingQueueEventArgs<T>(this);
            this.Complete(this, e);
            return e.Complete();
        }

        public event PendingQueueEventHandler<T> Complete = delegate { };

        public IEnumerator<T> GetEnumerator()
        {
            return this.Queue.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    public delegate void PendingQueueEventHandler<T>(object sender, PendingQueueEventArgs<T> e);

    public class PendingQueueEventArgs<T> : AsyncEventArgs
    {
        public PendingQueueEventArgs(IEnumerable<T> sequence)
        {
            this.Sequence = sequence;
        }

        public IEnumerable<T> Sequence { get; private set; }
    }
}
