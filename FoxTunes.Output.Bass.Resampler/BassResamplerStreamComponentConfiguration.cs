using System.Collections.Generic;

namespace FoxTunes
{
    public static class BassResamplerStreamComponentConfiguration
    {
        public const string RESAMPLER_ELEMENT = "AAAA5C85-178C-470D-A977-C54350875AB3";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(BassOutputConfiguration.SECTION, "Output")
                .WithElement(new BooleanConfigurationElement(RESAMPLER_ELEMENT, "High Quality Resampler", path: "Advanced").WithValue(false)
            );
        }
    }
}
