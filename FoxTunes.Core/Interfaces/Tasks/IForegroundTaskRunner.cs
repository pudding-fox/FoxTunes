using System;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IForegroundTaskRunner : IStandardComponent
    {
        Task Run(Action action);

        Task Run(Func<Task> func);
    }
}
