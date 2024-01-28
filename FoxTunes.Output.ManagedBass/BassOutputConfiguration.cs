using System.Collections.Generic;

namespace FoxTunes
{
    public static class BassOutputConfiguration
    {
        public const string SECTION = "8399D051-81BC-41A6-940D-36E6764213D2";

        public const string RATE_ELEMENT = "0452A558-F1ED-41B1-A3DC-95158E01003C";

        public const string RATE_044100_OPTION = "12616570-DDC5-4455-96F2-1D2625EAEC0C";

        public const string RATE_048000_OPTION = "4E96C885-E3F5-425E-9699-5EDE705B9720";

        public const string RATE_088200_OPTION = "4C5ACEDF-7A76-499F-A60B-25B9C6B1BE52";

        public const string RATE_096000_OPTION = "0D5F7382-7F31-4971-9605-FB2B23EA8954";

        public const string RATE_176400_OPTION = "D0CCFBF1-A5D4-4B9F-AD08-2E6E70DEBBCF";

        public const string RATE_192000_OPTION = "3F00BE95-307B-4B80-B8DA-40798889945B";

        public const string RATE_352800_OPTION = "3CF9FC05-A7A3-4B5B-A3BD-CA27625DF3D5";

        public const string RATE_384000_OPTION = "61A9BAC3-FFF3-43D7-A126-F5E2049FE97C";

        public const string ENFORCE_RATE_ELEMENT = "1C5E5B16-1B49-4C50-A8CF-BE3A6CCD4A87";

        public const string DEPTH_ELEMENT = "F535A2A6-DCA0-4E27-9812-498BB2A2C4BC";

        public const string DEPTH_16_OPTION = "768B8466-7582-4B1C-8687-7AB75D636CD8";

        public const string DEPTH_32_OPTION = "889573F2-2E08-4F2B-B94E-DAA945D96497";

        public const string MODE_ELEMENT = "76096B39-2F8A-4667-9C03-9742FF2D1EA7";

        public const string PLAY_FROM_RAM_ELEMENT = "AFE6BED1-7965-47A3-9798-E46124386A8D";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION, "Output")
                .WithElement(new SelectionConfigurationElement(RATE_ELEMENT, "Rate", path: "Advanced")
                    .WithOption(new SelectionConfigurationOption(RATE_044100_OPTION, "44100"), true)
                    .WithOption(new SelectionConfigurationOption(RATE_048000_OPTION, "48000"))
                    .WithOption(new SelectionConfigurationOption(RATE_088200_OPTION, "88200"))
                    .WithOption(new SelectionConfigurationOption(RATE_096000_OPTION, "96000"))
                    .WithOption(new SelectionConfigurationOption(RATE_176400_OPTION, "176400"))
                    .WithOption(new SelectionConfigurationOption(RATE_192000_OPTION, "192000"))
                    .WithOption(new SelectionConfigurationOption(RATE_352800_OPTION, "352800"))
                    .WithOption(new SelectionConfigurationOption(RATE_384000_OPTION, "384000")))
                .WithElement(new SelectionConfigurationElement(DEPTH_ELEMENT, "Depth", path: "Advanced")
                    .WithOption(new SelectionConfigurationOption(DEPTH_16_OPTION, "16bit"), true)
                    .WithOption(new SelectionConfigurationOption(DEPTH_32_OPTION, "32bit floating point")))
                .WithElement(new SelectionConfigurationElement(MODE_ELEMENT, "Mode"))
                .WithElement(new BooleanConfigurationElement(ENFORCE_RATE_ELEMENT, "Enforce Rate", path: "Advanced").WithValue(false))
                .WithElement(new BooleanConfigurationElement(PLAY_FROM_RAM_ELEMENT, "Play From Memory").WithValue(false)
            );
        }

        public static int GetRate(string value)
        {
            switch (value)
            {
                default:
                case RATE_044100_OPTION:
                    return 44100;
                case RATE_048000_OPTION:
                    return 48000;
                case RATE_088200_OPTION:
                    return 88200;
                case RATE_096000_OPTION:
                    return 96000;
                case RATE_176400_OPTION:
                    return 176400;
                case RATE_192000_OPTION:
                    return 192000;
                case RATE_352800_OPTION:
                    return 352800;
                case RATE_384000_OPTION:
                    return 384000;
            }
        }

        public static bool GetFloat(string value)
        {
            switch (value)
            {
                default:
                case DEPTH_16_OPTION:
                    return false;
                case DEPTH_32_OPTION:
                    return true;
            }
        }
    }
}
