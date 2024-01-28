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
            var success = true;
            var types = typeof(ICore).Assembly
                .GetExportedTypes()
                .Where(IsRequiredInterface);
            foreach (var type in types)
            {
                if (ComponentRegistry.Instance.GetComponent(type) == null)
                {
                    if (core.Components.UserInterface != null)
                    {
                        core.Components.UserInterface.Warn(string.Format("Component missing or invalid for interface \"{0}\".", type.FullName));
                    }
                    success = false;
                }
            }
            return success;
        }

        private static bool IsRequiredInterface(Type type)
        {
            return type.IsInterface && REQUIRED.Any(required => required != type && required.IsAssignableFrom(type));
        }

        public static readonly ICoreValidator Instance = new CoreValidator();
    }
}
