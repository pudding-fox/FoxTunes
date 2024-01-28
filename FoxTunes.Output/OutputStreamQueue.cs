using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;

namespace FoxTunes
{
    public class OutputStreamQueue : StandardComponent, IOutputStreamQueue
    {
        public OutputStreamQueue()
        {
            this.Queue = new Queue<IOutputStream>();
        }

        public Queue<IOutputStream> Queue { get; private set; }

        public IOutputStream Dequeue()
        {
            return this.Queue.Dequeue();
        }

        public void Enqueue(IOutputStream outputStream)
        {
            this.Queue.Enqueue(outputStream);
            this.OnEnqueued();
        }

        protected virtual void OnEnqueued()
        {
            if (this.Enqueued == null)
            {
                return;
            }
            this.Enqueued(this, EventArgs.Empty);
        }

        public event EventHandler Enqueued = delegate { };
    }
}
