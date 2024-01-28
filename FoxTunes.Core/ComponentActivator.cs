using FoxDb;
using FoxTunes.Interfaces;
using System;

namespace FoxTunes
{
    public class ComponentActivator : IComponentActivator
    {
        protected static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        private ComponentActivator()
        {

        }

        public T Activate<T>(Type type) where T : IBaseComponent
        {
            if (type.Assembly.ReflectionOnly)
            {
                type = AssemblyRegistry.Instance.GetExecutableType(type);
            }
            if (type.GetConstructor(new Type[] { }) == null)
            {
                Logger.Write(typeof(ComponentActivator), LogLevel.Warn, "Failed to locate constructor for component {0}.", type.Name);
                return default(T);
            }
            if (FastActivator.Instance.Activate(type) is T component)
            {
                return component;
            }
            else
            {
                Logger.Write(typeof(ComponentActivator), LogLevel.Warn, "Component {0} is not of the expected type {1}.", type.Name, typeof(T).Name);
                return default(T);
            }
        }

        public static readonly IComponentActivator Instance = new ComponentActivator();
    }

    public class ComponentActivatorException : Exception
    {
        public ComponentActivatorException(string message)
            : base(message)
        {

        }
    }
}
