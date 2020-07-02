using FoxTunes.Interfaces;
using System.Collections.Generic;

namespace FoxTunes
{
    public static class SpectrumBehaviourConfiguration
    {
        public const string SECTION = "B06236E7-F320-4D87-A1A6-9937E0B399BB";

        public const string ENABLED_ELEMENT = "50564810-FD40-4A44-BDF6-F0B2F60E674A";

        public const string BARS_ELEMENT = "CF0A0663-7CBF-4EE4-99C8-A0A096D4E876";

        public const string BARS_16_OPTION = "AAAADF84-DC4C-463E-9A76-D9D424890D91";

        public const string BARS_32_OPTION = "BBBBBA8B-CBA3-4800-B4BE-30D0C0758F7E";

        public const string BARS_64_OPTION = "CCCC9524-BC5A-48C0-8210-921B204307FC";

        public const string BARS_128_OPTION = "DDDD558E-7B9C-4101-992A-709B87756991";

        public const string BARS_256_OPTION = "EEEEFFC1-592E-4EC6-9CCD-5182935AD12E";

        public const string PEAKS_ELEMENT = "DDDD7FCF-8A71-4367-8F48-4F8D8C89739C";

        public const string INTERVAL_ELEMENT = "FFFF5F0C-6574-472A-B9EB-2BDBC1F3C438";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            var releaseType = StandardComponents.Instance.Configuration.ReleaseType;
            yield return new ConfigurationSection(SECTION, "Spectrum")
                .WithElement(new BooleanConfigurationElement(ENABLED_ELEMENT, "Enabled").WithValue(releaseType == ReleaseType.Default))
                .WithElement(new SelectionConfigurationElement(BARS_ELEMENT, "Bars").WithOptions(GetBarsOptions()).DependsOn(SECTION, ENABLED_ELEMENT))
                .WithElement(new BooleanConfigurationElement(PEAKS_ELEMENT, "Peaks").WithValue(true).DependsOn(SECTION, ENABLED_ELEMENT))
                .WithElement(new IntegerConfigurationElement(INTERVAL_ELEMENT, "Interval").WithValue(100).WithValidationRule(new IntegerValidationRule(1, 1000)).DependsOn(SECTION, ENABLED_ELEMENT)
            );
        }

        private static IEnumerable<SelectionConfigurationOption> GetBarsOptions()
        {
            yield return new SelectionConfigurationOption(BARS_16_OPTION, "16");
            yield return new SelectionConfigurationOption(BARS_32_OPTION, "32").Default();
            yield return new SelectionConfigurationOption(BARS_64_OPTION, "64");
            yield return new SelectionConfigurationOption(BARS_128_OPTION, "128");
            yield return new SelectionConfigurationOption(BARS_256_OPTION, "256");
        }

        public static int GetBars(SelectionConfigurationOption option)
        {
            switch (option.Id)
            {
                default:
                case BARS_16_OPTION:
                    return 16;
                case BARS_32_OPTION:
                    return 32;
                case BARS_64_OPTION:
                    return 64;
                case BARS_128_OPTION:
                    return 128;
                case BARS_256_OPTION:
                    return 256;
            }
        }

        public static int GetWidth(SelectionConfigurationOption option)
        {
            switch (option.Id)
            {
                default:
                case BARS_16_OPTION:
                    return 160;
                case BARS_32_OPTION:
                    return 160;
                case BARS_64_OPTION:
                    return 192;
                case BARS_128_OPTION:
                    return 256;
                case BARS_256_OPTION:
                    return 256;
            }
        }
    }
}
