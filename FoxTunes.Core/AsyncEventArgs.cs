using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    public abstract class AsyncEventArgs<T> : EventArgs
    {
        protected AsyncEventArgs()
        {
            this.Sources = new List<TaskCompletionSource<T>>();
        }

        public Deferral<T> Defer()
        {
            var source = new TaskCompletionSource<T>();
            var deferral = new Deferral<T>(result => source.SetResult(result));
            this.Sources.Add(source);
            return deferral;
        }

        public IList<TaskCompletionSource<T>> Sources { get; private set; }

        public async Task Complete()
        {
            foreach (var source in this.Sources)
            {
                await source.Task;
            }
        }
    }

    public abstract class AsyncEventArgs : AsyncEventArgs<object>
    {
    }
}
