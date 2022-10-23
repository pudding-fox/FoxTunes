using FoxTunes.Interfaces;
using System.Collections.Generic;

namespace FoxTunes
{
    public static class VisualizationBehaviourConfiguration
    {
        private static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        public const string SECTION = "B06236E7-F320-4D87-A1A6-9937E0B399BB";

        public const string QUALITY_ELEMENT = "BBBBB7B8-FEE1-4D3E-A7EB-D2DF8765EED0";

        public const string QUALITY_HIGH_OPTION = "AAAAF4FD-5A1A-4243-9015-BF76ABDEADBA";

        public const string QUALITY_LOW_OPTION = "BBBB06F5-6F01-4A4F-8674-5074747E2084";

        public const string SMOOTH_ELEMENT = "DEEEFBF0-AA25-456E-B759-AF94F6E9C371";

        public const string SMOOTH_FACTOR_ELEMENT = "EDDD7A0-CA10-41F4-ACA0-3EA1C2CD87CB";

        public const string INTERVAL_ELEMENT = "FFFF5F0C-6574-472A-B9EB-2BDBC1F3C438";

        public const string FFT_SIZE_ELEMENT = "GGGGAE69-551B-4E86-BE04-7EB00AD30099";

        public const string FFT_512_OPTION = "AAAA7106-4174-4A1E-9590-B1798B4187A3";

        public const string FFT_1024_OPTION = "BBBB7106-4174-4A1E-9590-B1798B4187A3";

        public const string FFT_2048_OPTION = "CCCC7106-4174-4A1E-9590-B1798B4187A3";

        public const string FFT_4096_OPTION = "DDDD7106-4174-4A1E-9590-B1798B4187A3";

        public const string FFT_8192_OPTION = "EEEE7106 - 4174 - 4A1E-9590-B1798B4187A3";

        public const string FFT_16384_OPTION = "FFFF7106-4174-4A1E-9590-B1798B4187A3";

        public const string FFT_32768_OPTION = "GGGG7106-4174-4A1E-9590-B1798B4187A3";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION, Strings.VisualizationBehaviourConfiguration_Section)
                .WithElement(new SelectionConfigurationElement(QUALITY_ELEMENT, Strings.VisualizationBehaviourConfiguration_Quality).WithOptions(GetQualityOptions()))
                .WithElement(new BooleanConfigurationElement(SMOOTH_ELEMENT, Strings.VisualizationBehaviourConfiguration_Smooth, path: Strings.General_Advanced))
                .WithElement(new IntegerConfigurationElement(SMOOTH_FACTOR_ELEMENT, Strings.VisualizationBehaviourConfiguration_SmoothFactor, path: Strings.General_Advanced).WithValue(10).WithValidationRule(new IntegerValidationRule(1, 100)).DependsOn(SECTION, SMOOTH_ELEMENT))
                .WithElement(new IntegerConfigurationElement(INTERVAL_ELEMENT, Strings.VisualizationBehaviourConfiguration_Interval, path: Strings.General_Advanced).WithValidationRule(new IntegerValidationRule(1, 100)))
                .WithElement(new SelectionConfigurationElement(FFT_SIZE_ELEMENT, Strings.VisualizationBehaviourConfiguration_FFTSize, path: Strings.General_Advanced).WithOptions(GetFFTOptions())
            );
            ComponentRegistry.Instance.GetComponent<IConfiguration>().GetElement<SelectionConfigurationElement>(
                SECTION,
                QUALITY_ELEMENT
            ).ConnectValue(option => UpdateConfiguration(option));
        }

        private static void UpdateConfiguration(SelectionConfigurationOption option)
        {
            var configuration = ComponentRegistry.Instance.GetComponent<IConfiguration>();
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
                    Logger.Write(typeof(VisualizationBehaviourConfiguration), LogLevel.Debug, "Using high quality profile.");
                    smooth.Value = true;
                    interval.Value = 16; //60 fps.
                    fftSize.Value = fftSize.GetOption(FFT_2048_OPTION);
                    break;
                case QUALITY_LOW_OPTION:
                    Logger.Write(typeof(VisualizationBehaviourConfiguration), LogLevel.Debug, "Using low quality profile.");
                    smooth.Value = false;
                    interval.Value = 100; //10 fps.
                    fftSize.Value = fftSize.GetOption(FFT_512_OPTION);
                    break;
            }
        }

        private static IEnumerable<SelectionConfigurationOption> GetQualityOptions()
        {
            var high = new SelectionConfigurationOption(QUALITY_HIGH_OPTION, Strings.VisualizationBehaviourConfiguration_Quality_High);
            var low = new SelectionConfigurationOption(QUALITY_LOW_OPTION, Strings.VisualizationBehaviourConfiguration_Quality_Low);
            if (Publication.ReleaseType == ReleaseType.Default)
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
            yield return new SelectionConfigurationOption(FFT_4096_OPTION, "4096");
            yield return new SelectionConfigurationOption(FFT_8192_OPTION, "8192");
            yield return new SelectionConfigurationOption(FFT_16384_OPTION, "16384");
            yield return new SelectionConfigurationOption(FFT_32768_OPTION, "32768");
        }

        public static int GetFFTSize(SelectionConfigurationOption option)
        {
            switch (option.Id)
            {
                case FFT_512_OPTION:
                    return 512;
                case FFT_1024_OPTION:
                    return 1024;
                default:
                case FFT_2048_OPTION:
                    return 2048;
                case FFT_4096_OPTION:
                    return 4096;
                case FFT_8192_OPTION:
                    return 8192;
                case FFT_16384_OPTION:
                    return 16384;
                case FFT_32768_OPTION:
                    return 32768;
            }
        }

    }
}
