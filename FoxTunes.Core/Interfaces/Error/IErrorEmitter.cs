using System;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IErrorEmitter : IErrorSource
    {
        Task Send(string message);

        Task Send(Exception exception);

        Task Send(string message, Exception exception);
    }
}
