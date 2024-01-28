using FoxTunes.Interfaces;
using System.Collections.Generic;

namespace FoxTunes
{
    public static class UIComponentLayoutProviderConfiguration
    {
        public const string ID = "BBBB083C-3C19-4AAC-97C7-565AF8F83115";

        public const string MAIN = "90732E4B-657B-45C2-B365-8FD69498D7C2";

        public const string RESET = "ZZZZ7FC9-B469-4AAB-A32E-FF4ECF6F9C56";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(WindowsUserInterfaceConfiguration.SECTION, "Appearance")
                .WithElement(
                    new SelectionConfigurationElement(WindowsUserInterfaceConfiguration.LAYOUT_ELEMENT, "Layout").WithOptions(GetLayoutOptions()))
                .WithElement(
                    new TextConfigurationElement(MAIN).WithValue(string.Empty).Hide())
                .WithElement(
                    new CommandConfigurationElement(RESET, "Reset Layout").WithHandler(ResetLayout)
            );
            StandardComponents.Instance.Configuration.GetElement<SelectionConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                WindowsUserInterfaceConfiguration.LAYOUT_ELEMENT
            ).ConnectValue(option => UpdateConfiguration(option));
        }

        private static void UpdateConfiguration(SelectionConfigurationOption option)
        {
            switch (option.Id)
            {
                case ID:
                    StandardComponents.Instance.Configuration.GetElement(WindowsUserInterfaceConfiguration.SECTION, RESET).Show();
                    break;
                default:
                    StandardComponents.Instance.Configuration.GetElement(WindowsUserInterfaceConfiguration.SECTION, RESET).Hide();
                    break;
            }
        }

        private static IEnumerable<SelectionConfigurationOption> GetLayoutOptions()
        {
            var releaseType = StandardComponents.Instance.Configuration.ReleaseType;
            var option = new SelectionConfigurationOption(ID, "Standard");
            if (releaseType == ReleaseType.Default)
            {
                option.Default();
            }
            yield return option;
        }

        public static void ResetLayout()
        {
            var configuration = ComponentRegistry.Instance.GetComponent<IConfiguration>();
            var element = configuration.GetElement<TextConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                MAIN
            );
            element.Value = string.Empty;
        }
    }
}
