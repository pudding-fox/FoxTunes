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

        public const string LIBRARY_VIEW_ELEMENT = "DDDD4100-3796-4A97-9F84-1976760927C8";

        public const string LIBRARY_TREE_OPTION = "AAAA996B-57B8-4B7E-A0AF-C632B341D3C5";

        public const string LIBRARY_BROWSER_OPTION = "BBBB4708-2F1B-46E5-B4E3-C321F01640EC";

        public const string UI_SCALING_ELEMENT = "EEEEFB85-BA70-4412-87BA-E4DC58AD9BA8";

        public const string MARQUEE_INTERVAL_ELEMENT = "FFFF685A-4D15-4AE1-B7AD-3E5786CB8EDB";

        public const string MARQUEE_STEP_ELEMENT = "GGGGDCB3-69C3-4F73-966C-6A7738E359A1";

        public const string EXTEND_GLASS_ELEMENT = "HHHH7881-D4F6-484C-8E4E-E3CD5802F8B5";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION, "Appearance")
                .WithElement(
                    new SelectionConfigurationElement(THEME_ELEMENT, "Theme").WithOptions(GetThemeOptions()))
                .WithElement(
                    new BooleanConfigurationElement(SHOW_ARTWORK_ELEMENT, "Show Artwork").WithValue(true))
                .WithElement(
                    new BooleanConfigurationElement(SHOW_LIBRARY_ELEMENT, "Show Library").WithValue(true))
                .WithElement(
                    new SelectionConfigurationElement(LIBRARY_VIEW_ELEMENT, "Library View").WithOptions(GetLibraryViewOptions()))
                .WithElement(
                    new DoubleConfigurationElement(UI_SCALING_ELEMENT, "Scaling Factor", path: "Advanced").WithValue(1.0).WithValidationRule(new DoubleValidationRule(0.5, 10)))
                .WithElement(
                    new IntegerConfigurationElement(MARQUEE_INTERVAL_ELEMENT, "Marquee Interval", path: "Advanced").WithValue(50).WithValidationRule(new IntegerValidationRule(10, 1000)))
                .WithElement(
                    new DoubleConfigurationElement(MARQUEE_STEP_ELEMENT, "Marquee Step", path: "Advanced").WithValue(0.75).WithValidationRule(new DoubleValidationRule(0.50, 10)))
                .WithElement(
                    new BooleanConfigurationElement(EXTEND_GLASS_ELEMENT, "Extend Glass").WithValue(false)
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

        private static IEnumerable<SelectionConfigurationOption> GetLibraryViewOptions()
        {
            yield return new SelectionConfigurationOption(LIBRARY_TREE_OPTION, "Tree");
            yield return new SelectionConfigurationOption(LIBRARY_BROWSER_OPTION, "Browser");
        }

        public static object GetLibraryView(SelectionConfigurationOption option)
        {
            switch (option.Id)
            {
                case LIBRARY_TREE_OPTION:
                    return new LibraryTree();
                case LIBRARY_BROWSER_OPTION:
                    return new LibraryBrowser();
            }
            return null;
        }
    }
}
