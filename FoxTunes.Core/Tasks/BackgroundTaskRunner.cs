using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class BackgroundTaskRunner : StandardComponent, IBackgroundTaskRunner
    {
        public Task Run(Action action)
        {
            return Task.Run(action);
        }

        public Task Run(Func<Task> func)
        {
            return Task.Run(func);
        }

        public Task<T> Run<T>(Func<T> func)
        {
            return Task.Run(func);
        }
    }
}
