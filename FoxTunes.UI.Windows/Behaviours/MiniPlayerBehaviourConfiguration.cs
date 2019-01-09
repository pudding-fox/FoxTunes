using FoxTunes.Properties;
using System.Collections.Generic;

namespace FoxTunes
{
    public static class MiniPlayerBehaviourConfiguration
    {
        public const string MINI_PLAYER_SECTION = "F3E58830-97C0-4BA2-9E07-3EC27E3D4418";

        public const string ENABLED_ELEMENT = "19BBF9DF-2F0C-4F6B-BC9F-0BFC142EBD57";

        public const string NOW_PLAYING_SCRIPT_ELEMENT = "958A78F3-B32F-4A8C-B566-9A8B39A896C7";

        public const string TOPMOST_ELEMENT = "D7E87F71-A506-48DF-9420-E6926465FFDC";

        public const string RESET_PLAYLIST_ELEMENT = "ED46C813-182A-42E1-A905-F2A809AC5EE3";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(MINI_PLAYER_SECTION, "Mini Player")
                .WithElement(
                    new BooleanConfigurationElement(ENABLED_ELEMENT, "Enabled").WithValue(false))
                .WithElement(
                    new TextConfigurationElement(NOW_PLAYING_SCRIPT_ELEMENT, "Now Playing Script").WithValue(Resources.NowPlaying).WithFlags(ConfigurationElementFlags.MultiLine))
                .WithElement(
                    new BooleanConfigurationElement(TOPMOST_ELEMENT, "Always On Top").WithValue(false))
                .WithElement(
                    new BooleanConfigurationElement(RESET_PLAYLIST_ELEMENT, "Reset Playlist").WithValue(false)
            );
        }
    }
}
