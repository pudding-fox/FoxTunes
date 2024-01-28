using FoxTunes.Theme;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes
{
    public static class WindowsUserInterfaceConfiguration
    {
        public const string SECTION = "0047011D-7C95-4EDE-A4DE-B839CF05E9AB";

        public const string THEME_ELEMENT = "06189DEE-1168-4D96-9355-31ECC0666820";

        public const string SHOW_ARTWORK_ELEMENT = "220FCE34-1C79-4B44-A5A9-5134B503B062";

        public const string SHOW_LIBRARY_ELEMENT = "E21CDA77-129F-4988-99EC-6A21EB9096D8";

        public const string UI_SCALING_ELEMENT = "CBA3FB85-BA70-4412-87BA-E4DC58AD9BA8";

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
                    new TextConfigurationElement(UI_SCALING_ELEMENT, "Scaling Factor").WithValue("1").WithValidationRule(new ScalingFactorValidationRule())
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

        private class ScalingFactorValidationRule : ValidationRule
        {
            public override bool Validate(object value, out string message)
            {
                var scalingFactor = default(double);
                if (value is double)
                {
                    scalingFactor = (double)value;
                }
                else if (value is string)
                {
                    if (!double.TryParse((string)value, out scalingFactor))
                    {
                        message = "Numeric value expected.";
                        return false;
                    }
                }
                if (scalingFactor < 0.5 || scalingFactor > 10)
                {
                    message = "Value between 0.5 and 10 expected.";
                    return false;
                }
                message = null;
                return true;
            }
        }
    }
}
