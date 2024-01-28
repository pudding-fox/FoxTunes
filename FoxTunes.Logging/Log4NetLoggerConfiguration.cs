using System.Collections.Generic;

namespace FoxTunes
{
    public static class Log4NetLoggerConfiguration
    {
        public const string DEFAULT_APPENDER_ELEMENT = "A673B653-D5CA-40C0-A722-FEB38492150F";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(LoggingBehaviourConfiguration.SECTION, "Logging")
                .WithElement(
                    new BooleanConfigurationElement(DEFAULT_APPENDER_ELEMENT, "Default Appender (Log.txt)")
#if DEBUG
                        .WithValue(true)
#else
                        .WithValue(false)
#endif
                );
        }
    }
}
