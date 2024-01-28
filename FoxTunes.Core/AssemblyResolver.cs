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

        public void EnableExecution()
        {
            AppDomain.CurrentDomain.AssemblyResolve += this.OnAssemblyResolve;
        }

        public void EnableReflectionOnly()
        {
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += this.OnReflectionOnlyAssemblyResolve;
        }

        public void DisableExecution()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= this.OnAssemblyResolve;
        }

        public void DisableReflectionOnly()
        {
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= this.OnReflectionOnlyAssemblyResolve;
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

        protected virtual Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var fileName = default(string);
            if (this.TryResolve(ComponentScanner.Instance.Location, args.Name, false, out fileName))
            {
                return Assembly.LoadFrom(fileName);
            }
            return null;
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
            //If the requesting assembly can be determined then begin looking there.
            if (args.RequestingAssembly != null && File.Exists(args.RequestingAssembly.Location))
            {
                directoryName = Path.GetDirectoryName(args.RequestingAssembly.Location);
            }
            else
            {
                directoryName = ComponentScanner.Instance.Location;
            }
            var fileName = default(string);
            if (this.TryResolve(directoryName, args.Name, true, out fileName))
            {
                var assembly = AssemblyRegistry.Instance.GetOrLoadReflectionAssembly(fileName);
                //Load immidiate references.
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
            //Failed to resolve, perhaps somebody else will succeed.
            return null;
        }

        protected virtual bool TryResolve(string directoryName, string name, bool tryLoad, out string result)
        {
            foreach (var fileName in this.GetFiles(directoryName))
            {
                var assemblyName = AssemblyRegistry.Instance.GetAssemblyName(fileName);
                if (assemblyName == null)
                {
                    continue;
                }
                if (!string.Equals(assemblyName.FullName, name, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                result = fileName;
                return true;
            }
            if (tryLoad)
            {
                //I'm not sure why but some platforms end up here for framework assemblies
                //like System.Runtime. I'm not sure how else to get the location.
                try
                {
                    var assembly = Assembly.Load(name);
                    if (File.Exists(assembly.Location))
                    {
                        result = assembly.Location;
                        return true;
                    }
                }
                catch
                {
                    //Nothing to do.
                }
            }
            result = default(string);
            return false;
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
