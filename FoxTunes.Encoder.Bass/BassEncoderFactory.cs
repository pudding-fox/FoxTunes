using System;
using System.Reflection;
using System.Security.Policy;

namespace FoxTunes
{
    public class BassEncoderFactory : StandardComponent, IBassEncoderFactory
    {
        public IBassEncoder CreateEncoder(int concurrency)
        {
            var domain = this.CreateDomain();
            var encoder = this.CreateEncoder(domain, concurrency);
            return encoder;
        }

        protected virtual AppDomain CreateDomain()
        {
            var name = string.Format("{0}_{1}", typeof(BassEncoderFactory), Guid.NewGuid().ToString("d"));
            var domain = AppDomain.CreateDomain(
                name,
                AppDomain.CurrentDomain.Evidence,
                AppDomain.CurrentDomain.SetupInformation
            );
            return domain;
        }

        protected virtual IBassEncoder CreateEncoder(AppDomain domain, int concurrency)
        {
            var type = typeof(BassEncoder);
            var handle = domain.CreateInstance(
                type.Assembly.FullName,
                type.FullName,
                true,
                BindingFlags.Default,
                null,
                new object[] { domain, concurrency },
                null,
                null
            );
            return (IBassEncoder)handle.Unwrap();
        }
    }
}
