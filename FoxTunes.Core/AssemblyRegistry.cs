using FoxTunes.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace FoxTunes
{
    public class AssemblyRegistry : IAssemblyRegistry
    {
        private AssemblyRegistry()
        {
            this._ReflectionAssemblies = new ConcurrentDictionary<string, Assembly>();
            this._ExecutableAssemblies = new ConcurrentDictionary<string, Assembly>();
        }

        private ConcurrentDictionary<string, Assembly> _ReflectionAssemblies { get; set; }

        public IEnumerable<Assembly> ReflectionAssemblies
        {
            get
            {
                return this._ReflectionAssemblies.Values;
            }
        }

        private ConcurrentDictionary<string, Assembly> _ExecutableAssemblies { get; set; }

        public IEnumerable<Assembly> ExecutableAssemblies
        {
            get
            {
                return this._ExecutableAssemblies.Values;
            }
        }

        public Assembly GetOrLoadReflectionAssembly(string fileName)
        {
            return this.GetOrLoadAssembly(this._ReflectionAssemblies, fileName, () => Assembly.ReflectionOnlyLoadFrom(fileName));
        }

        public Assembly GetOrLoadExecutableAssembly(string fileName)
        {
            return this.GetOrLoadAssembly(this._ExecutableAssemblies, fileName, () => Assembly.LoadFrom(fileName));
        }

        private Assembly GetOrLoadAssembly(ConcurrentDictionary<string, Assembly> assemblies, string fileName, Func<Assembly> loader)
        {
            var assembly = default(Assembly);
            if (assemblies.TryGetValue(fileName, out assembly))
            {
                return assembly;
            }
            assembly = loader();
            if (!assemblies.TryAdd(fileName, assembly))
            {
                throw new AssemblyRegistryException(string.Format("Failed to cache assembly {0}.", fileName));
            }
            return assembly;
        }

        public Type GetReflectionType(Type type)
        {
            return this.GetOrLoadReflectionAssembly(type.Assembly.Location).GetType(type.FullName);
        }

        public Type GetExecutableType(Type type)
        {
            return this.GetOrLoadExecutableAssembly(type.Assembly.Location).GetType(type.FullName);
        }

        public static readonly IAssemblyRegistry Instance = new AssemblyRegistry();
    }

    public class AssemblyRegistryException : Exception
    {
        public AssemblyRegistryException(string message)
            : base(message)
        {

        }
    }
}
