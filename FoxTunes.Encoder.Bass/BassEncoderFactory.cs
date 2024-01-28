using FoxTunes.Interfaces;
using System;
using System.Reflection;

namespace FoxTunes
{
    public class BassEncoderFactory : StandardComponent, IBassEncoderFactory
    {
        public IBassEncoder CreateEncoder()
        {
            var domain = this.CreateDomain();
            var encoder = this.CreateEncoder(domain);
            return encoder;
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
            Logger.Write(this, LogLevel.Debug, "App domain created: {0}", name);
            return domain;
        }

        protected virtual IBassEncoder CreateEncoder(AppDomain domain)
        {
            var type = typeof(BassEncoder);
            var handle = domain.CreateInstance(
                type.Assembly.FullName,
                type.FullName,
                true,
                BindingFlags.Default,
                null,
                new object[] { domain },
                null,
                null
            );
            var encoder = (IBassEncoder)handle.Unwrap();
            Logger.Write(this, LogLevel.Debug, "Created and unwrapped type \"{0}\" from app domain \"{1}\".", type.Name, domain.FriendlyName);
            return encoder;
        }
    }
}
