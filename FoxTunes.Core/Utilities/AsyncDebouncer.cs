using FoxTunes.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace FoxTunes
{
    public class AsyncDebouncer : IDisposable
    {
        protected static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        private AsyncDebouncer()
        {
            this.Tasks = new ConcurrentDictionary<Func<Task>, TaskCompletionSource<bool>>();
        }

        public AsyncDebouncer(int timeout) : this()
        {
            this.Timer = new global::System.Timers.Timer(timeout);
            this.Timer.AutoReset = false;
            this.Timer.Elapsed += this.OnElapsed;
        }

        public AsyncDebouncer(TimeSpan timeout) : this(Convert.ToInt32(timeout.TotalMilliseconds))
        {

        }

        public Task Exec(Func<Task> task)
        {
            var completionSource = this.Tasks.GetOrAdd(task, key => new TaskCompletionSource<bool>());
            if (this.Timer != null)
            {
                this.Timer.Stop();
                this.Timer.Start();
            }
            return completionSource.Task;
        }

        public async Task ExecNow(Func<Task> task)
        {
            var completionSource = default(TaskCompletionSource<bool>);
            this.Tasks.TryRemove(task, out completionSource);
            await task().ConfigureAwait(false);
            if (completionSource != null)
            {
                completionSource.TrySetResult(true);
            }
        }

        public void Cancel(Func<Task> task)
        {
            var completionSource = default(TaskCompletionSource<bool>);
            this.Tasks.TryRemove(task, out completionSource);
            if (this.Tasks.Count == 0)
            {
                this.Timer.Stop();
            }
            if (completionSource != null)
            {
                completionSource.TrySetResult(false);
            }
        }

        public void Wait()
        {
            while (true)
            {
                if (this.Timer == null || !this.Timer.Enabled)
                {
                    return;
                }
                if (this.Tasks.Count == 0)
                {
                    return;
                }
                Thread.Sleep(1000);
            }
        }

        protected virtual void OnElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                var tasks = this.Tasks.Select(pair =>
                {
                    try
                    {
                        return pair.Key().ContinueWith(task =>
                        {
                            if (task.IsFaulted)
                            {
                                pair.Value.SetException(task.Exception);
                            }
                            else
                            {
                                pair.Value.SetResult(true);
                            }
                        });
                    }
                    finally
                    {
                        this.Tasks.TryRemove(pair.Key);
                    }
                }).ToArray();
                Task.WaitAll(tasks);
            }
            catch
            {
                //Nothing can be done, never throw on background thread.
            }
        }

        public ConcurrentDictionary<Func<Task>, TaskCompletionSource<bool>> Tasks { get; private set; }

        public global::System.Timers.Timer Timer { get; private set; }

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
            if (this.Timer != null)
            {
                this.Timer.Elapsed -= this.OnElapsed;
                this.Timer.Dispose();
                this.Timer = null;
            }
        }

        ~AsyncDebouncer()
        {
            Logger.Write(this.GetType(), LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            try
            {
                this.Dispose(true);
            }
            catch
            {
                //Nothing can be done, never throw on GC thread.
            }
        }
    }
}
