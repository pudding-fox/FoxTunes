using System.Collections.Generic;

namespace FoxTunes
{
    public static class Log4NetLoggerConfiguration
    {
        public const string SECTION = "6D2403BD-7CBD-4690-A119-90F4A45B00CD";

        public const string DEFAULT_APPENDER_ELEMENT = "A673B653-D5CA-40C0-A722-FEB38492150F";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION, "Logging")
                .WithElement(
                    new BooleanConfigurationElement(DEFAULT_APPENDER_ELEMENT, "Default Appender").WithValue(true)
                );
        }
    }
}
