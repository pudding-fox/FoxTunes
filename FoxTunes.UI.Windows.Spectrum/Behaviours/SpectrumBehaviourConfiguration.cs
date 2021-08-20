using FoxTunes.Interfaces;
using System.Collections.Generic;

namespace FoxTunes
{
    public static class SpectrumBehaviourConfiguration
    {
        private static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        public const string SECTION = VisualizationBehaviourConfiguration.SECTION;

        public const string BARS_ELEMENT = "AAAA0663-7CBF-4EE4-99C8-A0A096D4E876";

        public const string BARS_16_OPTION = "AAAADF84-DC4C-463E-9A76-D9D424890D91";

        public const string BARS_32_OPTION = "BBBBBA8B-CBA3-4800-B4BE-30D0C0758F7E";

        public const string BARS_64_OPTION = "CCCC9524-BC5A-48C0-8210-921B204307FC";

        public const string BARS_128_OPTION = "DDDD558E-7B9C-4101-992A-709B87756991";

        public const string BARS_256_OPTION = "EEEEFFC1-592E-4EC6-9CCD-5182935AD12E";

        public const string BANDS_ELEMENT = "AABBF573-83D3-498E-BEF8-F1DB5A329B9D";

        public const string BANDS_10_OPTION = "AAAA058C-2C96-4540-9ABE-10A584A17CE4";

        public const string BANDS_14_OPTION = "BBBB2B6D-E6FE-43F1-8358-AEE0299F0F8E";

        public const string BANDS_21_OPTION = "CCCCC739-B777-474B-B4A8-F96375254FAC";

        public const string BANDS_31_OPTION = "DDDDDA9D-0512-4F9F-903D-9AC33A9C6CFD";

        public const string PEAKS_ELEMENT = "DDDD7FCF-8A71-4367-8F48-4F8D8C89739C";

        public const string RMS_ELEMENT = "DDDEE2B6A-188E-4FF4-A277-37D140D49C45";

        public const string CREST_ELEMENT = "DEEEFFB9-2014-4004-94F9-E566874317ED";

        public const string HIGH_CUT_ELEMENT = "DDEE2ADA-C904-4AA5-82F0-F53412EF24BD";

        public const string HOLD_ELEMENT = "EEEE64D9-FF15-49FB-BDF4-706958576FFC";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION)
                .WithElement(new SelectionConfigurationElement(BARS_ELEMENT, Strings.SpectrumBehaviourConfiguration_Bars, path: Strings.SpectrumBehaviourConfiguration_Path).WithOptions(GetBarsOptions()))
                .WithElement(new SelectionConfigurationElement(BANDS_ELEMENT, Strings.SpectrumBehaviourConfiguration_Bands, path: Strings.SpectrumBehaviourConfiguration_Path).WithOptions(GetBandsOptions()))
                .WithElement(new BooleanConfigurationElement(PEAKS_ELEMENT, Strings.SpectrumBehaviourConfiguration_Peaks, path: string.Format("{0}/{1}", Strings.SpectrumBehaviourConfiguration_Path, Strings.General_Advanced)))
                .WithElement(new BooleanConfigurationElement(RMS_ELEMENT, Strings.SpectrumBehaviourConfiguration_Rms, path: string.Format("{0}/{1}", Strings.SpectrumBehaviourConfiguration_Path, Strings.General_Advanced)))
                .WithElement(new BooleanConfigurationElement(CREST_ELEMENT, Strings.SpectrumBehaviourConfiguration_Crest, path: string.Format("{0}/{1}", Strings.SpectrumBehaviourConfiguration_Path, Strings.General_Advanced)).WithValue(false))
                .WithElement(new IntegerConfigurationElement(HOLD_ELEMENT, Strings.SpectrumBehaviourConfiguration_Hold, path: string.Format("{0}/{1}", Strings.SpectrumBehaviourConfiguration_Path, Strings.General_Advanced)).WithValue(1000).WithValidationRule(new IntegerValidationRule(500, 5000)).DependsOn(SECTION, PEAKS_ELEMENT))
                .WithElement(new BooleanConfigurationElement(HIGH_CUT_ELEMENT, Strings.SpectrumBehaviourConfiguration_HighCut, path: string.Format("{0}/{1}", Strings.SpectrumBehaviourConfiguration_Path, Strings.General_Advanced)).WithValue(true)
            );
            ComponentRegistry.Instance.GetComponent<IConfiguration>().GetElement<SelectionConfigurationElement>(
                SECTION,
                VisualizationBehaviourConfiguration.QUALITY_ELEMENT
            ).ConnectValue(option => UpdateConfiguration(option));
        }

        private static void UpdateConfiguration(SelectionConfigurationOption option)
        {
            var configuration = ComponentRegistry.Instance.GetComponent<IConfiguration>();
            var peaks = configuration.GetElement<BooleanConfigurationElement>(
                SECTION,
                PEAKS_ELEMENT
            );
            var rms = configuration.GetElement<BooleanConfigurationElement>(
                SECTION,
                RMS_ELEMENT
            );
            switch (option.Id)
            {
                default:
                case VisualizationBehaviourConfiguration.QUALITY_HIGH_OPTION:
                    Logger.Write(typeof(SpectrumBehaviourConfiguration), LogLevel.Debug, "Using high quality profile.");
                    peaks.Value = true;
                    rms.Value = true;
                    break;
                case VisualizationBehaviourConfiguration.QUALITY_LOW_OPTION:
                    Logger.Write(typeof(SpectrumBehaviourConfiguration), LogLevel.Debug, "Using low quality profile.");
                    peaks.Value = false;
                    rms.Value = false;
                    break;
            }
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
                case BARS_16_OPTION:
                    return 16;
                default:
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

        public static int GetWidthForBars(SelectionConfigurationOption option)
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

        public static int GetWidthForBands(SelectionConfigurationOption option)
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
