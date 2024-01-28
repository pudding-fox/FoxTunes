using System.Collections.Generic;

namespace FoxTunes
{
    public static class PlaylistTracksLibraryBehaviourConfiguration
    {
        public const string ENABLED_ELEMENT = "AAAA99C9-30D7-4E14-A3B5-91D88087ECB8";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(PlaylistBehaviourConfiguration.SECTION, "Playlist")
                .WithElement(
                    new BooleanConfigurationElement(ENABLED_ELEMENT, "Follow Library Selection").WithValue(false)
            );
        }
    }
}
