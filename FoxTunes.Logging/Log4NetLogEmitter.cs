using FoxTunes.Interfaces;
using log4net.Appender;
using log4net.Core;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class Log4NetLogEmitter : AppenderSkeleton, ILogEmitter
    {
        public Log4NetLogEmitter()
        {
            this.Enabled = true;
        }

        public bool Enabled { get; set; }

        public IForegroundTaskRunner ForegroundTaskRunner { get; private set; }

        protected override void Append(LoggingEvent loggingEvent)
        {
            if (!this.Enabled)
            {
                return;
            }
            this.OnLogMessage(loggingEvent);
        }

        protected virtual void OnLogMessage(LoggingEvent loggingEvent)
        {
            this.OnLogMessage(loggingEvent.LoggerName, GetLevel(loggingEvent.Level), loggingEvent.RenderedMessage);
        }

        protected virtual void OnLogMessage(string name, LogLevel level, string message)
        {
            if (this.LogMessage == null)
            {
                return;
            }
            this.ForegroundTaskRunner.RunAsync(() => this.LogMessage(this, new LogMessageEventArgs(new LogMessage(name, level, message))));
        }

        public event LogMessageEventHandler LogMessage = delegate { };

        private static LogLevel GetLevel(Level level)
        {
            return (LogLevel)Enum.Parse(typeof(LogLevel), level.Name, true);
        }

        #region IStandardComponent

        public void InitializeComponent(ICore core)
        {
            this.ForegroundTaskRunner = core.Components.ForegroundTaskRunner;
        }

        public void Interlocked(Action action)
        {
            throw new NotImplementedException();
        }

        public Task Interlocked(Func<Task> func)
        {
            throw new NotImplementedException();
        }

        public T Interlocked<T>(Func<T> func)
        {
            throw new NotImplementedException();
        }

        public Task<T> Interlocked<T>(Func<Task<T>> func)
        {
            throw new NotImplementedException();
        }

        public void Interlocked(Action action, TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        public Task Interlocked(Func<Task> func, TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        public Task<T> Interlocked<T>(Func<Task<T>> func, TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged == null)
            {
                return;
            }
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        #endregion
    }
}
