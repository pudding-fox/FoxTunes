using System.Collections.Generic;

namespace FoxTunes
{
    public static class LyricsBehaviourConfiguration
    {
        public const string SECTION = "42FB4DBD-E28C-4E42-B64F-6921CFCEF924";

        public const string AUTO_SCROLL = "AAAA70DD-84E9-48D5-B174-E0A0FC498698";

        public const string AUTO_SCROLL_EASING = "BBBBCC1E-B4A8-41A9-A611-47F06EFFF24B";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION, "Lyrics")
                .WithElement(new BooleanConfigurationElement(AUTO_SCROLL, "Auto Scroll").WithValue(true).DependsOn(MetaDataBehaviourConfiguration.SECTION, MetaDataBehaviourConfiguration.ENABLE_ELEMENT))
                .WithElement(new BooleanConfigurationElement(AUTO_SCROLL_EASING, "Auto Scroll Easing").WithValue(false).DependsOn(SECTION, AUTO_SCROLL)
            );
        }
    }
}
