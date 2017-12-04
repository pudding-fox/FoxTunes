using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FoxTunes
{
    public class ComponentScanner : IComponentScanner
    {
        private ComponentScanner()
        {

        }

        public string Location
        {
            get
            {
                return Path.GetDirectoryName(typeof(ComponentLoader).Assembly.Location);
            }
        }

        public IEnumerable<string> FileNames
        {
            get
            {
                return Directory.GetFiles(this.Location, "FoxTunes*.dll");
            }
        }

        public IEnumerable<Type> GetComponents(Type interfaceType)
        {
            foreach (var fileName in this.FileNames)
            {
                var assembly = default(Assembly);
                try
                {
                    assembly = AssemblyRegistry.Instance.GetOrLoadReflectionAssembly(fileName);
                }
                catch
                {
                    continue;
                }
                var types = default(IEnumerable<Type>);
                try
                {
                    types = assembly.GetExportedTypes();
                }
                catch
                {
                    continue;
                }
                foreach (var type in types)
                {
                    if (!type.IsClass || type.IsAbstract)
                    {
                        continue;
                    }
                    if (!this.ImplementsInterface(type, interfaceType.FullName))
                    {
                        continue;
                    }
                    yield return type;
                }
            }
        }

        private bool ImplementsInterface(Type type, string interfaceTypeName)
        {
            return type.GetInterfaces().Any(interfaceType => string.Equals(interfaceType.FullName, interfaceTypeName, StringComparison.OrdinalIgnoreCase));
        }

        public static readonly IComponentScanner Instance = new ComponentScanner();
    }
}
