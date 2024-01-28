using System.Collections.Generic;

namespace FoxTunes
{
    public static class EnhancedSpectrumBehaviourConfiguration
    {
        public const string SECTION = VisualizationBehaviourConfiguration.SECTION;

        public const string BANDS_ELEMENT = "AABBF573-83D3-498E-BEF8-F1DB5A329B9D";

        public const string BANDS_10_OPTION = "AAAA058C-2C96-4540-9ABE-10A584A17CE4";

        public const string BANDS_14_OPTION = "BBBB2B6D-E6FE-43F1-8358-AEE0299F0F8E";

        public const string BANDS_21_OPTION = "CCCCC739-B777-474B-B4A8-F96375254FAC";

        public const string BANDS_31_OPTION = "DDDD0A9D-0512-4F9F-903D-9AC33A9C6CFD";

        public const string PEAKS_ELEMENT = "BBBBDCF0-8B24-4321-B7BE-74DADE59D4FA";

        public const string HOLD_ELEMENT = "CCCCA25C-F8FA-4C37-82E8-2C5F297D2ED3";

        public const int MIN_HOLD = 500;

        public const int MAX_HOLD = 5000;

        public const int DEFAULT_HOLD = 1000;

        public const string RMS_ELEMENT = "DDDEE2B6A-188E-4FF4-A277-37D140D49C45";

        public const string CREST_ELEMENT = "DEEEFFB9-2014-4004-94F9-E566874317ED";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION)
                .WithElement(new SelectionConfigurationElement(BANDS_ELEMENT, Strings.EnhancedSpectrumBehaviourConfiguration_Bands, path: Strings.EnhancedSpectrumBehaviourConfiguration_Path).WithOptions(GetBandsOptions()))
                .WithElement(new BooleanConfigurationElement(PEAKS_ELEMENT, Strings.EnhancedSpectrumBehaviourConfiguration_Peaks, path: string.Format("{0}/{1}", Strings.EnhancedSpectrumBehaviourConfiguration_Path, Strings.General_Advanced)).WithValue(true))
                .WithElement(new IntegerConfigurationElement(HOLD_ELEMENT, Strings.EnhancedSpectrumBehaviourConfiguration_Hold, path: string.Format("{0}/{1}", Strings.EnhancedSpectrumBehaviourConfiguration_Path, Strings.General_Advanced)).WithValue(DEFAULT_HOLD).WithValidationRule(new IntegerValidationRule(MIN_HOLD, MAX_HOLD)).DependsOn(SECTION, PEAKS_ELEMENT))
                .WithElement(new BooleanConfigurationElement(RMS_ELEMENT, Strings.EnhancedSpectrumBehaviourConfiguration_Rms, path: string.Format("{0}/{1}", Strings.EnhancedSpectrumBehaviourConfiguration_Path, Strings.General_Advanced)).WithValue(true))
                .WithElement(new BooleanConfigurationElement(CREST_ELEMENT, Strings.EnhancedSpectrumBehaviourConfiguration_Crest, path: string.Format("{0}/{1}", Strings.EnhancedSpectrumBehaviourConfiguration_Path, Strings.General_Advanced)).WithValue(false)
            );
        }

        private static IEnumerable<SelectionConfigurationOption> GetBandsOptions()
        {
            yield return new SelectionConfigurationOption(BANDS_10_OPTION, "10");
            yield return new SelectionConfigurationOption(BANDS_14_OPTION, "14").Default();
            yield return new SelectionConfigurationOption(BANDS_21_OPTION, "21");
            yield return new SelectionConfigurationOption(BANDS_31_OPTION, "31");
        }

        public static int[] GetBands(SelectionConfigurationOption option)
        {
            switch (option.Id)
            {
                case BANDS_10_OPTION:
                    return new[]
                    {
                        20,
                        50,
                        100,
                        200,
                        500,
                        1000,
                        2000,
                        5000,
                        10000,
                        20000
                    };
                default:
                case BANDS_14_OPTION:
                    return new[]
                    {
                        20,
                        50,
                        100,
                        200,
                        500,
                        1000,
                        1400,
                        2000,
                        3000,
                        5000,
                        7500,
                        10000,
                        17000,
                        20000
                    };
                case BANDS_21_OPTION:
                    return new[]
                    {
                        20,
                        35,
                        50,
                        70,
                        100,
                        160,
                        200,
                        360,
                        500,
                        760,
                        1000,
                        1400,
                        2000,
                        2600,
                        3000,
                        5000,
                        7500,
                        10000,
                        13000,
                        17000,
                        20000
                    };
                case BANDS_31_OPTION:
                    return new[]
                    {
                        20,
                        35,
                        50,
                        70,
                        100,
                        120,
                        160,
                        200,
                        300,
                        360,
                        440,
                        500,
                        600,
                        760,
                        1000,
                        1200,
                        1400,
                        1600,
                        2000,
                        2600,
                        3000,
                        3600,
                        4000,
                        5000,
                        7500,
                        10000,
                        12000,
                        14000,
                        17000,
                        20000
                    };
            }
        }

        public static int GetWidth(SelectionConfigurationOption option)
        {
            switch (option.Id)
            {
                default:
                case BANDS_10_OPTION:
                    return 280;
                case BANDS_14_OPTION:
                    return 364;
                case BANDS_21_OPTION:
                    return 516;
                case BANDS_31_OPTION:
                    return 728;
            }
        }
    }
}
