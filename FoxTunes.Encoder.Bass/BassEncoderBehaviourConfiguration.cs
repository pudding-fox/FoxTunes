using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FoxTunes
{
    public static class BassEncoderBehaviourConfiguration
    {
        public const string SECTION = "D1D2586A-8FE3-4F1E-A5A2-58C57A99FB11";

        public const string FORMAT_ELEMENT = "AAAA29F6-02C7-4694-A689-C2DC9EDEE419";

        public const string DESTINATION_ELEMENT = "BBBBBF10-E515-4A99-B49B-4C5821A22945";

        public const string DESTINATION_BROWSE_OPTION = "AAAA0072-4E23-4735-BF80-8AD72A4AE927";

        public const string DESTINATION_SOURCE_OPTION = "BBBB2B59-96E2-49F3-A727-1E102D7C9D3F";

        public const string DESTINATION_SPECIFIC_OPTION = "CCCC1412-8AAE-4191-A920-8BB042D21F39";

        public const string DESTINATION_LOCATION_ELEMENT = "CCCC1639-B91A-44A1-8579-364639C3F52C";

        public const string COPY_TAGS = "DDDD37B0-7CB2-4E55-9DD4-8B1F7DEF0EC9";

        public const string THREADS_ELEMENT = "EEEE8113-0C57-464D-B370-C3B77AC048D2";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION, "Converter")
                .WithElement(
                    new SelectionConfigurationElement(FORMAT_ELEMENT, "Format").WithOptions(GetFormatOptions()))
                .WithElement(
                    new SelectionConfigurationElement(DESTINATION_ELEMENT, "Destination").WithOptions(GetDestinationOptions()))
                .WithElement(
                    new TextConfigurationElement(DESTINATION_LOCATION_ELEMENT, "Location").WithValue(
                        Path.Combine(
                            ComponentScanner.Instance.Location,
                            "Converter"
                        )
                    ).WithFlags(ConfigurationElementFlags.FolderName)
                )
                .WithElement(
                    new BooleanConfigurationElement(COPY_TAGS, "Copy Tags").WithValue(true))
                .WithElement(
                    new IntegerConfigurationElement(THREADS_ELEMENT, "Background Threads").WithValue(Environment.ProcessorCount).WithValidationRule(new IntegerValidationRule(1, 32))
            );
            StandardComponents.Instance.Configuration.GetElement<SelectionConfigurationElement>(SECTION, DESTINATION_ELEMENT).ConnectValue(option => UpdateConfiguration(option));
        }

        private static IEnumerable<SelectionConfigurationOption> GetFormatOptions()
        {
            foreach (var settings in ComponentRegistry.Instance.GetComponents<IBassEncoderSettings>())
            {
                yield return new SelectionConfigurationOption(settings.Name, settings.Name);
            }
        }

        public static IBassEncoderSettings GetFormat(SelectionConfigurationOption option)
        {
            return ComponentRegistry.Instance.GetComponents<IBassEncoderSettings>().FirstOrDefault(
                settings => string.Equals(settings.Name, option.Name, StringComparison.OrdinalIgnoreCase)
            );
        }

        private static IEnumerable<SelectionConfigurationOption> GetDestinationOptions()
        {
            yield return new SelectionConfigurationOption(DESTINATION_BROWSE_OPTION, "Browse Folder");
            yield return new SelectionConfigurationOption(DESTINATION_SOURCE_OPTION, "Source Folder");
            yield return new SelectionConfigurationOption(DESTINATION_SPECIFIC_OPTION, "Specific Folder");
        }

        public static BassEncoderOutputDestination GetDestination(SelectionConfigurationOption option)
        {
            switch (option.Id)
            {
                default:
                case DESTINATION_BROWSE_OPTION:
                    return BassEncoderOutputDestination.Browse;
                case DESTINATION_SOURCE_OPTION:
                    return BassEncoderOutputDestination.Source;
                case DESTINATION_SPECIFIC_OPTION:
                    return BassEncoderOutputDestination.Specific;
            }
        }

        private static void UpdateConfiguration(SelectionConfigurationOption option)
        {
            switch (option.Id)
            {
                case DESTINATION_BROWSE_OPTION:
                case DESTINATION_SOURCE_OPTION:
                    StandardComponents.Instance.Configuration.GetElement(SECTION, DESTINATION_LOCATION_ELEMENT).Hide();
                    break;
                case DESTINATION_SPECIFIC_OPTION:
                    StandardComponents.Instance.Configuration.GetElement(SECTION, DESTINATION_LOCATION_ELEMENT).Show();
                    break;
            }
        }
    }
}
