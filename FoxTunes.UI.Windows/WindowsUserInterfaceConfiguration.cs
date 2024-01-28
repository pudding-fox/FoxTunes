using FoxTunes.Theme;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace FoxTunes
{
    public static class WindowsUserInterfaceConfiguration
    {
        public const string APPEARANCE_SECTION = "0047011D-7C95-4EDE-A4DE-B839CF05E9AB";

        public const string THEME_ELEMENT = "06189DEE-1168-4D96-9355-31ECC0666820";

        public const string SHOW_ARTWORK_ELEMENT = "220FCE34-1C79-4B44-A5A9-5134B503B062";

        public const string SHOW_LIBRARY_ELEMENT = "E21CDA77-129F-4988-99EC-6A21EB9096D8";

        public const string KEYBOARD_SHORTCUTS_SECTION = "4B5B0E73-8000-484E-8F68-77E11FC8AD45";

        public const string PLAY_ELEMENT = "98B8D9B9-1D10-4EEB-81BA-7D54C7A86198";

        public const string PREVIOUS_ELEMENT = "D782497E-32FA-4E0F-B73F-8BF4705782B0";

        public const string NEXT_ELEMENT = "504225C3-6A2C-4D2C-985B-1D7210304EFB";

        public const string STOP_ELEMENT = "6C8C73AA-0388-4988-8068-59B768F2A02E";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            var themeOptions = GetThemeOptions().ToArray();
            yield return new ConfigurationSection(APPEARANCE_SECTION, "Appearance")
                .WithElement(
                    new SelectionConfigurationElement(THEME_ELEMENT, "Theme")
                    {
                        SelectedOption = themeOptions.FirstOrDefault()
                    }.WithOptions(() => themeOptions))
                .WithElement(
                    new BooleanConfigurationElement(SHOW_ARTWORK_ELEMENT, "Show Artwork").WithValue(true))
                .WithElement(
                    new BooleanConfigurationElement(SHOW_LIBRARY_ELEMENT, "Show Library").WithValue(true)
            );
            yield return new ConfigurationSection(KEYBOARD_SHORTCUTS_SECTION, "Keyboard Shortcuts")
                .WithElement(
                    new TextConfigurationElement(PLAY_ELEMENT, "Play")
                        .WithValue(Enum.GetName(typeof(Key), Key.MediaPlayPause)))
                .WithElement(
                    new TextConfigurationElement(PREVIOUS_ELEMENT, "Previous")
                        .WithValue(Enum.GetName(typeof(Key), Key.MediaPreviousTrack)))
                 .WithElement(
                    new TextConfigurationElement(NEXT_ELEMENT, "Next")
                        .WithValue(Enum.GetName(typeof(Key), Key.MediaNextTrack)))
                         .WithElement(
                    new TextConfigurationElement(STOP_ELEMENT, "Stop")
                        .WithValue(Enum.GetName(typeof(Key), Key.MediaStop))
            );
        }

        private static IEnumerable<SelectionConfigurationOption> GetThemeOptions()
        {
            var themes = ComponentRegistry.Instance.GetComponents<ITheme>();
            foreach (var theme in themes)
            {
                yield return new SelectionConfigurationOption(theme.Id, theme.Name, theme.Description);
            }
        }
    }
}
