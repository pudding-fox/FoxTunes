using System.Collections.Generic;

namespace FoxTunes
{
    public static class PlaylistBehaviourConfiguration
    {
        public const string SECTION = "11FAE8A9-8DF4-4DD5-B0C7-DFFCBABDC04A";

        public const string SHUFFLE_ELEMENT = "E666183E-486E-45B4-A7CB-CE225AB89A1F";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION, "Playlist")
                .WithElement(
                    new BooleanConfigurationElement(SHUFFLE_ELEMENT, "Shuffle").WithValue(false)
            );
        }
    }
}
