using System;
using System.Collections.Generic;

namespace FoxTunes
{
    public static class BassReplayGainScannerBehaviourConfiguration
    {
        public const string WRITE_TAGS = "AAAA2157-6395-40A7-80DE-C2DCDE4B5131";

        public const string THREADS = "BBBB7889-4D4C-4EB8-8EE9-BE075B64347A";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(BassOutputConfiguration.SECTION, "Output")
                .WithElement(
                    new BooleanConfigurationElement(WRITE_TAGS, "Write Tags", path: "Replay Gain/Scanner").WithValue(true))
                .WithElement(
                    new IntegerConfigurationElement(THREADS, "Background Threads", path: "Replay Gain/Scanner").WithValue(Environment.ProcessorCount).WithValidationRule(new IntegerValidationRule(1, 32))
                );
        }
    }
}
