using FoxTunes.Interfaces;
using System.Collections.Generic;

namespace FoxTunes
{
    public class BassEncoderSettingsFactory : StandardComponent, IConfigurableComponent
    {
        public IConfiguration Configuration { get; private set; }

        public SelectionConfigurationElement Format { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Configuration = core.Components.Configuration;
            this.Format = this.Configuration.GetElement<SelectionConfigurationElement>(
                BassEncoderSettingsFactoryConfiguration.SECTION,
                BassEncoderSettingsFactoryConfiguration.FORMAT_ELEMENT
            );
            base.InitializeComponent(core);
        }

        public IBassEncoderSettings CreateSettings()
        {
            var format = BassEncoderSettingsFactoryConfiguration.GetFormat(this.Format);
            return format;
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return BassEncoderSettingsFactoryConfiguration.GetConfigurationSections();
        }
    }
}
