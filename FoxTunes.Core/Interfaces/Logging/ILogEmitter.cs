using System;

namespace FoxTunes.Interfaces
{
    public interface ILogEmitter : IStandardComponent
    {
        event LogMessageEventHandler LogMessage;
    }

    public delegate void LogMessageEventHandler(object sender, LogMessageEventArgs e);

    public class LogMessageEventArgs : EventArgs
    {
        public LogMessageEventArgs(LogMessage logMessage)
        {
            this.LogMessage = logMessage;
        }

        public LogMessage LogMessage { get; private set; }
    }

    public class LogMessage
    {
        public LogMessage(string name, LogLevel level, string message)
        {
            this.Name = name;
            this.Level = level;
            this.Message = message;
        }

        public string Name { get; private set; }

        public LogLevel Level { get; private set; }

        public string Message { get; private set; }
    }
}
