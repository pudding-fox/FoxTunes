using System.Collections.Generic;

namespace FoxTunes
{
    public static class BassOutputTempoStreamComponentBehaviourConfiguration
    {
        public const string ENABLED = "";

        public const string TEMPO = "";

        public const string PITCH = "";

        public const string RATE = "";

        public const string AA_FILTER = "";

        public const string AA_FILTER_LENGTH = "";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(BassOutputConfiguration.SECTION, "Output")
                .WithElement(
                    new BooleanConfigurationElement(ENABLED, "Enabled", path: "Tempo")
                        .WithValue(false))
                .WithElement(
                    new DoubleConfigurationElement(TEMPO, "Tempo", path: "Tempo")
                        .WithValue(0.0)
                        .DependsOn(BassOutputConfiguration.SECTION, ENABLED))
                .WithElement(
                    new DoubleConfigurationElement(PITCH, "Pitch", path: "Tempo")
                        .WithValue(0.0)
                        .DependsOn(BassOutputConfiguration.SECTION, ENABLED))
                .WithElement(
                    new DoubleConfigurationElement(RATE, "Rate", path: "Tempo")
                        .WithValue(0.0)
                        .DependsOn(BassOutputConfiguration.SECTION, ENABLED))
                .WithElement(
                    new BooleanConfigurationElement(AA_FILTER, "Anti Alias Filter", path: "Tempo")
                        .WithValue(true)
                        .DependsOn(BassOutputConfiguration.SECTION, ENABLED))
                .WithElement(
                    new IntegerConfigurationElement(AA_FILTER_LENGTH, "Anti Alias Filter Length", path: "Tempo")
                        .WithValue(32)
                        .DependsOn(BassOutputConfiguration.SECTION, ENABLED)
                        .DependsOn(BassOutputConfiguration.SECTION, AA_FILTER));
        }
    }
}
