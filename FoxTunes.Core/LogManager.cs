using FoxTunes.Interfaces;
using System;

namespace FoxTunes
{
    public static class LogManager
    {
        public static ILogger Logger
        {
            get
            {
                return ComponentRegistry.Instance.GetComponent<ILogger>() ?? new NullLogger();
            }
        }

        private class NullLogger : BaseComponent, ILogger
        {
            public bool IsDebugEnabled(IBaseComponent component)
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

            public bool IsInfoEnabled(IBaseComponent component)
            {
                return false;
            }

            public bool IsWarnEnabled(IBaseComponent component)
            {
                return false;
            }

            public bool IsDebugEnabled(Type type)
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

            public bool IsInfoEnabled(Type type)
            {
                return false;
            }

            public bool IsWarnEnabled(Type type)
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
        }
    }
}
