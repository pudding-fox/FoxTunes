using System;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IForegroundTaskRunner : IStandardComponent
    {
        [Obsolete]
        Task Run(Action action);

        [Obsolete]
        Task Run(Func<Task> func);

        Task RunAsync(Action action);

        Task RunAsync(Func<Task> func);
    }
}
