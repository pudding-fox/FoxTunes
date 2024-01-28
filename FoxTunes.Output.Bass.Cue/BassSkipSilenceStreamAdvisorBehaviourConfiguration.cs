using System.Collections.Generic;

namespace FoxTunes
{
    public static class BassSkipSilenceStreamAdvisorBehaviourConfiguration
    {
        public const string ENABLED_ELEMENT = "RRRR223E-4396-495B-8600-5130CCEB81E0";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(BassOutputConfiguration.SECTION, "Output")
                .WithElement(new BooleanConfigurationElement(ENABLED_ELEMENT, "Skip Silence", path: "Advanced").WithValue(false)
            );
        }
    }
}
