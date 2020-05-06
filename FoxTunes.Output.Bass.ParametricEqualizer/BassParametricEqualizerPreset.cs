using System.Collections.Generic;

namespace FoxTunes
{
    public static class BassParametricEqualizerPreset
    {
        public const string PRESET_NONE = "None";

        public const string PRESET_BASS = "Bass";

        public const string PRESET_FLAT = "Flat";

        public const string PRESET_POP = "Pop";

        public const string PRESET_ROCK = "Rock";

        public static IEnumerable<string> Presets
        {
            get
            {
                yield return PRESET_NONE;
                yield return PRESET_BASS;
                yield return PRESET_FLAT;
                yield return PRESET_POP;
                yield return PRESET_ROCK;
            }
        }

        public static void LoadPreset(string name)
        {
            var bands = default(Dictionary<string, int>);
            switch (name)
            {
                case PRESET_NONE:
                    bands = new Dictionary<string, int>()
                    {
                        { BassParametricEqualizerStreamComponentConfiguration.BAND_32, 0 },
                        { BassParametricEqualizerStreamComponentConfiguration.BAND_64, 0 },
                        { BassParametricEqualizerStreamComponentConfiguration.BAND_125, 0 },
                        { BassParametricEqualizerStreamComponentConfiguration.BAND_250, 0 },
                        { BassParametricEqualizerStreamComponentConfiguration.BAND_500, 0 },
                        { BassParametricEqualizerStreamComponentConfiguration.BAND_1000, 0 },
                        { BassParametricEqualizerStreamComponentConfiguration.BAND_2000, 0 },
                        { BassParametricEqualizerStreamComponentConfiguration.BAND_4000, 0 },
                        { BassParametricEqualizerStreamComponentConfiguration.BAND_8000, 0 },
                        { BassParametricEqualizerStreamComponentConfiguration.BAND_16000, 0 }
                    };
                    break;
                case PRESET_BASS:
                    bands = new Dictionary<string, int>()
                    {
                        { BassParametricEqualizerStreamComponentConfiguration.BAND_32, 9 },
                        { BassParametricEqualizerStreamComponentConfiguration.BAND_64, 6 },
                        { BassParametricEqualizerStreamComponentConfiguration.BAND_125, 2 },
                        { BassParametricEqualizerStreamComponentConfiguration.BAND_250, 0 },
                        { BassParametricEqualizerStreamComponentConfiguration.BAND_500, -1 },
                        { BassParametricEqualizerStreamComponentConfiguration.BAND_1000, -1 },
                        { BassParametricEqualizerStreamComponentConfiguration.BAND_2000, 0 },
                        { BassParametricEqualizerStreamComponentConfiguration.BAND_4000, 0 },
                        { BassParametricEqualizerStreamComponentConfiguration.BAND_8000, 0 },
                        { BassParametricEqualizerStreamComponentConfiguration.BAND_16000, 0 }
                    };
                    break;
                case PRESET_FLAT:
                    bands = new Dictionary<string, int>()
                    {
                        { BassParametricEqualizerStreamComponentConfiguration.BAND_32, -5 },
                        { BassParametricEqualizerStreamComponentConfiguration.BAND_64, -5 },
                        { BassParametricEqualizerStreamComponentConfiguration.BAND_125, -2 },
                        { BassParametricEqualizerStreamComponentConfiguration.BAND_250, 0 },
                        { BassParametricEqualizerStreamComponentConfiguration.BAND_500, 2 },
                        { BassParametricEqualizerStreamComponentConfiguration.BAND_1000, 3 },
                        { BassParametricEqualizerStreamComponentConfiguration.BAND_2000, 3 },
                        { BassParametricEqualizerStreamComponentConfiguration.BAND_4000, 3 },
                        { BassParametricEqualizerStreamComponentConfiguration.BAND_8000, 3 },
                        { BassParametricEqualizerStreamComponentConfiguration.BAND_16000, 3 }
                    };
                    break;
                case PRESET_POP:
                    bands = new Dictionary<string, int>()
                    {
                        { BassParametricEqualizerStreamComponentConfiguration.BAND_32, -1 },
                        { BassParametricEqualizerStreamComponentConfiguration.BAND_64, 1 },
                        { BassParametricEqualizerStreamComponentConfiguration.BAND_125, 3 },
                        { BassParametricEqualizerStreamComponentConfiguration.BAND_250, 4 },
                        { BassParametricEqualizerStreamComponentConfiguration.BAND_500, 4 },
                        { BassParametricEqualizerStreamComponentConfiguration.BAND_1000, 3 },
                        { BassParametricEqualizerStreamComponentConfiguration.BAND_2000, 2 },
                        { BassParametricEqualizerStreamComponentConfiguration.BAND_4000, 1 },
                        { BassParametricEqualizerStreamComponentConfiguration.BAND_8000, 2 },
                        { BassParametricEqualizerStreamComponentConfiguration.BAND_16000, 4 }
                    };
                    break;
                case PRESET_ROCK:
                    bands = new Dictionary<string, int>()
                    {
                        { BassParametricEqualizerStreamComponentConfiguration.BAND_32, 5 },
                        { BassParametricEqualizerStreamComponentConfiguration.BAND_64, 3 },
                        { BassParametricEqualizerStreamComponentConfiguration.BAND_125, 0 },
                        { BassParametricEqualizerStreamComponentConfiguration.BAND_250, 0 },
                        { BassParametricEqualizerStreamComponentConfiguration.BAND_500, 0 },
                        { BassParametricEqualizerStreamComponentConfiguration.BAND_1000, 3 },
                        { BassParametricEqualizerStreamComponentConfiguration.BAND_2000, 4 },
                        { BassParametricEqualizerStreamComponentConfiguration.BAND_4000, 4 },
                        { BassParametricEqualizerStreamComponentConfiguration.BAND_8000, 3 },
                        { BassParametricEqualizerStreamComponentConfiguration.BAND_16000, 3 }
                    };
                    break;
            }
            foreach (var band in BassParametricEqualizerStreamComponentConfiguration.Bands)
            {
                var element = StandardComponents.Instance.Configuration.GetElement<DoubleConfigurationElement>(
                    BassOutputConfiguration.SECTION,
                    band.Key
                );
                if (element == null)
                {
                    continue;
                }
                element.Value = bands[band.Key];
            }
            StandardComponents.Instance.Configuration.Save();
        }
    }
}
