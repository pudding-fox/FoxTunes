using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace FoxTunes
{
    public static class EnhancedSpectrumConfiguration
    {
        public const string SECTION = VisualizationBehaviourConfiguration.SECTION;

        public const string BANDS_ELEMENT = "AABBF573-83D3-498E-BEF8-F1DB5A329B9D";

        public const string BANDS_10_OPTION = "AAAA058C-2C96-4540-9ABE-10A584A17CE4";

        public const string BANDS_20_OPTION = "BBBB2B6D-E6FE-43F1-8358-AEE0299F0F8E";

        public const string BANDS_40_OPTION = "EEEE9CF3-A711-45D7-BFAC-A72E769106B9";

        public const string BANDS_80_OPTION = "FFFF83BC-5871-491D-963E-3D12554FF4BE";

        public const string BANDS_160_OPTION = "GGGG5E28-CC67-43F2-8778-61570785C766";

        public const string PEAK_ELEMENT = "BBBBDCF0-8B24-4321-B7BE-74DADE59D4FA";

        public const string RMS_ELEMENT = "DDDEE2B6A-188E-4FF4-A277-37D140D49C45";

        public const string CREST_ELEMENT = "DEEEFFB9-2014-4004-94F9-E566874317ED";

        public const string COLOR_PALETTE_ELEMENT = "EEEE907A-5812-42CD-9844-89362C96C6AF";

        public const string COLOR_PALETTE_THEME = "THEME";

        public const string COLOR_PALETTE_PEAK = "PEAK";

        public const string COLOR_PALETTE_RMS = "RMS";

        public const string COLOR_PALETTE_VALUE = "VALUE";

        public const string COLOR_PALETTE_CREST = "CREST";

        public const string COLOR_PALETTE_BACKGROUND = "BACKGROUND";

        public const string DURATION_ELEMENT = "FFFF965B-101C-4A09-9A9A-91BAB17575E6";

        public const int DURATION_MIN = 16;

        public const int DURATION_MAX = 64;

        public const int DURATION_DEFAULT = 32;

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION)
                .WithElement(new SelectionConfigurationElement(BANDS_ELEMENT, Strings.EnhancedSpectrumConfiguration_Bands, path: Strings.EnhancedSpectrumConfiguration_Path).WithOptions(GetBandsOptions()))
                .WithElement(new BooleanConfigurationElement(PEAK_ELEMENT, Strings.EnhancedSpectrumConfiguration_Peak, path: string.Format("{0}/{1}", Strings.EnhancedSpectrumConfiguration_Path, Strings.General_Advanced)).WithValue(true))
                .WithElement(new BooleanConfigurationElement(RMS_ELEMENT, Strings.EnhancedSpectrumConfiguration_Rms, path: string.Format("{0}/{1}", Strings.EnhancedSpectrumConfiguration_Path, Strings.General_Advanced)).WithValue(true))
                .WithElement(new BooleanConfigurationElement(CREST_ELEMENT, Strings.EnhancedSpectrumConfiguration_Crest, path: string.Format("{0}/{1}", Strings.EnhancedSpectrumConfiguration_Path, Strings.General_Advanced)).WithValue(false).DependsOn(SECTION, PEAK_ELEMENT).DependsOn(SECTION, RMS_ELEMENT))
                .WithElement(new TextConfigurationElement(COLOR_PALETTE_ELEMENT, Strings.EnhancedSpectrumConfiguration_ColorPalette, path: string.Format("{0}/{1}", Strings.EnhancedSpectrumConfiguration_Path, Strings.General_Advanced)).WithValue(GetDefaultColorPalette()).WithFlags(ConfigurationElementFlags.MultiLine))
                .WithElement(new IntegerConfigurationElement(DURATION_ELEMENT, Strings.EnhancedSpectrumConfiguration_Duration, path: Strings.EnhancedSpectrumConfiguration_Path).WithValue(DURATION_DEFAULT).WithValidationRule(new IntegerValidationRule(DURATION_MIN, DURATION_MAX))
            );
        }

        private static IEnumerable<SelectionConfigurationOption> GetBandsOptions()
        {
            yield return new SelectionConfigurationOption(BANDS_10_OPTION, "10");
            yield return new SelectionConfigurationOption(BANDS_20_OPTION, "20");
            yield return new SelectionConfigurationOption(BANDS_40_OPTION, "40").Default();
            yield return new SelectionConfigurationOption(BANDS_80_OPTION, "80");
            yield return new SelectionConfigurationOption(BANDS_160_OPTION, "160");
        }

        private static IDictionary<string, int[]> Bands = new Dictionary<string, int[]>(StringComparer.OrdinalIgnoreCase)
        {
            { BANDS_10_OPTION, new[] {
                    50,
                    94,
                    176,
                    331,
                    630,
                    1200,
                    2200,
                    4100,
                    7700,
                    14000
                }
            },
            { BANDS_20_OPTION, new[] {
                    50,
                    69,
                    94,
                    129,
                    176,
                    241,
                    331,
                    453,
                    620,
                    850,
                    1200,
                    1600,
                    2200,
                    3000,
                    4100,
                    5600,
                    7700,
                    11000,
                    14000,
                    20000
                }
            },
            { BANDS_40_OPTION, new[] {
                    50,
                    59,
                    69,
                    80,
                    94,
                    110,
                    129,
                    150,
                    176,
                    206,
                    241,
                    282,
                    331,
                    387,
                    453,
                    530,
                    620,
                    726,
                    850,
                    1000,
                    1200,
                    1400,
                    1600,
                    1900,
                    2200,
                    2600,
                    3000,
                    3500,
                    4100,
                    4800,
                    5600,
                    6600,
                    7700,
                    9000,
                    11000,
                    12000,
                    14000,
                    17000,
                    20000,
                    23000
                }
            },
            { BANDS_80_OPTION,  new[] {
                    50,
                    54,
                    59,
                    63,
                    69,
                    74,
                    80,
                    87,
                    94,
                    102,
                    110,
                    119,
                    129,
                    139,
                    150,
                    163,
                    176,
                    191,
                    206,
                    223,
                    241,
                    261,
                    282,
                    306,
                    331,
                    358,
                    387,
                    419,
                    453,
                    490,
                    530,
                    574,
                    620,
                    671,
                    726,
                    786,
                    850,
                    920,
                    1000,
                    1100,
                    1200,
                    1300,
                    1400,
                    1500,
                    1600,
                    1700,
                    1900,
                    2000,
                    2200,
                    2400,
                    2600,
                    2800,
                    3000,
                    3200,
                    3500,
                    3800,
                    4100,
                    4400,
                    4800,
                    5200,
                    5600,
                    6100,
                    6600,
                    7100,
                    7700,
                    8300,
                    9000,
                    10000,
                    11000,
                    11500,
                    12000,
                    13000,
                    14000,
                    16000,
                    17000,
                    18000,
                    20000,
                    21000,
                    23000,
                    25000
                }
            },
            { BANDS_160_OPTION, new[] {
                    50,
                    52,
                    54,
                    56,
                    59,
                    61,
                    63,
                    66,
                    69,
                    71,
                    77,
                    80,
                    83,
                    87,
                    90,
                    94,
                    98,
                    102,
                    106,
                    110,
                    114,
                    119,
                    124,
                    129,
                    134,
                    139,
                    145,
                    150,
                    157,
                    163,
                    169,
                    176,
                    183,
                    191,
                    198,
                    206,
                    214,
                    223,
                    232,
                    241,
                    251,
                    261,
                    272,
                    282,
                    294,
                    306,
                    318,
                    331,
                    344,
                    358,
                    372,
                    387,
                    402,
                    419,
                    435,
                    453,
                    453,
                    471,
                    490,
                    510,
                    530,
                    551,
                    574,
                    597,
                    620,
                    645,
                    671,
                    698,
                    726,
                    755,
                    786,
                    817,
                    850,
                    884,
                    920,
                    1000,
                    1030,
                    1060,
                    1100,
                    1150,
                    1200,
                    1250,
                    1300,
                    1350,
                    1400,
                    1450,
                    1500,
                    1550,
                    1600,
                    1700,
                    1750,
                    1800,
                    1900,
                    1950,
                    2000,
                    2100,
                    2200,
                    2300,
                    2400,
                    2500,
                    2600,
                    2700,
                    2800,
                    2900,
                    3000,
                    3100,
                    3200,
                    3400,
                    3500,
                    3600,
                    3800,
                    3900,
                    4100,
                    4300,
                    4400,
                    4600,
                    4800,
                    5000,
                    5200,
                    5400,
                    5600,
                    5800,
                    6100,
                    6300,
                    6600,
                    6800,
                    7100,
                    7400,
                    7700,
                    8000,
                    8300,
                    8700,
                    9000,
                    9400,
                    10000,
                    10500,
                    11000,
                    11300,
                    11600,
                    12000,
                    12500,
                    13000,
                    13500,
                    14000,
                    14500,
                    15000,
                    16000,
                    16500,
                    17000,
                    18000,
                    18500,
                    19000,
                    20000,
                    21000,
                    21500,
                    22000,
                    23000,
                    24000,
                    25000,
                    26000
                }
            }
        };

        public static int[] GetBands(SelectionConfigurationOption option)
        {
            var bands = default(int[]);
            if (!Bands.TryGetValue(option.Id, out bands))
            {
                bands = Bands[BANDS_20_OPTION];
            }
            return bands;
        }

        public static string GetDefaultColorPalette()
        {
            var builder = new StringBuilder();
            return builder.ToString();
        }

        public static IDictionary<string, Color[]> GetColorPalette(string value, Color[] colors)
        {
            if (!string.IsNullOrEmpty(value))
            {
                try
                {
                    var palettes = value.ToNamedColorStops().ToDictionary(
                        pair => string.IsNullOrEmpty(pair.Key) ? COLOR_PALETTE_VALUE : pair.Key,
                        pair => pair.Value.ToGradient(),
                        StringComparer.OrdinalIgnoreCase
                    );
                    palettes[COLOR_PALETTE_THEME] = colors;
                    return palettes;
                }
                catch
                {
                    //Nothing can be done.
                }
            }
            return new Dictionary<string, Color[]>(StringComparer.OrdinalIgnoreCase)
            {
                { COLOR_PALETTE_THEME, colors }
            };
        }

        public static int GetFFTSize(SelectionConfigurationOption fftSize, SelectionConfigurationOption bands)
        {
            var size = VisualizationBehaviourConfiguration.GetFFTSize(fftSize);
            var count = GetBands(bands).Length;
            //More bands requires more FFT bins, increase if required.
            switch (count)
            {
                case 40:
                case 80:
                    size = Math.Max(size, 8192);
                    break;
                case 160:
                    size = Math.Max(size, 16384);
                    break;
            }
            return size;
        }
    }
}
