using System.Collections.Generic;

namespace FoxTunes
{
    public static class InputManagerConfiguration
    {
        public const string SECTION = "4B5B0E73-8000-484E-8F68-77E11FC8AD45";

        public const string ENABLED_ELEMENT = "AAAA41A0-976D-4573-8758-984AACBD235B";

        public const string PLAY_ELEMENT = "BBBBD9B9-1D10-4EEB-81BA-7D54C7A86198";

        public const string PREVIOUS_ELEMENT = "CCCC497E-32FA-4E0F-B73F-8BF4705782B0";

        public const string NEXT_ELEMENT = "DDDD25C3-6A2C-4D2C-985B-1D7210304EFB";

        public const string STOP_ELEMENT = "EEEE73AA-0388-4988-8068-59B768F2A02E";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION, "Keyboard Shortcuts")
                .WithElement(
                    new BooleanConfigurationElement(ENABLED_ELEMENT, "Enabled", path: "Global Hotkeys").WithValue(false))
                .WithElement(
                    new TextConfigurationElement(PLAY_ELEMENT, "Play", path: "Global Hotkeys")
                        .WithValue("MediaPlayPause"))
                .WithElement(
                    new TextConfigurationElement(PREVIOUS_ELEMENT, "Previous", path: "Global Hotkeys")
                        .WithValue("MediaPreviousTrack"))
                 .WithElement(
                    new TextConfigurationElement(NEXT_ELEMENT, "Next", path: "Global Hotkeys")
                        .WithValue("MediaNextTrack"))
                .WithElement(
                    new TextConfigurationElement(STOP_ELEMENT, "Stop", path: "Global Hotkeys")
                        .WithValue("MediaStop")
            );
            StandardComponents.Instance.Configuration.GetElement<BooleanConfigurationElement>(SECTION, ENABLED_ELEMENT).ConnectValue(enabled => UpdateConfiguration(enabled));
        }

        private static void UpdateConfiguration(bool enabled)
        {
            if (enabled)
            {
                StandardComponents.Instance.Configuration.GetElement(SECTION, PLAY_ELEMENT).Show();
                StandardComponents.Instance.Configuration.GetElement(SECTION, PREVIOUS_ELEMENT).Show();
                StandardComponents.Instance.Configuration.GetElement(SECTION, NEXT_ELEMENT).Show();
                StandardComponents.Instance.Configuration.GetElement(SECTION, STOP_ELEMENT).Show();
            }
            else
            {
                StandardComponents.Instance.Configuration.GetElement(SECTION, PLAY_ELEMENT).Hide();
                StandardComponents.Instance.Configuration.GetElement(SECTION, PREVIOUS_ELEMENT).Hide();
                StandardComponents.Instance.Configuration.GetElement(SECTION, NEXT_ELEMENT).Hide();
                StandardComponents.Instance.Configuration.GetElement(SECTION, STOP_ELEMENT).Hide();
            }
        }
    }
}
