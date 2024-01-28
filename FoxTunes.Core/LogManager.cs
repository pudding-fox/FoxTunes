using FoxTunes.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FoxTunes
{
    public static class LogManager
    {
        public static string FileName = Path.Combine(
            Publication.StoragePath,
            "Log.txt"
        );

        private static ILogger _Logger { get; set; }

        public static ILogger Logger
        {
            get
            {
                if (_Logger == null)
                {
                    return NullLogger.Instance;
                }
                return _Logger;
            }
            set
            {
                _Logger = value;
            }
        }

        private class NullLogger : BaseComponent, ILogger
        {
            public bool IsTraceEnabled(IBaseComponent component)
            {
                return false;
            }

            public bool IsDebugEnabled(IBaseComponent component)
            {
                return false;
            }

            public bool IsInfoEnabled(IBaseComponent component)
            {
                return false;
            }

            public bool IsWarnEnabled(IBaseComponent component)
            {
                return false;
            }

            public bool IsErrorEnabled(IBaseComponent component)
            {
                return false;
            }

            public bool IsFatalEnabled(IBaseComponent component)
            {
                return false;
            }

            public bool IsTraceEnabled(Type type)
            {
                return false;
            }

            public bool IsDebugEnabled(Type type)
            {
                return false;
            }

            public bool IsInfoEnabled(Type type)
            {
                return false;
            }

            public bool IsWarnEnabled(Type type)
            {
                return false;
            }

            public bool IsErrorEnabled(Type type)
            {
                return false;
            }

            public bool IsFatalEnabled(Type type)
            {
                return false;
            }

            public void Write(IBaseComponent component, LogLevel level, string message, params object[] args)
            {
                //Nothing to do.
            }

            public void Write(Type type, LogLevel level, string message, params object[] args)
            {
                //Nothing to do.
            }

            public Task WriteAsync(IBaseComponent component, LogLevel level, string message, params object[] args)
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }

            public Task WriteAsync(Type type, LogLevel level, string message, params object[] args)
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }

            public void Dispose()
            {
                //Nothing to do.
            }

            public static readonly ILogger Instance = new NullLogger();
        }
    }
}
