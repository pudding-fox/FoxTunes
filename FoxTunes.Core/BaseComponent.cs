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

        public bool IsInitialized { get; private set; }

        public virtual void InitializeComponent(ICore core)
        {
            this.IsInitialized = true;
        }

        protected virtual void OnPropertyChanging(string propertyName)
        {
            if (this.PropertyChanging == null)
            {
                return;
            }
            this.PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
        }

        [field: NonSerialized]
        public event PropertyChangingEventHandler PropertyChanging = delegate { };

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

        [field: NonSerialized]
        public event ComponentErrorEventHandler Error;
    }
}
