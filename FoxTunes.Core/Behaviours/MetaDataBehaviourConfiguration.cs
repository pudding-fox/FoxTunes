using System;
using System.Collections.Generic;

namespace FoxTunes
{
    public static class MetaDataBehaviourConfiguration
    {
        public const string SECTION = "C1EBA337-A333-444D-9AAE-D8CE36C39605";

        public const string COPY_IMAGES_ELEMENT = "AAAAA875-8195-49AF-A1A4-2EAB2294A6D8";

        public const string THREADS_ELEMENT = "BBBB16BD-B4A7-4F9D-B4DA-F81E932F6DD9";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION, "Meta Data")
                .WithElement(
                    new BooleanConfigurationElement(COPY_IMAGES_ELEMENT, "Copy Images").WithValue(false))
                .WithElement(
                    new IntegerConfigurationElement(THREADS_ELEMENT, "Background Threads").WithValue(Environment.ProcessorCount).WithValidationRule(new IntegerValidationRule(1, 32))
            );
        }
    }
}
