using FoxTunes.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class BackgroundTaskRunner : StandardComponent, IBackgroundTaskRunner
    {
        public Task Run(Action action, BackgroundTaskPriority priority = BackgroundTaskPriority.None)
        {
            return Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.None, GetScheduler(priority));
        }

        public Task Run(Func<Task> func, BackgroundTaskPriority priority = BackgroundTaskPriority.None)
        {
            return Task.Factory.StartNew(func, CancellationToken.None, TaskCreationOptions.None, GetScheduler(priority));
        }

        public Task<T> Run<T>(Func<T> func, BackgroundTaskPriority priority = BackgroundTaskPriority.None)
        {
            return Task.Factory.StartNew(func, CancellationToken.None, TaskCreationOptions.None, GetScheduler(priority));
        }

        private TaskScheduler GetScheduler(BackgroundTaskPriority priority)
        {
            switch (priority)
            {
                case BackgroundTaskPriority.None:
                    return TaskScheduler.Default;
                case BackgroundTaskPriority.Low:
                    return PriorityScheduler.Low;
                case BackgroundTaskPriority.High:
                    return PriorityScheduler.High;
            }
            throw new NotImplementedException();
        }
    }
}
