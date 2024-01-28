using FoxTunes.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class PriorityScheduler : TaskScheduler, IDisposable
    {
        private PriorityScheduler()
        {
            this.Tasks = new BlockingCollection<Task>();
        }

        public PriorityScheduler(ThreadPriority priority)
            : this()
        {
            this.Priority = priority;
        }

        public ThreadPriority Priority { get; private set; }

        private BlockingCollection<Task> Tasks { get; set; }

        private Thread[] Threads { get; set; }

        public string Name
        {
            get
            {
                return string.Format("Scheduler:{0}", Enum.GetName(typeof(ThreadPriority), this.Priority));
            }
        }

        public override int MaximumConcurrencyLevel
        {
            get
            {
                return Math.Max(1, Environment.ProcessorCount);
            }
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return this.Tasks;
        }

        protected override void QueueTask(Task task)
        {
            this.EnsureThreads();
            this.Tasks.Add(task);
        }

        private void EnsureThreads()
        {
            if (this.Threads != null)
            {
                return;
            }
            LogManager.Logger.Write(typeof(PriorityScheduler), LogLevel.Trace, "Creating scheduler {0} thread pool.", this.Name);
            this.Threads = new Thread[this.MaximumConcurrencyLevel];
            for (var a = 0; a < this.Threads.Length; a++)
            {
                this.Threads[a] = new Thread(this.OnExecuteTasks);
                this.Threads[a].Name = string.Format("{0}:{1}", this.Name, a);
                this.Threads[a].Priority = this.Priority;
                this.Threads[a].IsBackground = true;
                this.Threads[a].Start();
            }
            LogManager.Logger.Write(typeof(PriorityScheduler), LogLevel.Trace, "Created scheduler {0} thread pool with {1} threads.", this.Name, this.Threads.Length);
        }

        public void OnExecuteTasks()
        {
            this.YieldIfRequired();
            try
            {
                foreach (var task in this.Tasks.GetConsumingEnumerable())
                {
                    this.YieldIfRequired();
                    this.TryExecuteTask(task);
                    this.YieldIfRequired();
                }
            }
            catch
            {
            }
        }

        protected virtual void Shutdown()
        {
            if (this.Threads == null)
            {
                return;
            }
            this.Tasks.CompleteAdding();
            foreach (var thread in this.Threads)
            {
                thread.Join();
            }
            this.Tasks.Dispose();
            this.Tasks = new BlockingCollection<Task>();
            this.Threads = null;
        }

        private void YieldIfRequired()
        {
            if (this.Priority >= ThreadPriority.Normal)
            {
                return;
            }
            Thread.Yield();
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return false;
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
            this.Shutdown();
        }

        ~PriorityScheduler()
        {
            LogManager.Logger.Write(typeof(PriorityScheduler), LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            this.Dispose(true);
        }

        public static readonly PriorityScheduler Low = new PriorityScheduler(ThreadPriority.Lowest);

        public static readonly PriorityScheduler High = new PriorityScheduler(ThreadPriority.Highest);
    }
}
