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

        public const string SECTION = "B06236E7-F320-4D87-A1A6-9937E0B399BB";

        public const string ENABLED_ELEMENT = "50564810-FD40-4A44-BDF6-F0B2F60E674A";

        public const string BARS_ELEMENT = "AAAA0663-7CBF-4EE4-99C8-A0A096D4E876";

        public const string BARS_16_OPTION = "AAAADF84-DC4C-463E-9A76-D9D424890D91";

        public const string BARS_32_OPTION = "BBBBBA8B-CBA3-4800-B4BE-30D0C0758F7E";

        public const string BARS_64_OPTION = "CCCC9524-BC5A-48C0-8210-921B204307FC";

        public const string BARS_128_OPTION = "DDDD558E-7B9C-4101-992A-709B87756991";

        public const string BARS_256_OPTION = "EEEEFFC1-592E-4EC6-9CCD-5182935AD12E";

        public const string QUALITY_ELEMENT = "BBBBB7B8-FEE1-4D3E-A7EB-D2DF8765EED0";

        public const string QUALITY_HIGH_OPTION = "AAAAF4FD-5A1A-4243-9015-BF76ABDEADBA";

        public const string QUALITY_LOW_OPTION = "BBBB06F5-6F01-4A4F-8674-5074747E2084";

        public const string PEAKS_ELEMENT = "DDDD7FCF-8A71-4367-8F48-4F8D8C89739C";

        public const string HIGH_CUT_ELEMENT = "DDEE2ADA-C904-4AA5-82F0-F53412EF24BD";

        public const string SMOOTH_ELEMENT = "DEEEFBF0-AA25-456E-B759-AF94F6E9C371";

        public const string SMOOTH_FACTOR_ELEMENT = "EDDD7A0-CA10-41F4-ACA0-3EA1C2CD87CB";

        public const string HOLD_ELEMENT = "EEEE64D9-FF15-49FB-BDF4-706958576FFC";

        public const string INTERVAL_ELEMENT = "FFFF5F0C-6574-472A-B9EB-2BDBC1F3C438";

        public const string FFT_SIZE_ELEMENT = "GGGGAE69-551B-4E86-BE04-7EB00AD30099";

        public const string FFT_512_OPTION = "AAAA7106-4174-4A1E-9590-B1798B4187A3";

        public const string FFT_1024_OPTION = "BBBB7106-4174-4A1E-9590-B1798B4187A3";

        public const string FFT_2048_OPTION = "CCCCA066-CABA-4FEF-8987-14B11F16E5AE";

        public const string AMPLITUDE_ELEMENT = "HHHH7D69-3C36-44EB-8960-4147A148F31A";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            var releaseType = StandardComponents.Instance.Configuration.ReleaseType;
            yield return new ConfigurationSection(SECTION, "Spectrum")
                .WithElement(new BooleanConfigurationElement(ENABLED_ELEMENT, "Show In Toolbar").WithValue(releaseType == ReleaseType.Default))
                .WithElement(new SelectionConfigurationElement(BARS_ELEMENT, "Bars").WithOptions(GetBarsOptions()))
                .WithElement(new SelectionConfigurationElement(QUALITY_ELEMENT, "Quality").WithOptions(GetQualityOptions()))
                .WithElement(new BooleanConfigurationElement(PEAKS_ELEMENT, "Peaks", path: "Advanced"))
                .WithElement(new IntegerConfigurationElement(HOLD_ELEMENT, "Peak Hold", path: "Advanced").WithValue(1000).WithValidationRule(new IntegerValidationRule(500, 5000)).DependsOn(SECTION, PEAKS_ELEMENT))
                .WithElement(new BooleanConfigurationElement(HIGH_CUT_ELEMENT, "High Cut", path: "Advanced").WithValue(true))
                .WithElement(new BooleanConfigurationElement(SMOOTH_ELEMENT, "Smooth", path: "Advanced"))
                .WithElement(new IntegerConfigurationElement(SMOOTH_FACTOR_ELEMENT, "Smooth Factor", path: "Advanced").WithValue(10).WithValidationRule(new IntegerValidationRule(1, 100)).DependsOn(SECTION, SMOOTH_ELEMENT))
                .WithElement(new IntegerConfigurationElement(INTERVAL_ELEMENT, "Interval", path: "Advanced").WithValidationRule(new IntegerValidationRule(1, 100)))
                .WithElement(new SelectionConfigurationElement(FFT_SIZE_ELEMENT, "FFT Size", path: "Advanced").WithOptions(GetFFTOptions()))
                .WithElement(new IntegerConfigurationElement(AMPLITUDE_ELEMENT, "Amplitude", path: "Advanced").WithValue(5).WithValidationRule(new IntegerValidationRule(1, 10))
            );
            ComponentRegistry.Instance.GetComponent<IConfiguration>().GetElement<SelectionConfigurationElement>(
                SECTION,
                QUALITY_ELEMENT
            ).ConnectValue(option => UpdateConfiguration(option));
        }

        private static void UpdateConfiguration(SelectionConfigurationOption option)
        {
            var configuration = ComponentRegistry.Instance.GetComponent<IConfiguration>();
            var peaks = configuration.GetElement<BooleanConfigurationElement>(
                SECTION,
                PEAKS_ELEMENT
            );
            var smooth = configuration.GetElement<BooleanConfigurationElement>(
                SECTION,
                SMOOTH_ELEMENT
            );
            var interval = configuration.GetElement<IntegerConfigurationElement>(
                SECTION,
                INTERVAL_ELEMENT
            );
            var fftSize = configuration.GetElement<SelectionConfigurationElement>(
                SECTION,
                FFT_SIZE_ELEMENT
            );
            switch (option.Id)
            {
                default:
                case QUALITY_HIGH_OPTION:
                    Logger.Write(typeof(SpectrumBehaviourConfiguration), LogLevel.Debug, "Using high quality profile.");
                    peaks.Value = true;
                    smooth.Value = true;
                    interval.Value = 20;
                    fftSize.Value = fftSize.GetOption(FFT_2048_OPTION);
                    break;
                case QUALITY_LOW_OPTION:
                    Logger.Write(typeof(SpectrumBehaviourConfiguration), LogLevel.Debug, "Using low quality profile.");
                    peaks.Value = false;
                    smooth.Value = false;
                    interval.Value = 100;
                    fftSize.Value = fftSize.GetOption(FFT_512_OPTION);
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

        private static IEnumerable<SelectionConfigurationOption> GetQualityOptions()
        {
            var releaseType = StandardComponents.Instance.Configuration.ReleaseType;
            var high = new SelectionConfigurationOption(QUALITY_HIGH_OPTION, "High");
            var low = new SelectionConfigurationOption(QUALITY_LOW_OPTION, "Low");
            if (releaseType == ReleaseType.Default)
            {
                high.Default();
            }
            else
            {
                low.Default();
            }
            yield return high;
            yield return low;
        }

        private static IEnumerable<SelectionConfigurationOption> GetFFTOptions()
        {
            yield return new SelectionConfigurationOption(FFT_512_OPTION, "512");
            yield return new SelectionConfigurationOption(FFT_1024_OPTION, "1024");
            yield return new SelectionConfigurationOption(FFT_2048_OPTION, "2048");
        }

        public static int GetFFTSize(SelectionConfigurationOption option)
        {
            switch (option.Id)
            {
                case FFT_512_OPTION:
                    return 256;
                case FFT_1024_OPTION:
                    return 512;
                default:
                case FFT_2048_OPTION:
                    return 1024;
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
