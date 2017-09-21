using System;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IBackgroundTaskRunner : IStandardComponent
    {
        Task Run(Action action);

        Task Run(Func<Task> func);

        Task<T> Run<T>(Func<T> func);

        Task<T> Run<T>(Func<Task<T>> func);
    }
}
