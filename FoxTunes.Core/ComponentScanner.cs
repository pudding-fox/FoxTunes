using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FoxTunes
{
    public class ComponentScanner : BaseComponent, IComponentScanner
    {
        const FileSystemHelper.SearchOption SEARCH_OPTIONS =
            FileSystemHelper.SearchOption.Recursive |
            FileSystemHelper.SearchOption.UseSystemCache |
            FileSystemHelper.SearchOption.UseSystemExclusions;

        private ComponentScanner()
        {
            this._FileNames = new Lazy<IEnumerable<string>>(() =>
            {
                return Enumerable.Concat(
                    FileSystemHelper.EnumerateFiles(this.Location, "FoxTunes*.dll", SEARCH_OPTIONS),
                    FileSystemHelper.EnumerateFiles(this.Location, "FoxTunes*.exe", SEARCH_OPTIONS)
                ).ToArray();
            });
        }

        public string Location
        {
            get
            {
                return Path.GetDirectoryName(typeof(ComponentLoader).Assembly.Location);
            }
        }

        private Lazy<IEnumerable<string>> _FileNames { get; set; }

        public IEnumerable<string> FileNames
        {
            get
            {
                return this._FileNames.Value;
            }
        }

        public IEnumerable<Type> GetComponents(Type interfaceType)
        {
            Logger.Write(this, LogLevel.Debug, "Scanning for components of type: {0}", interfaceType.Name);
            foreach (var fileName in this.FileNames)
            {
                var assemblyName = AssemblyRegistry.Instance.GetAssemblyName(fileName);
                if (assemblyName == null)
                {
                    continue;
                }
                var assembly = default(Assembly);
                try
                {
                    assembly = AssemblyRegistry.Instance.GetOrLoadReflectionAssembly(fileName);
                }
                catch (Exception e)
                {
                    Logger.Write(this, LogLevel.Warn, "Failed to load assembly {0}: {1}", fileName, e.Message);
                    continue;
                }
                var types = default(IEnumerable<Type>);
                try
                {
                    types = assembly.GetExportedTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    Logger.Write(this, LogLevel.Trace, "Error was handled while getting exported types for assembly {0}: {1}", fileName, e.Message);
                    types = e.Types;
                }
                catch (Exception e)
                {
                    Logger.Write(this, LogLevel.Warn, "Failed to get exported types for assembly {0}: {1}", fileName, e.Message);
                    continue;
                }
                foreach (var type in types)
                {
                    if (!type.IsClass || type.IsAbstract || !this.ImplementsInterface(type, interfaceType.FullName))
                    {
                        continue;
                    }
                    Logger.Write(this, LogLevel.Debug, "Scanning component of type {0} in assembly {1}: {2}", interfaceType.Name, fileName, type.Name);
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
