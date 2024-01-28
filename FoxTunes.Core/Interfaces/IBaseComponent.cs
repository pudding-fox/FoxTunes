using System;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IBaseComponent : IObservable
    {
        void InitializeComponent(ICore core);

        void Interlocked(Action action);

        Task Interlocked(Func<Task> func);

        T Interlocked<T>(Func<T> func);

        Task<T> Interlocked<T>(Func<Task<T>> func);

        void Interlocked(Action action, TimeSpan timeout);

        Task Interlocked(Func<Task> func, TimeSpan timeout);

        Task<T> Interlocked<T>(Func<Task<T>> func, TimeSpan timeout);
    }
}
