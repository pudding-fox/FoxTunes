using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class PendingQueue<T> : IEnumerable<T>
    {
        private volatile bool Completing = false;

        private PendingQueue()
        {
            this.Queue = new Queue<T>();
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

        public int Timeout { get; private set; }

        public void Enqueue(T value)
        {
            lock (this.Queue)
            {
                this.Queue.Enqueue(value);
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
            lock (this.Queue)
            {
                this.OnComplete();
                this.Queue.Clear();
            }
            this.Completing = false;
        }


        protected virtual void OnComplete()
        {
            if (this.Complete == null)
            {
                return;
            }
            this.Complete(this, EventArgs.Empty);
        }

        public event EventHandler Complete = delegate { };

        public IEnumerator<T> GetEnumerator()
        {
            return this.Queue.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
