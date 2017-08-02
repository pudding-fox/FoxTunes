using FoxTunes.Interfaces;
using System;
using System.Linq;
using System.Collections.Concurrent;

namespace FoxTunes
{
    public class OutputStreamQueue : StandardComponent, IOutputStreamQueue
    {
        const int QUEUE_CAPACITY = 3;

        public OutputStreamQueue()
        {
            this.Queue = new ConcurrentDictionary<PlaylistItem, OutputStreamQueueValue>();
        }

        private ConcurrentDictionary<PlaylistItem, OutputStreamQueueValue> Queue { get; set; }

        public bool IsQueued(PlaylistItem playlistItem)
        {
            return this.Queue.ContainsKey(playlistItem);
        }

        public void Enqueue(IOutputStream outputStream, bool dequeue)
        {
            if (!this.Queue.TryAdd(outputStream.PlaylistItem, new OutputStreamQueueValue(outputStream)))
            {
                throw new InvalidOperationException("Failed to add the specified output stream to the queue.");
            }
            this.EnsureCapacity(outputStream.PlaylistItem);
            if (!dequeue)
            {
                return;
            }
            this.Dequeue(outputStream.PlaylistItem);
        }

        private void EnsureCapacity(PlaylistItem playlistItem)
        {
            //We remove the oldest items in the queue to enforce the capacity.
            //Hopefully they are not needed.
            var query =
                from key in this.Queue.Keys
                where key != playlistItem
                orderby this.Queue[key].CreatedAt
                select key;
            while (this.Queue.Count > QUEUE_CAPACITY)
            {
                var key = query.FirstOrDefault();
                if (key == null)
                {
                    return;
                }
                var value = default(OutputStreamQueueValue);
                this.Queue.TryRemove(key, out value);
                value.OutputStream.Dispose();
            }
        }

        public void Dequeue(PlaylistItem playlistItem)
        {
            var value = default(OutputStreamQueueValue);
            if (!this.Queue.TryRemove(playlistItem, out value))
            {
                throw new InvalidOperationException("Failed to locate the specified playlist item in the queue.");
            }
            this.OnDequeued(value.OutputStream);
        }

        protected virtual void OnDequeued(IOutputStream outputStream)
        {
            if (this.Dequeued == null)
            {
                return;
            }
            this.Dequeued(this, new OutputStreamQueueEventArgs(outputStream));
        }

        public event OutputStreamQueueEventHandler Dequeued;

        private class OutputStreamQueueValue
        {
            private OutputStreamQueueValue()
            {
                this.CreatedAt = DateTime.UtcNow;
            }

            public OutputStreamQueueValue(IOutputStream outputStream)
                : this()
            {
                this.OutputStream = outputStream;
            }

            public IOutputStream OutputStream { get; private set; }

            public DateTime CreatedAt { get; private set; }
        }
    }
}
