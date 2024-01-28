using System.Collections.Generic;

namespace FoxTunes
{
    public static class BassParametricEqualizerStreamComponentConfiguration
    {
        public const string ENABLED_ELEMENT = "AAAAF34F-A090-4AEE-BD65-128561960C92";

        public const string BAND_32 = "BBBBC109-B99A-49C1-909D-16A862EFF344";

        public const string BAND_64 = "CCCCDD9E-7430-4123-B7CA-FDDD7680EF2C";

        public const string BAND_125 = "DDDD4934-28A4-4841-BC31-9BB895B13A14";

        public const string BAND_250 = "EEEE45E7-0B32-4E73-B394-2D510A34FB05";

        public const string BAND_500 = "FFFFD2A0-40A7-4772-9829-48D7C8CDCF52";

        public const string BAND_1000 = "GGGGEFB4-F65D-444F-A021-5B8F0B643AC5";

        public const string BAND_2000 = "HHHHB649-B83F-48D0-AF02-FA127FFC9F04";

        public const string BAND_4000 = "IIIIE3B6-1026-4505-8C77-294185F61452";

        public const string BAND_8000 = "JJJJAADF-5286-46BA-AC4F-80CFB1115215";

        public const string BAND_16000 = "KKKKD105-FC44-40F4-AF85-972CF7F328E9";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            var section = new ConfigurationSection(BassOutputConfiguration.SECTION, "Output")
                .WithElement(new BooleanConfigurationElement(ENABLED_ELEMENT, "Enabled", path: "Parametric Equalizer").WithValue(false));

            foreach (var band in Bands)
            {
                section.WithElement(
                    new IntegerConfigurationElement(band.Key, GetBandName(band.Value), path: "Parametric Equalizer")
                        .WithValue(0)
                        .WithValidationRule(
                            new IntegerValidationRule(
                                BassParametricEqualizerStreamComponent.MIN_GAIN,
                                BassParametricEqualizerStreamComponent.MAX_GAIN
                            )
                        )
                );
            }

            yield return section;

            StandardComponents.Instance.Configuration.GetElement<BooleanConfigurationElement>(BassOutputConfiguration.SECTION, ENABLED_ELEMENT).ConnectValue(value => UpdateConfiguration(value));
        }

        private static string GetBandName(int value)
        {
            if (value < 1000)
            {
                return string.Format("{0}Hz", value);
            }
            else
            {
                return string.Format("{0}kHz", value / 1000);
            }
        }

        private static void UpdateConfiguration(bool enabled)
        {
            foreach (var band in Bands)
            {
                var element = StandardComponents.Instance.Configuration.GetElement(
                    BassOutputConfiguration.SECTION,
                    band.Key
                );
                if (element == null)
                {
                    continue;
                }
                if (enabled)
                {
                    element.Show();
                }
                else
                {
                    element.Hide();
                }
            }
        }

        public static IEnumerable<KeyValuePair<string, int>> Bands
        {
            get
            {
                yield return new KeyValuePair<string, int>(BAND_32, 32);
                yield return new KeyValuePair<string, int>(BAND_64, 64);
                yield return new KeyValuePair<string, int>(BAND_125, 125);
                yield return new KeyValuePair<string, int>(BAND_250, 250);
                yield return new KeyValuePair<string, int>(BAND_500, 500);
                yield return new KeyValuePair<string, int>(BAND_1000, 1000);
                yield return new KeyValuePair<string, int>(BAND_2000, 2000);
                yield return new KeyValuePair<string, int>(BAND_4000, 4000);
                yield return new KeyValuePair<string, int>(BAND_8000, 8000);
                yield return new KeyValuePair<string, int>(BAND_16000, 16000);
            }
        }
    }
}
