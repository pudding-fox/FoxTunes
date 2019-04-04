using System.Collections.Generic;

namespace FoxTunes
{
    public static class LoggingBehaviourConfiguration
    {
        public const string SECTION = "6D2403BD-7CBD-4690-A119-90F4A45B00CD";

        public const string TRACE_ELEMENT = "8E1516BC-719C-4786-BB34-E2EE4F5C6491";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION, "Logging")
                .WithElement(
                    new BooleanConfigurationElement(TRACE_ELEMENT, "Trace").WithValue(false)
                );
        }
    }
}
