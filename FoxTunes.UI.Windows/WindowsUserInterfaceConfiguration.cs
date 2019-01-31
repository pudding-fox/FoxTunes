using System;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes
{
    public static class WindowsUserInterfaceConfiguration
    {
        public const string SECTION = "0047011D-7C95-4EDE-A4DE-B839CF05E9AB";

        public const string THEME_ELEMENT = "AAAA9DEE-1168-4D96-9355-31ECC0666820";

        public const string SHOW_ARTWORK_ELEMENT = "BBBBCE34-1C79-4B44-A5A9-5134B503B062";

        public const string SHOW_LIBRARY_ELEMENT = "CCCCDA77-129F-4988-99EC-6A21EB9096D8";

        public const string UI_SCALING_ELEMENT = "DDDDFB85-BA70-4412-87BA-E4DC58AD9BA8";

        public const string MARQUEE_INTERVAL_ELEMENT = "EEEE685A-4D15-4AE1-B7AD-3E5786CB8EDB";

        public const string MARQUEE_STEP_ELEMENT = "FFFFDCB3-69C3-4F73-966C-6A7738E359A1";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            var themeOptions = GetThemeOptions().ToArray();
            yield return new ConfigurationSection(SECTION, "Appearance")
                .WithElement(
                    new SelectionConfigurationElement(THEME_ELEMENT, "Theme")
                    {
                        SelectedOption = themeOptions.FirstOrDefault()
                    }.WithOptions(() => themeOptions))
                .WithElement(
                    new BooleanConfigurationElement(SHOW_ARTWORK_ELEMENT, "Show Artwork").WithValue(true))
                .WithElement(
                    new BooleanConfigurationElement(SHOW_LIBRARY_ELEMENT, "Show Library").WithValue(true))
                .WithElement(
                    new TextConfigurationElement(UI_SCALING_ELEMENT, "Scaling Factor", path: "Advanced").WithValue("1").WithValidationRule(new DoubleValidationRule(0.5, 10)))
                .WithElement(
                    new TextConfigurationElement(MARQUEE_INTERVAL_ELEMENT, "Marquee Interval", path: "Advanced").WithValue("50").WithValidationRule(new IntegerValidationRule(10, 1000)))
                .WithElement(
                    new TextConfigurationElement(MARQUEE_STEP_ELEMENT, "Marquee Step", path: "Advanced").WithValue("0.75").WithValidationRule(new DoubleValidationRule(0.50, 10))
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

        public static ITheme GetTheme(string id)
        {
            var themes = ComponentRegistry.Instance.GetComponents<ITheme>();
            return themes.FirstOrDefault(theme => string.Equals(theme.Id, id, StringComparison.OrdinalIgnoreCase));
        }
    }
}
