using System;
using System.Collections.Generic;

namespace FoxTunes
{
    public static class BassEncoderSettingsFactoryConfiguration
    {
        public const string SECTION = "D1D2586A-8FE3-4F1E-A5A2-58C57A99FB11";

        public const string FORMAT_ELEMENT = "AAAA7464-2172-4E22-A3F6-817888878A62";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION, "Converter")
                .WithElement(new SelectionConfigurationElement(FORMAT_ELEMENT, "Format")
                    .WithOptions(GetFormatOptions())
            );
        }

        private static IEnumerable<SelectionConfigurationOption> GetFormatOptions()
        {
            yield return new SelectionConfigurationOption(FlacEncoderSettings.NAME, FlacEncoderSettings.NAME);
        }

        public static IBassEncoderSettings GetFormat(SelectionConfigurationElement option)
        {
            switch (option.Id)
            {
                case FlacEncoderSettings.NAME:
                    return new FlacEncoderSettings();
            }
            throw new NotImplementedException();
        }
    }
}
