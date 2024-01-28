using System.Collections.Generic;

namespace FoxTunes
{
    public static class KeyBindingsBehaviourConfiguration
    {
        public const string PLAY_ELEMENT = "AAAA39A4-A260-4AA8-8E3F-E0ECD5C6727C";

        public const string PREVIOUS_ELEMENT = "BBBBC8F4-5F5B-4260-B7A5-F9829F4C8DF1";

        public const string NEXT_ELEMENT = "CCCCAF62-A5C5-4C6B-804F-47E086963B87";

        public const string STOP_ELEMENT = "DDDD3853-09F5-4DF7-9CDD-918B7B2A5F22";

        public const string SETTINGS_ELEMENT = "EEEE5C74-24D3-4C51-B50A-74EC4921FDC7";

        public const string MINI_PLAYER_ELEMENT = "FFFFDF70-E0DB-4154-9567-01AE394BA476";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(InputManagerConfiguration.SECTION, "Keyboard Shortcuts")
                .WithElement(
                    new TextConfigurationElement(PLAY_ELEMENT, "Play")
                        .WithValue("Alt+Space"))
                .WithElement(
                    new TextConfigurationElement(PREVIOUS_ELEMENT, "Previous")
                        .WithValue("Alt+Left"))
                 .WithElement(
                    new TextConfigurationElement(NEXT_ELEMENT, "Next")
                        .WithValue("Alt+Right"))
                .WithElement(
                    new TextConfigurationElement(STOP_ELEMENT, "Stop")
                        .WithValue("Alt+Back"))
                .WithElement(
                    new TextConfigurationElement(SETTINGS_ELEMENT, "Settings")
                        .WithValue("Alt+S"))
                .WithElement(
                    new TextConfigurationElement(MINI_PLAYER_ELEMENT, "Toggle Mini Player")
                        .WithValue("Alt+M")
            );
        }
    }
}
