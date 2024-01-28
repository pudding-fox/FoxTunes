using System;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes
{
    public static class WindowsUserInterfaceConfiguration
    {
        public const string SECTION = "0047011D-7C95-4EDE-A4DE-B839CF05E9AB";

        public const string THEME_ELEMENT = "AAAA9DEE-1168-4D96-9355-31ECC0666820";

        public const string TOP_LEFT_ELEMENT = "BBBBBF2-DA76-4696-A4E1-F1E77D169C4F";

        public const string BOTTOM_LEFT_ELEMENT = "CCCC515-DC31-4DAD-8EF2-273AEE88A627";

        public const string TOP_CENTER_ELEMENT = "DDDD122D-45D9-4463-8BE3-E706CBB2FF67";

        public const string BOTTOM_CENTER_ELEMENT = "EEEE00E8-5BC8-465D-A5CE-BB93938B43C6";

        public const string TOP_RIGHT_ELEMENT = "FFFF12F1-1F4E-414B-B437-E9422C440CC0";

        public const string BOTTOM_RIGHT_ELEMENT = "GGGGEC8-4D19-4E0F-9D65-44A15288B22A";

        public const string UI_SCALING_ELEMENT = "IIIIFB85-BA70-4412-87BA-E4DC58AD9BA8";

        public const string MARQUEE_INTERVAL_ELEMENT = "JJJJ685A-4D15-4AE1-B7AD-3E5786CB8EDB";

        public const string MARQUEE_STEP_ELEMENT = "KKKKDCB3-69C3-4F73-966C-6A7738E359A1";

        public const string EXTEND_GLASS_ELEMENT = "LLLL7881-D4F6-484C-8E4E-E3CD5802F8B5";

        public const string SEARCH_INTERVAL_ELEMENT = "MMMM2482-0BD0-46A9-A110-C9835331F11B";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION, "Appearance")
                .WithElement(
                    new SelectionConfigurationElement(THEME_ELEMENT, "Theme").WithOptions(GetThemeOptions()))
                .WithElement(
                    new SelectionConfigurationElement(TOP_LEFT_ELEMENT, "Top Left", path: "Layout").WithOptions(GetControlOptions(TOP_LEFT_ELEMENT)))
                .WithElement(
                    new SelectionConfigurationElement(BOTTOM_LEFT_ELEMENT, "Bottom Left", path: "Layout").WithOptions(GetControlOptions(BOTTOM_LEFT_ELEMENT)))
                .WithElement(
                    new SelectionConfigurationElement(TOP_CENTER_ELEMENT, "Top Center", path: "Layout").WithOptions(GetControlOptions(TOP_CENTER_ELEMENT)))
                .WithElement(
                    new SelectionConfigurationElement(BOTTOM_CENTER_ELEMENT, "Bottom Center", path: "Layout").WithOptions(GetControlOptions(BOTTOM_CENTER_ELEMENT)))
                .WithElement(
                    new SelectionConfigurationElement(TOP_RIGHT_ELEMENT, "Top Right", path: "Layout").WithOptions(GetControlOptions(TOP_RIGHT_ELEMENT)))
                .WithElement(
                    new SelectionConfigurationElement(BOTTOM_RIGHT_ELEMENT, "Bottom Right", path: "Layout").WithOptions(GetControlOptions(BOTTOM_RIGHT_ELEMENT)))
                .WithElement(
                    new DoubleConfigurationElement(UI_SCALING_ELEMENT, "Scaling Factor", path: "Advanced").WithValue(1.0).WithValidationRule(new DoubleValidationRule(1, 4, 0.4)))
                .WithElement(
                    new IntegerConfigurationElement(MARQUEE_INTERVAL_ELEMENT, "Marquee Interval", path: "Advanced").WithValue(50).WithValidationRule(new IntegerValidationRule(10, 1000)))
                .WithElement(
                    new DoubleConfigurationElement(MARQUEE_STEP_ELEMENT, "Marquee Step", path: "Advanced").WithValue(0.80).WithValidationRule(new DoubleValidationRule(0.80, 10, 0.4)))
                .WithElement(
                    new BooleanConfigurationElement(EXTEND_GLASS_ELEMENT, "Extend Glass").WithValue(false))
                .WithElement(
                    new IntegerConfigurationElement(SEARCH_INTERVAL_ELEMENT, "Search Interval", path: "Advanced").WithValue(1000).WithValidationRule(new IntegerValidationRule(100, 1000, 100))
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

        public static ITheme GetTheme(SelectionConfigurationOption option)
        {
            var themes = ComponentRegistry.Instance.GetComponents<ITheme>();
            return themes.FirstOrDefault(theme => string.Equals(theme.Id, option.Id, StringComparison.OrdinalIgnoreCase));
        }

        private static IEnumerable<SelectionConfigurationOption> GetControlOptions(string id)
        {
            yield return new SelectionConfigurationOption(UIComponent.PLACEHOLDER, "Empty");
            foreach (var component in LayoutManager.Instance.Components)
            {
                var option = new SelectionConfigurationOption(component.Id, component.Name, component.Description);
                if (string.Equals(component.Slot, id, StringComparison.OrdinalIgnoreCase))
                {
                    option.Default();
                }
                yield return option;
            }
        }

        public static Type GetControl(SelectionConfigurationOption option)
        {
            var component = LayoutManager.Instance.GetComponent(option.Id);
            if (component == null)
            {
                return LayoutManager.PLACEHOLDER;
            }
            return component.Type;
        }
    }
}
