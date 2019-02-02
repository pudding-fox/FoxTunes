using System.Collections.Generic;

namespace FoxTunes
{
    public static class MiniPlayerBehaviourConfiguration
    {
        public const string SECTION = "F3E58830-97C0-4BA2-9E07-3EC27E3D4418";

        public const string ENABLED_ELEMENT = "AAAAF9DF-2F0C-4F6B-BC9F-0BFC142EBD57";

        public const string NOW_PLAYING_SCRIPT_ELEMENT = "BBBB78F3-B32F-4A8C-B566-9A8B39A896C7";

        public const string TOPMOST_ELEMENT = "CCCC7F71-A506-48DF-9420-E6926465FFDC";

        public const string RESET_PLAYLIST_ELEMENT = "DDDDC813-182A-42E1-A905-F2A809AC5EE3";

        public const string AUTO_PLAY_ELEMENT = "EEEEEF66-531B-44D3-8F8D-FD9AA5A49605";

        public const string SHOW_ARTWORK_ELEMENT = "FFFFB3A4-E59D-46CE-B80A-86EAB9427108";

        public const string SHOW_PLAYLIST_ELEMENT = "GGGG60A9-B108-49DD-9D9D-EA608BBDB0E7";

        public const string PLAYLIST_SCRIPT_ELEMENT = "HHHHD917-2172-421D-9E22-F549B17CE0C8";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION, "Mini Player")
                .WithElement(
                    new BooleanConfigurationElement(ENABLED_ELEMENT, "Enabled").WithValue(false).Hide())
                .WithElement(
                    new TextConfigurationElement(NOW_PLAYING_SCRIPT_ELEMENT, "Playing Script", path: "Advanced").WithValue(Resources.NowPlaying).WithFlags(ConfigurationElementFlags.MultiLine))
                .WithElement(
                    new TextConfigurationElement(PLAYLIST_SCRIPT_ELEMENT, "Playlist Script", path: "Advanced").WithValue(Resources.Playlist).WithFlags(ConfigurationElementFlags.MultiLine))
                .WithElement(
                    new BooleanConfigurationElement(TOPMOST_ELEMENT, "Always On Top").WithValue(false))
                .WithElement(
                    new BooleanConfigurationElement(RESET_PLAYLIST_ELEMENT, "Reset Playlist").WithValue(true))
                .WithElement(
                    new BooleanConfigurationElement(AUTO_PLAY_ELEMENT, "Auto Play").WithValue(true))
                .WithElement(
                    new BooleanConfigurationElement(SHOW_ARTWORK_ELEMENT, "Show Artwork").WithValue(true))
                .WithElement(
                    new BooleanConfigurationElement(SHOW_PLAYLIST_ELEMENT, "Show Playlist").WithValue(false)
            );
        }
    }
}
