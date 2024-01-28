using System;
using System.Collections.Generic;
using System.Timers;

namespace FoxTunes
{
    public class PendingQueue<T>
    {
        public static readonly object SyncRoot = new object();

        private PendingQueue()
        {
            this.Queue = new Queue<T>();
        }

        public PendingQueue(int timeout)
            : this()
        {
            this.Timer = new Timer(timeout);
            this.Timer.AutoReset = false;
            this.Timer.Elapsed += this.OnElapsed;
        }


        public PendingQueue(TimeSpan timeout)
            : this(Convert.ToInt32(timeout.TotalMilliseconds))
        {

        }
        public Timer Timer { get; private set; }

        public Queue<T> Queue { get; private set; }

        public void Enqueue(T value)
        {
            lock (SyncRoot)
            {
                this.Queue.Enqueue(value);
                this.Timer.Stop();
                this.Timer.Start();
            }
        }

        protected virtual void OnElapsed(object sender, ElapsedEventArgs e)
        {
            lock (SyncRoot)
            {
                this.OnComplete();
            }
        }

        protected virtual void OnComplete()
        {
            if (this.Complete != null)
            {
                var e = new PendingQueueEventArgs<T>(this.Queue.ToArray());
                this.Complete(this, e);
            }
            this.Queue.Clear();
        }

        public event PendingQueueEventHandler<T> Complete;
    }

    public delegate void PendingQueueEventHandler<T>(object sender, PendingQueueEventArgs<T> e);

    public class PendingQueueEventArgs<T> : EventArgs
    {
        public PendingQueueEventArgs(IEnumerable<T> sequence)
        {
            this.Sequence = sequence;
        }

        public IEnumerable<T> Sequence { get; private set; }
    }
}
