using System.Collections.Generic;

namespace FoxTunes
{
    public static class InputManagerConfiguration
    {
        public const string SECTION = "4B5B0E73-8000-484E-8F68-77E11FC8AD45";

        public const string ENABLED_ELEMENT = "D60E41A0-976D-4573-8758-984AACBD235B";

        public const string PLAY_ELEMENT = "98B8D9B9-1D10-4EEB-81BA-7D54C7A86198";

        public const string PREVIOUS_ELEMENT = "D782497E-32FA-4E0F-B73F-8BF4705782B0";

        public const string NEXT_ELEMENT = "504225C3-6A2C-4D2C-985B-1D7210304EFB";

        public const string STOP_ELEMENT = "6C8C73AA-0388-4988-8068-59B768F2A02E";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION, "Keyboard Shortcuts")
                .WithElement(
                    new BooleanConfigurationElement(ENABLED_ELEMENT, "Enabled").WithValue(false))
                .WithElement(
                    new TextConfigurationElement(PLAY_ELEMENT, "Play")
                        .WithValue("MediaPlayPause"))
                .WithElement(
                    new TextConfigurationElement(PREVIOUS_ELEMENT, "Previous")
                        .WithValue("MediaPreviousTrack"))
                 .WithElement(
                    new TextConfigurationElement(NEXT_ELEMENT, "Next")
                        .WithValue("MediaNextTrack"))
                         .WithElement(
                    new TextConfigurationElement(STOP_ELEMENT, "Stop")
                        .WithValue("MediaStop")
            );
            StandardComponents.Instance.Configuration.GetElement(
                SECTION,
                ENABLED_ELEMENT
            ).ConnectValue<bool>(enabled => UpdateConfiguration(enabled));
        }

        private static void UpdateConfiguration(bool enabled)
        {
            if (enabled)
            {
                StandardComponents.Instance.Configuration.GetElement<TextConfigurationElement>(SECTION, PLAY_ELEMENT).Show();
                StandardComponents.Instance.Configuration.GetElement<TextConfigurationElement>(SECTION, PREVIOUS_ELEMENT).Show();
                StandardComponents.Instance.Configuration.GetElement<TextConfigurationElement>(SECTION, NEXT_ELEMENT).Show();
                StandardComponents.Instance.Configuration.GetElement<TextConfigurationElement>(SECTION, STOP_ELEMENT).Show();
            }
            else
            {
                StandardComponents.Instance.Configuration.GetElement<TextConfigurationElement>(SECTION, PLAY_ELEMENT).Hide();
                StandardComponents.Instance.Configuration.GetElement<TextConfigurationElement>(SECTION, PREVIOUS_ELEMENT).Hide();
                StandardComponents.Instance.Configuration.GetElement<TextConfigurationElement>(SECTION, NEXT_ELEMENT).Hide();
                StandardComponents.Instance.Configuration.GetElement<TextConfigurationElement>(SECTION, STOP_ELEMENT).Hide();
            }
        }
    }
}
