using System;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IForegroundTaskRunner : IBaseComponent
    {
        Task Run(Action action);
    }
}
