using FoxTunes.Interfaces;
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

        public const string PRIMARY_LIBRARY_VIEW = "GGII1A2C-E16D-449D-A0E7-262B49D28C7D";

        public const string UI_SCALING_ELEMENT = "IIIIFB85-BA70-4412-87BA-E4DC58AD9BA8";

        public const string MARQUEE_INTERVAL_ELEMENT = "JJJJ685A-4D15-4AE1-B7AD-3E5786CB8EDB";

        public const string MARQUEE_STEP_ELEMENT = "KKKKDCB3-69C3-4F73-966C-6A7738E359A1";

        public const string EXTEND_GLASS_ELEMENT = "LLLL7881-D4F6-484C-8E4E-E3CD5802F8B5";

        public const string SHOW_CURSOR_ADORNERS = "NNNN7E23-A1E4-4BB6-9291-B553F4F7AD12";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            var releaseType = StandardComponents.Instance.Configuration.ReleaseType;
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
                    new SelectionConfigurationElement(PRIMARY_LIBRARY_VIEW, "Primary Library View", path: "Advanced").WithOptions(GetLibraryViews()))
                .WithElement(
                    new DoubleConfigurationElement(UI_SCALING_ELEMENT, "Scaling Factor", path: "Advanced").WithValue(1.0).WithValidationRule(new DoubleValidationRule(1, 4, 0.4)))
                .WithElement(
                    new IntegerConfigurationElement(MARQUEE_INTERVAL_ELEMENT, "Marquee Interval", path: "Advanced").WithValue(50).WithValidationRule(new IntegerValidationRule(10, 1000)))
                .WithElement(
                    new DoubleConfigurationElement(MARQUEE_STEP_ELEMENT, "Marquee Step", path: "Advanced").WithValue(0.80).WithValidationRule(new DoubleValidationRule(0.80, 10, 0.4)))
                .WithElement(
                    new BooleanConfigurationElement(EXTEND_GLASS_ELEMENT, "Extend Glass").WithValue(false))
                .WithElement(
                    new BooleanConfigurationElement(SHOW_CURSOR_ADORNERS, "Show Cursor Adorners", path: "Advanced").WithValue(releaseType == ReleaseType.Default)
            );
        }

        private static IEnumerable<SelectionConfigurationOption> GetThemeOptions()
        {
            var themes = ComponentRegistry.Instance.GetComponents<ITheme>();
            var releaseType = StandardComponents.Instance.Configuration.ReleaseType;
            foreach (var theme in themes)
            {
                var option = new SelectionConfigurationOption(theme.Id, theme.Name, theme.Description);
                if (theme.ReleaseType == releaseType)
                {
                    option.Default();
                }
                yield return option;
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

        private static IEnumerable<SelectionConfigurationOption> GetLibraryViews()
        {
            yield return new SelectionConfigurationOption(UIComponent.PLACEHOLDER, "None");
            foreach (var component in LayoutManager.Instance.Components)
            {
                if (component.Role != UIComponentRole.LibraryView)
                {
                    continue;
                }
                var option = new SelectionConfigurationOption(component.Id, component.Name, component.Description);
                if (string.Equals(option.Id, LibraryTree.ID, StringComparison.OrdinalIgnoreCase))
                {
                    option.Default();
                }
                yield return option;
            }
        }

        public static bool GetIsPrimaryView(SelectionConfigurationOption option, string id)
        {
            return string.Equals(option.Id, id, StringComparison.OrdinalIgnoreCase);
        }
    }
}
