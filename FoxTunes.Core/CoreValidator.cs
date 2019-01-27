using FoxTunes.Interfaces;
using System;
using System.Linq;

namespace FoxTunes
{
    public class CoreValidator : BaseComponent, ICoreValidator
    {
        public static Type[] REQUIRED = new[]
        {
            typeof(IStandardComponent),
            typeof(IStandardManager),
            typeof(IStandardBehaviour),
            typeof(IStandardFactory)
        };

        public bool Validate(ICore core)
        {
            var types = typeof(ICore).Assembly.GetExportedTypes().Where(IsRequiredInterface);
            foreach (var type in types)
            {
                if (ComponentRegistry.Instance.GetComponent(type) == null)
                {
                    Logger.Write(this, LogLevel.Error, "Required component \"{0}\" was not found.", type.Name);
                    return false;
                }
            }
            return true;
        }

        private static bool IsRequiredInterface(Type type)
        {
            return type.IsInterface && REQUIRED.Any(required => required != type && required.IsAssignableFrom(type));
        }

        public static readonly ICoreValidator Instance = new CoreValidator();
    }
}
