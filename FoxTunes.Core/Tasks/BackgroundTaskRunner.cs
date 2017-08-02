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
    }
}
