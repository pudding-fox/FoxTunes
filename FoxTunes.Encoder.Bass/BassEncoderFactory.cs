using FoxTunes.Interfaces;
using System;
using System.IO;
using System.Reflection;

namespace FoxTunes
{
    public class BassEncoderFactory : StandardComponent, IBassEncoderFactory
    {
        public string Location
        {
            get
            {
                return typeof(BassEncoderFactory).Assembly.Location;
            }
        }

        public IBassEncoder CreateEncoder()
        {
            var domain = this.CreateDomain();
            try
            {
                var encoder = this.CreateEncoder(domain);
                return encoder;
            }
            catch
            {
                AppDomain.Unload(domain);
                throw;
            }
        }

        protected virtual AppDomain CreateDomain()
        {
            Logger.Write(this, LogLevel.Debug, "Creating app domain.");
            var name = string.Format("{0}_{1}", typeof(BassEncoderFactory), Guid.NewGuid().ToString("d"));
            var domain = AppDomain.CreateDomain(
                name,
                AppDomain.CurrentDomain.Evidence,
                AppDomain.CurrentDomain.SetupInformation
            );
#pragma warning disable 612, 618
            domain.AppendPrivatePath(Path.GetDirectoryName(this.Location));
#pragma warning restore 612, 618
            Logger.Write(this, LogLevel.Debug, "App domain created: {0}", name);
            return domain;
        }

        protected virtual IBassEncoder CreateEncoder(AppDomain domain)
        {
            Logger.Write(this, LogLevel.Debug, "Creating encoder.");
            var name = AssemblyName.GetAssemblyName(this.Location);
            var handler = new ResolveEventHandler((sender, e) =>
            {
                if (string.Equals(name.FullName, e.Name, StringComparison.OrdinalIgnoreCase))
                {
                    Logger.Write(this, LogLevel.Debug, "Handing assembly resolution: \"{0}\" => \"{1}\"", e.Name, this.Location);
                    return Assembly.LoadFrom(this.Location);
                }
                return null;
            });
            //I don't know why we have to handle AssemblyResolve to the current assembly.
            //There is only one copy and it's already loaded on both the main and encoder AppDomain.
            //We get an InvalidCastException otherwise.
            AppDomain.CurrentDomain.AssemblyResolve += handler;
            try
            {
                var encoder = ActivationProxy
                    .CreateProxy(domain)
                    .CreateInstance<BassEncoder>(this.Location, domain);
                Logger.Write(this, LogLevel.Debug, "Encoder created.");
                return encoder;
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -= handler;
            }
        }
    }
}
