using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
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

        public string Resolve(string directoryName, string name)
        {
            foreach (var fileName in this.GetFiles(directoryName))
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
            //I'm not sure why but some platforms end up here for framework assemblies
            //like System.Runtime. I'm not sure how else to get the location.
            try
            {
                var assembly = Assembly.Load(name);
                if (File.Exists(assembly.Location))
                {
                    return assembly.Location;
                }
            }
            catch
            {
                //Nothing to do.
            }
            throw new AssemblyResolverException(string.Format("Failed to resolve assembly {0}.", name));
        }

        public IEnumerable<string> GetFiles(string directoryName)
        {
            //Prefer assemblies in the base directory.
            foreach (var fileName in Directory.EnumerateFiles(ComponentScanner.Instance.Location, "*.dll"))
            {
                yield return fileName;
            }
            //If the requesting assembly is in another folder then check that too.
            if (Directory.Exists(directoryName) && !string.Equals(Path.GetFullPath(ComponentScanner.Instance.Location), Path.GetFullPath(directoryName)))
            {
                foreach (var fileName in Directory.EnumerateFiles(directoryName, "*.dll"))
                {
                    yield return fileName;
                }
            }
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
            var directoryName = default(string);
            if (File.Exists(args.RequestingAssembly.Location))
            {
                directoryName = Path.GetDirectoryName(args.RequestingAssembly.Location);
            }
            var fileName = this.Resolve(directoryName, args.Name);
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
