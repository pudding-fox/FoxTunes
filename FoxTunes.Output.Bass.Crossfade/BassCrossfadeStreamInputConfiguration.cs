using ManagedBass.Crossfade;
using System;
using System.Collections.Generic;

namespace FoxTunes
{
    public static class BassCrossfadeStreamInputConfiguration
    {
        public const string INPUT_CROSSFADE_OPTION = "BBBBFDE4-7361-456F-AB55-85364B51C2A2";

        public const string MODE_ELEMENT = "AAAA0393-436D-4500-9E4C-D7A3CAE8AC28";

        public const string MODE_ALWAYS_OPTION = "AAAA51B8-2F7B-46BB-BA0F-7D71316055A0";

        public const string MODE_MANUAL_OPTION = "BBBB2352-F7A9-40AF-A654-C76BB6C59A6B";

        public const string PERIOD_IN_ELEMENT = "BBBB69B8-E637-4B00-B46C-37BEAA9873F8";

        public const string PERIOD_OUT_ELEMENT = "CCCC079C-98AB-4773-B55F-E2C91CDDE0BE";

        public const string TYPE_IN_ELEMENT = "DDDDD685-3F5A-44C3-8286-8ED88F5B385E";

        public const string TYPE_OUT_ELEMENT = "EEEEB3EF-2B3A-4CB8-93B5-946A34F9FEF5";

        public const string TYPE_LINEAR_OPTION = "AAAA6A54-B0D9-4CFA-9AEC-4AE730D62716";

        public const string TYPE_LOGARITHMIC_OPTION = "BBBBC8C7-FFE1-4522-AAC6-17D9F2E2638A";

        public const string TYPE_EXPONENTIAL_OPTION = "CCCC416A-08A4-4255-87CF-B4DF467A2285";

        public const string TYPE_EASE_IN_OPTION = "DDDD083D-8F1D-460C-A557-66DD71366F5C";

        public const string TYPE_EASE_OUT_OPTION = "EEEED4F2-9E6E-4499-8D36-F219AB45AC85";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(BassOutputConfiguration.SECTION, "Input")
                .WithElement(new SelectionConfigurationElement(BassOutputConfiguration.INPUT_ELEMENT, "Transport")
                    .WithOptions(new[] { new SelectionConfigurationOption(INPUT_CROSSFADE_OPTION, "Crossfade") }))
                .WithElement(new SelectionConfigurationElement(MODE_ELEMENT, "Mode", path: "Crossfade").WithOptions(GetModeOptions()))
                .WithElement(new IntegerConfigurationElement(PERIOD_IN_ELEMENT, "Fade In Period", path: "Crossfade").WithValue(100).WithValidationRule(new IntegerValidationRule(0, 5000, step: 100)))
                .WithElement(new IntegerConfigurationElement(PERIOD_OUT_ELEMENT, "Fade Out Period", path: "Crossfade").WithValue(100).WithValidationRule(new IntegerValidationRule(0, 5000, step: 100)))
                .WithElement(new SelectionConfigurationElement(TYPE_IN_ELEMENT, "Fade In Curve", path: "Crossfade").WithOptions(GetTypeOptions(TYPE_EASE_IN_OPTION)))
                .WithElement(new SelectionConfigurationElement(TYPE_OUT_ELEMENT, "Fade Out Curve", path: "Crossfade").WithOptions(GetTypeOptions(TYPE_EASE_OUT_OPTION))
            );
        }

        private static IEnumerable<SelectionConfigurationOption> GetModeOptions()
        {
            yield return new SelectionConfigurationOption(MODE_ALWAYS_OPTION, "Always");
            yield return new SelectionConfigurationOption(MODE_MANUAL_OPTION, "Manual").Default();
        }

        public static BassCrossfadeMode GetMode(SelectionConfigurationOption option)
        {
            switch (option.Id)
            {
                case MODE_ALWAYS_OPTION:
                    return BassCrossfadeMode.Always;
                default:
                case MODE_MANUAL_OPTION:
                    return BassCrossfadeMode.Manual;
            }
        }

        private static IEnumerable<SelectionConfigurationOption> GetTypeOptions(string value)
        {
            var options = new[]
            {
                new SelectionConfigurationOption(TYPE_LINEAR_OPTION, "Linear"),
                new SelectionConfigurationOption(TYPE_LOGARITHMIC_OPTION, "Logarithmic"),
                new SelectionConfigurationOption(TYPE_EXPONENTIAL_OPTION, "Exponential"),
                new SelectionConfigurationOption(TYPE_EASE_IN_OPTION, "Ease In"),
                new SelectionConfigurationOption(TYPE_EASE_OUT_OPTION, "Ease Out")
            };
            foreach (var option in options)
            {
                if (string.Equals(option.Id, value, StringComparison.OrdinalIgnoreCase))
                {
                    option.Default();
                }
                yield return option;
            }
        }

        public static BassCrossfadeType GetType(SelectionConfigurationOption option)
        {
            switch (option.Id)
            {
                case TYPE_LINEAR_OPTION:
                    return BassCrossfadeType.Linear;
                case TYPE_LOGARITHMIC_OPTION:
                    return BassCrossfadeType.Logarithmic;
                case TYPE_EXPONENTIAL_OPTION:
                    return BassCrossfadeType.Exponential;
                default:
                case TYPE_EASE_IN_OPTION:
                    return BassCrossfadeType.EaseIn;
                case TYPE_EASE_OUT_OPTION:
                    return BassCrossfadeType.EaseOut;
            }
        }
    }
}
