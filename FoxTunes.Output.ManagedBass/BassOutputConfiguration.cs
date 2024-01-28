using System.Collections.Generic;

namespace FoxTunes
{
    public static class BassOutputConfiguration
    {
        public const string OUTPUT_SECTION = "8399D051-81BC-41A6-940D-36E6764213D2";

        public const string RATE_ELEMENT = "0452A558-F1ED-41B1-A3DC-95158E01003C";

        public const string RATE_044100_OPTION = "12616570-DDC5-4455-96F2-1D2625EAEC0C";

        public const string RATE_048000_OPTION = "4E96C885-E3F5-425E-9699-5EDE705B9720";

        public const string RATE_088200_OPTION = "4C5ACEDF-7A76-499F-A60B-25B9C6B1BE52";

        public const string RATE_096000_OPTION = "0D5F7382-7F31-4971-9605-FB2B23EA8954";

        public const string RATE_176400_OPTION = "D0CCFBF1-A5D4-4B9F-AD08-2E6E70DEBBCF";

        public const string RATE_192000_OPTION = "3F00BE95-307B-4B80-B8DA-40798889945B";

        public const string DEPTH_ELEMENT = "F535A2A6-DCA0-4E27-9812-498BB2A2C4BC";

        public const string DEPTH_16_OPTION = "768B8466-7582-4B1C-8687-7AB75D636CD8";

        public const string DEPTH_32_OPTION = "889573F2-2E08-4F2B-B94E-DAA945D96497";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(OUTPUT_SECTION, "Output")
                .WithElement(new SelectionConfigurationElement(RATE_ELEMENT, "Rate")
                    .WithOption(new SelectionConfigurationOption(RATE_044100_OPTION, "44100"), true)
                    .WithOption(new SelectionConfigurationOption(RATE_048000_OPTION, "48000"))
                    .WithOption(new SelectionConfigurationOption(RATE_088200_OPTION, "88200"))
                    .WithOption(new SelectionConfigurationOption(RATE_096000_OPTION, "96000"))
                    .WithOption(new SelectionConfigurationOption(RATE_176400_OPTION, "176400"))
                    .WithOption(new SelectionConfigurationOption(RATE_192000_OPTION, "192000")))
                .WithElement(new SelectionConfigurationElement(DEPTH_ELEMENT, "Depth")
                    .WithOption(new SelectionConfigurationOption(DEPTH_16_OPTION, "16bit"), true)
                    .WithOption(new SelectionConfigurationOption(DEPTH_32_OPTION, "32bit floating point"))
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
