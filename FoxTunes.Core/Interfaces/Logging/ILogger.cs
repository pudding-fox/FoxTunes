using System;

namespace FoxTunes.Interfaces
{
    public interface ILogger : IStandardComponent
    {
        bool IsDebugEnabled(IBaseComponent component);

        bool IsErrorEnabled(IBaseComponent component);

        bool IsFatalEnabled(IBaseComponent component);

        bool IsInfoEnabled(IBaseComponent component);

        bool IsWarnEnabled(IBaseComponent component);

        bool IsDebugEnabled(Type type);

        bool IsErrorEnabled(Type type);

        bool IsFatalEnabled(Type type);

        bool IsInfoEnabled(Type type);

        bool IsWarnEnabled(Type type);

        void Write(IBaseComponent component, LogLevel level, string message, params object[] args);

        void Write(Type type, LogLevel level, string message, params object[] args);
    }

    public enum LogLevel : byte
    {
        Debug,
        Error,
        Fatal,
        Info,
        Warn
    }
}
