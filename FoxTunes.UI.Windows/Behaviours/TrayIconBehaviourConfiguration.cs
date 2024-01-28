using System.Collections.Generic;

namespace FoxTunes
{
    public static class TrayIconBehaviourConfiguration
    {
        public const string TRAY_ICON_ELEMENT = "82D11AC8-7D75-43C9-9E99-FF69EC5D8040";

        public const string MINIMIZE_TO_TRAY_ELEMENT = "98EF2156-2E0E-4EA3-AFAA-6775CF3421F3";

        public const string CLOSE_TO_TRAY_ELEMENT = "E851012A-C958-4964-9574-D57196F36D21";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(WindowsUserInterfaceConfiguration.APPEARANCE_SECTION, "Appearance")
                .WithElement(
                    new BooleanConfigurationElement(TRAY_ICON_ELEMENT, "Show Tray Icon").WithValue(false))
                .WithElement(
                    new BooleanConfigurationElement(MINIMIZE_TO_TRAY_ELEMENT, "Minimize To Tray").WithValue(true))
                .WithElement(
                    new BooleanConfigurationElement(CLOSE_TO_TRAY_ELEMENT, "Close To Tray").WithValue(false)
            );
            StandardComponents.Instance.Configuration.GetElement(WindowsUserInterfaceConfiguration.APPEARANCE_SECTION, TRAY_ICON_ELEMENT).ConnectValue<bool>(mode => UpdateConfiguration(mode));
        }

        private static void UpdateConfiguration(bool mode)
        {
            if (mode)
            {
                StandardComponents.Instance.Configuration.GetElement<BooleanConfigurationElement>(WindowsUserInterfaceConfiguration.APPEARANCE_SECTION, MINIMIZE_TO_TRAY_ELEMENT).Show();
                StandardComponents.Instance.Configuration.GetElement<BooleanConfigurationElement>(WindowsUserInterfaceConfiguration.APPEARANCE_SECTION, CLOSE_TO_TRAY_ELEMENT).Show();
            }
            else
            {
                StandardComponents.Instance.Configuration.GetElement<BooleanConfigurationElement>(WindowsUserInterfaceConfiguration.APPEARANCE_SECTION, MINIMIZE_TO_TRAY_ELEMENT).Hide();
                StandardComponents.Instance.Configuration.GetElement<BooleanConfigurationElement>(WindowsUserInterfaceConfiguration.APPEARANCE_SECTION, CLOSE_TO_TRAY_ELEMENT).Hide();
            }
        }
    }
}
