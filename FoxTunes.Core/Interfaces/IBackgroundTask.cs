using System;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IBackgroundTask : IBaseComponent, IReportsProgress, IDisposable
    {
        string Id { get; }

        bool Visible { get; }

        bool IsCancellationRequested { get; }

        event EventHandler CancellationRequested;

        event AsyncEventHandler Started;

        bool IsStarted { get; }

        event AsyncEventHandler Completed;

        bool IsCompleted { get; }

        Exception Exception { get; }

        event EventHandler ExceptionChanged;

        event AsyncEventHandler Faulted;

        bool IsFaulted { get; }

        Task Run();

        bool Cancellable { get; }

        void Cancel();
    }
}
