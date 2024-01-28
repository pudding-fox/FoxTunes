using FoxTunes.Interfaces;
using System.Collections.Generic;

namespace FoxTunes
{
    public static class MinimalLayoutProviderConfiguration
    {
        public const string ID = "AAAAA18B-86F0-45A6-988D-B15A56128429";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(WindowsUserInterfaceConfiguration.SECTION, "Appearance")
                .WithElement(
                    new SelectionConfigurationElement(WindowsUserInterfaceConfiguration.LAYOUT_ELEMENT, "Layout").WithOptions(GetLayoutOptions())
            );
        }

        private static IEnumerable<SelectionConfigurationOption> GetLayoutOptions()
        {
            var releaseType = StandardComponents.Instance.Configuration.ReleaseType;
            var option = new SelectionConfigurationOption(ID, "Minimal");
            if (releaseType == ReleaseType.Minimal)
            {
                option.Default();
            }
            yield return option;
        }
    }
}
