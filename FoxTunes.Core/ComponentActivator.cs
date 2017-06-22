using FoxTunes.Interfaces;
using System;

namespace FoxTunes
{
    public class ComponentActivator : IComponentActivator
    {
        private ComponentActivator()
        {

        }

        public T Activate<T>(Type type) where T : IBaseComponent
        {
            if (type.Assembly.ReflectionOnly)
            {
                type = AssemblyRegistry.Instance.GetExecutableType(type);
            }
            if (type.GetConstructor(new Type[] { }) != null)
            {
                return (T)Activator.CreateInstance(type);
            }
            throw new ComponentActivatorException(string.Format("Failed to locate constructor for component {0}.", type.Name));
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
