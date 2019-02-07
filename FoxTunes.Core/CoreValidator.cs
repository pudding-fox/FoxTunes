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
            var success = true;
            foreach (var key in ComponentSlots.Lookup.Keys)
            {
                var value = ComponentSlots.Lookup[key];
                if (ComponentRegistry.Instance.GetComponents(value).Count() > 1)
                {
                    ComponentResolver.Instance.Add(value);
                    if (core.Components.UserInterface != null)
                    {
                        core.Components.UserInterface.Warn(string.Format("Multiple components are installed for slot \"{0}\".\nEdit {1} to resolve the conflict.", key, ComponentResolver.FILE_NAME.GetName()));
                    }
                    success = false;
                }
            }
            if (!success)
            {
                return false;
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
