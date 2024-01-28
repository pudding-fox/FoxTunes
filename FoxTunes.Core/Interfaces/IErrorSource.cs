using System;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IErrorSource : IStandardComponent
    {
        event ComponentErrorEventHandler Error;
    }

    public delegate Task ComponentErrorEventHandler(object sender, ComponentErrorEventArgs e);

    public class ComponentErrorEventArgs : EventArgs
    {
        public ComponentErrorEventArgs(Exception exception) : this(exception.Message, exception)
        {
        }

        public ComponentErrorEventArgs(string message, Exception exception)
        {
            this.Message = message;
            this.Exception = exception;
        }

        public string Message { get; private set; }

        public Exception Exception { get; private set; }
    }
}
