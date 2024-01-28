using System;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IBackgroundTaskRunner : IStandardComponent
    {
        Task Run(Action action);

        Task Run(Func<Task> func);
    }
}
