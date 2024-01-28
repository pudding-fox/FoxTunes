using System;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IBackgroundTaskRunner : IStandardComponent
    {
        Task Run(Action action, BackgroundTaskPriority priority = BackgroundTaskPriority.None);

        Task Run(Func<Task> func, BackgroundTaskPriority priority = BackgroundTaskPriority.None);

        Task<T> Run<T>(Func<T> func, BackgroundTaskPriority priority = BackgroundTaskPriority.None);
    }

    public enum BackgroundTaskPriority : byte
    {
        None = 0,
        Low = 1,
        High = 2
    }
}
