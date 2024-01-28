using System.Collections.Generic;

namespace FoxTunes
{
    public static class BassResamplerStreamComponentConfiguration
    {
        public const string RESAMPLER_ELEMENT = "91D65C85-178C-470D-A977-C54350875AB3";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(BassOutputConfiguration.OUTPUT_SECTION, "Output")
                .WithElement(new BooleanConfigurationElement(RESAMPLER_ELEMENT, "High Quality Resampler").WithValue(false)
            );
        }
    }
}
