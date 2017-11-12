using System;

namespace FoxTunes.Interfaces
{
    public interface IBaseComponent : IObservable
    {
        void InitializeComponent(ICore core);

        event ComponentOutputErrorEventHandler Error;
    }

    public delegate void ComponentOutputErrorEventHandler(object sender, ComponentOutputErrorEventArgs e);

    public class ComponentOutputErrorEventArgs : EventArgs
    {
        public ComponentOutputErrorEventArgs(Exception exception) : this(exception.Message, exception)
        {
        }

        public ComponentOutputErrorEventArgs(string message, Exception exception)
        {
            this.Message = message;
            this.Exception = exception;
        }

        public string Message { get; private set; }

        public Exception Exception { get; private set; }
    }
}
