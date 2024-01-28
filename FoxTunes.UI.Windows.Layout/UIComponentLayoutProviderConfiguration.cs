using FoxTunes.Interfaces;
using System.Collections.Generic;

namespace FoxTunes
{
    public static class UIComponentLayoutProviderConfiguration
    {
        public const string ID = "BBBB083C-3C19-4AAC-97C7-565AF8F83115";

        public const string MAIN = "90732E4B-657B-45C2-B365-8FD69498D7C2";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(WindowsUserInterfaceConfiguration.SECTION, "Appearance")
                .WithElement(
                    new SelectionConfigurationElement(WindowsUserInterfaceConfiguration.LAYOUT_ELEMENT, "Layout").WithOptions(GetLayoutOptions()))
                .WithElement(
                    new TextConfigurationElement(MAIN).WithValue(Resources.Main).Hide()
            );
        }

        private static IEnumerable<SelectionConfigurationOption> GetLayoutOptions()
        {
            var releaseType = StandardComponents.Instance.Configuration.ReleaseType;
            var option = new SelectionConfigurationOption(ID, "Dynamic");
            if (releaseType == ReleaseType.Default)
            {
                option.Default();
            }
            yield return option;
        }
    }
}
