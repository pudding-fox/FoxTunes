using System.Collections.Generic;

namespace FoxTunes
{
    public static class DiscogsBehaviourConfiguration
    {
        public const string SECTION = "D41A188F-119B-4CBF-AF10-522A7AC77CAE";

        public const string ENABLED = "AAAA76ED-AC82-4C9F-8FCE-096E3ABC2A47";

        public const string BASE_URL = "BBBB3214-B1DC-4BAB-9435-311C005998EA";

        public const string CONSUMER_KEY = "CCCC15BB-F99D-4B1A-9433-EDC634C94ABF";

        public const string CONSUMER_SECRET = "DDDD019F-AC8C-4C17-9127-0ADB9FE86FD0";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION, Strings.DiscogsBehaviourConfiguration_Section)
                .WithElement(new BooleanConfigurationElement(ENABLED, Strings.DiscogsBehaviourConfiguration_Enabled)
                    .WithValue(false))
                .WithElement(new TextConfigurationElement(BASE_URL, Strings.DiscogsBehaviourConfiguration_BaseUrl)
                    .WithValue(Discogs.BASE_URL)
                    .DependsOn(SECTION, ENABLED))
                .WithElement(new TextConfigurationElement(CONSUMER_KEY, Strings.DiscogsBehaviourConfiguration_ConsumerKey)
                    .WithValue(Discogs.KEY)
                    .DependsOn(SECTION, ENABLED)
                    .WithFlags(ConfigurationElementFlags.Secret))
                .WithElement(new TextConfigurationElement(CONSUMER_SECRET, Strings.DiscogsBehaviourConfiguration_ConsumerSecret)
                    .WithValue(Discogs.SECRET)
                    .DependsOn(SECTION, ENABLED)
                    .WithFlags(ConfigurationElementFlags.Secret));
        }
    }
}
