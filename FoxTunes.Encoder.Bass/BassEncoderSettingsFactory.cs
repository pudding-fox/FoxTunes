using FoxTunes.Interfaces;
using System.Collections.Generic;

namespace FoxTunes
{
    public class BassEncoderSettingsFactory : StandardComponent
    {
        public static IEnumerable<IBassEncoderSettings> Profiles
        {
            get
            {
                yield return new AppleLosslessEncoderSettings();
                yield return new FlacEncoderSettings();
                yield return new WavPackEncoderSettings();
            }
        }

        public ICore Core { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public SelectionConfigurationElement Format { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.Configuration = core.Components.Configuration;
            this.Format = this.Configuration.GetElement<SelectionConfigurationElement>(
                BassEncoderBehaviourConfiguration.SECTION,
                BassEncoderBehaviourConfiguration.FORMAT_ELEMENT
            );
            base.InitializeComponent(core);
        }

        public IBassEncoderSettings CreateSettings()
        {
            Logger.Write(this, LogLevel.Debug, "Creating settings for profile: {0}", this.Format.Value.Name);
            var format = BassEncoderBehaviourConfiguration.GetFormat(this.Format.Value);
            format.InitializeComponent(this.Core);
            return format;
        }
    }
}
