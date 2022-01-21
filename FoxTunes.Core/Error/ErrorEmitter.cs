using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class ErrorEmitter : StandardComponent, IErrorEmitter
    {
        public Task Send(string message)
        {
            return this.Send(new Exception(message));
        }

        public Task Send(Exception exception)
        {
            return this.Send(exception.Message, exception);
        }

        public Task Send(string message, Exception exception)
        {
            return this.OnError(this, new ComponentErrorEventArgs(message, exception));
        }

        protected virtual Task OnError(object sender, ComponentErrorEventArgs e)
        {
            Logger.Write(this, LogLevel.Error, e.Message);
            if (this.Error == null)
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            return this.Error(sender, e);
        }

        public event ComponentErrorEventHandler Error;
    }
}
