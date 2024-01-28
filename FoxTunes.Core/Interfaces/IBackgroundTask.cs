using System;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IBackgroundTask : IBaseComponent, IReportsProgress
    {
        string Id { get; }

        bool Visible { get; }

        event AsyncEventHandler Started;

        event AsyncEventHandler Completed;

        Exception Exception { get; }

        event EventHandler ExceptionChanged;

        event AsyncEventHandler Faulted;

        Task Run();
    }
}
