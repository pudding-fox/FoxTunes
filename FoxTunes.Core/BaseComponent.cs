using FoxTunes.Interfaces;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace FoxTunes
{
    [Serializable]
    public abstract class BaseComponent : IBaseComponent
    {
        protected static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        public virtual void InitializeComponent(ICore core)
        {
            //Nothing to do.
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged == null)
            {
                return;
            }
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected virtual Task OnError(string message)
        {
            return this.OnError(new Exception(message));
        }

        protected virtual Task OnError(Exception exception)
        {
            return this.OnError(exception.Message, exception);
        }

        protected virtual Task OnError(string message, Exception exception)
        {
            Logger.Write(this, LogLevel.Error, message, exception);
            if (this.Error == null)
            {
                return Task.CompletedTask;
            }
            return this.Error(this, new ComponentOutputErrorEventArgs(message, exception));
        }

        [field: NonSerialized]
        public event ComponentOutputErrorEventHandler Error;
    }
}
