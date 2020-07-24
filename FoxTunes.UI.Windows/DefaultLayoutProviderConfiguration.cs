using FoxTunes.Interfaces;
using System.Collections.Generic;

namespace FoxTunes
{
    public static class DefaultLayoutProviderConfiguration
    {
        public const string ID = "AAAA4ED2-782D-4622-ADF4-AAE2B543E0F3";

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
            var option = new SelectionConfigurationOption(ID, "Basic");
            if (releaseType == ReleaseType.Default)
            {
                option.Default();
            }
            yield return option;
        }
    }
}
