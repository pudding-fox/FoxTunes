using System.Collections.Generic;

namespace FoxTunes
{
    public static class SpectrumBehaviourConfiguration
    {
        public const string SECTION = "B06236E7-F320-4D87-A1A6-9937E0B399BB";

        public const string ENABLED_ELEMENT = "50564810-FD40-4A44-BDF6-F0B2F60E674A";

        public const string BARS_ELEMENT = "CF0A0663-7CBF-4EE4-99C8-A0A096D4E876";

        public const string BARS_16_OPTION = "4201DF84-DC4C-463E-9A76-D9D424890D91";

        public const string BARS_32_OPTION = "917ABA8B-CBA3-4800-B4BE-30D0C0758F7E";

        public const string BARS_64_OPTION = "E0879524-BC5A-48C0-8210-921B204307FC";

        public const string BARS_128_OPTION = "47FB558E-7B9C-4101-992A-709B87756991";

        public const string BARS_256_OPTION = "1642FFC1-592E-4EC6-9CCD-5182935AD12E";

        public const string BARS_512_OPTION = "DC1897F3-1C4A-4660-ABC8-8E686F921FBF";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION, "Spectrum")
                .WithElement(new BooleanConfigurationElement(ENABLED_ELEMENT, "Enabled").WithValue(true))
                .WithElement(new SelectionConfigurationElement(BARS_ELEMENT, "Bars").WithOptions(GetBarsOptions())
            );
        }

        private static IEnumerable<SelectionConfigurationOption> GetBarsOptions()
        {
            yield return new SelectionConfigurationOption(BARS_16_OPTION, "16");
            yield return new SelectionConfigurationOption(BARS_32_OPTION, "32");
            yield return new SelectionConfigurationOption(BARS_64_OPTION, "64");
            yield return new SelectionConfigurationOption(BARS_128_OPTION, "128");
            yield return new SelectionConfigurationOption(BARS_256_OPTION, "256");
            yield return new SelectionConfigurationOption(BARS_512_OPTION, "512");
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
                case BARS_512_OPTION:
                    return 512;
            }
        }
    }
}
