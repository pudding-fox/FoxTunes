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
            this.Semaphore = new SemaphoreSlim(1, 1);
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

        public SemaphoreSlim Semaphore { get; private set; }

        public int Timeout { get; private set; }

        public void Enqueue(T value)
        {
            this.Semaphore.Wait();
            try
            {
                this.Queue.Enqueue(value);
            }
            finally
            {
                this.Semaphore.Release();
            }
            Task.Factory.StartNew(() => this.BeginComplete());
        }

        protected async Task BeginComplete()
        {
            await Task.Delay(this.Timeout);
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
                return Task.CompletedTask;
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
