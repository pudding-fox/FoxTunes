using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace FoxTunes
{
    public class AssemblyResolver : IAssemblyResolver
    {
        private AssemblyResolver()
        {

        }

        public void Enable()
        {
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += this.OnReflectionOnlyAssemblyResolve;
        }

        public void Disable()
        {
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= this.OnReflectionOnlyAssemblyResolve;
        }

        public string Resolve(string name)
        {
            foreach (var fileName in ComponentScanner.Instance.FileNames)
            {
                var assemblyName = default(AssemblyName);
                try
                {
                    assemblyName = AssemblyName.GetAssemblyName(fileName);
                }
                catch
                {
                    continue;
                }
                if (!string.Equals(assemblyName.FullName, name, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                return fileName;
            }
            throw new AssemblyResolverException(string.Format("Failed to resolve assembly {0}.", name));
        }

        protected virtual Assembly OnReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                return Assembly.ReflectionOnlyLoad(args.Name);
            }
            catch
            {
                //Nothing to do.
            }
            var fileName = this.Resolve(args.Name);
            var assembly = AssemblyRegistry.Instance.GetOrLoadReflectionAssembly(fileName);
            foreach (var reference in assembly.GetReferencedAssemblies())
            {
                try
                {
                    Assembly.ReflectionOnlyLoad(reference.FullName);
                }
                catch
                {
                    //Nothing to do.
                }
            }
            return assembly;
        }

        public static readonly IAssemblyResolver Instance = new AssemblyResolver();
    }

    public class AssemblyResolverException : Exception
    {
        public AssemblyResolverException(string message)
            : base(message)
        {

        }
    }
}
