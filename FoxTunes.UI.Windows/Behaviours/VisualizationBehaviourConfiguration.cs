using FoxTunes.ViewModel;
using System.Collections.Generic;

namespace FoxTunes
{
    public static class VisualizationBehaviourConfiguration
    {
        public const string SECTION = "B06236E7-F320-4D87-A1A6-9937E0B399BB";

        public const string INTERVAL_ELEMENT = "FFFF5F0C-6574-472A-B9EB-2BDBC1F3C438";

        public const int MIN_INTERVAL = 5;

        public const int MAX_INTERVAL = 40;

        public const int DEFAULT_INTERVAL = 16;

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
                .WithElement(new IntegerConfigurationElement(INTERVAL_ELEMENT, Strings.VisualizationBehaviourConfiguration_Interval, path: Strings.General_Advanced).WithValue(DEFAULT_INTERVAL).WithValidationRule(new IntegerValidationRule(MIN_INTERVAL, MAX_INTERVAL)))
                .WithElement(new SelectionConfigurationElement(FFT_SIZE_ELEMENT, Strings.VisualizationBehaviourConfiguration_FFTSize, path: Strings.General_Advanced).WithOptions(GetFFTOptions())
            );
        }

        private static IEnumerable<SelectionConfigurationOption> GetFFTOptions()
        {
            yield return new SelectionConfigurationOption(FFT_512_OPTION, "512");
            yield return new SelectionConfigurationOption(FFT_1024_OPTION, "1024");
            yield return new SelectionConfigurationOption(FFT_2048_OPTION, "2048").Default();
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
