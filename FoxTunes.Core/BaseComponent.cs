using FoxTunes.Interfaces;
using System;
using System.ComponentModel;

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

        protected virtual void OnError(string message)
        {
            this.OnError(new Exception(message));
        }

        protected virtual void OnError(Exception exception)
        {
            this.OnError(exception.Message, exception);
        }

        protected virtual void OnError(string message, Exception exception)
        {
            Logger.Write(this, LogLevel.Error, message, exception);
            if (this.Error == null)
            {
                return;
            }
            this.Error(this, new ComponentOutputErrorEventArgs(message, exception));
        }

        [field: NonSerialized]
        public event ComponentOutputErrorEventHandler Error = delegate { };
    }
}
