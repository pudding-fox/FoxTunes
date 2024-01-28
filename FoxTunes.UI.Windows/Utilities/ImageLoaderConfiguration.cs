using System.Collections.Generic;

namespace FoxTunes
{
    public static class ImageLoaderConfiguration
    {
        public const string SECTION = "E7A1034C-B3E3-4983-8995-875074426CBB";

        public const string HIGH_QUALITY_RESIZER = "AAAA03BB-6761-4371-B0DF-8D59191FA489";

        public const string CACHE_SIZE = "BBBBDADA-446D-490B-BDDC-446CCC063AD5";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION, "Images")
                .WithElement(new BooleanConfigurationElement(HIGH_QUALITY_RESIZER, "High Quality Resizer").WithValue(true))
                .WithElement(new IntegerConfigurationElement(CACHE_SIZE, "Cache Size").WithValue(128).WithValidationRule(new IntegerValidationRule(64, 1280))
            );
        }
    }
}
