using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;

namespace FoxTunes
{
    public static class ImageLoaderConfiguration
    {
        public const string SECTION = "E7A1034C-B3E3-4983-8995-875074426CBB";

        public const string HIGH_QUALITY_RESIZER = "AAAA03BB-6761-4371-B0DF-8D59191FA489";

        public const string CACHE_SIZE = "BBBBDADA-446D-490B-BDDC-446CCC063AD5";

        public const string THREADS = "CCCC8536-6631-4B67-A84A-00AF6218E7BA";

        public const string INTERVAL = "DDDD3458-2514-478A-BC80-8731CAE69C88";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            var cacheSize = default(int);
            var releaseType = StandardComponents.Instance.Configuration.ReleaseType;
            switch (releaseType)
            {
                case ReleaseType.Minimal:
                    cacheSize = 128;
                    break;
                default:
                    cacheSize = 1024;
                    break;
            }
            yield return new ConfigurationSection(SECTION, "Images")
                .WithElement(new BooleanConfigurationElement(HIGH_QUALITY_RESIZER, "High Quality Resizer").WithValue(true))
                .WithElement(new IntegerConfigurationElement(CACHE_SIZE, "Cache Size", path: "Advanced").WithValue(cacheSize).WithValidationRule(new IntegerValidationRule(64, 2048)))
                .WithElement(new IntegerConfigurationElement(THREADS, "Background Threads", path: "Advanced").WithValue(Environment.ProcessorCount).WithValidationRule(new IntegerValidationRule(1, 32)))
                .WithElement(new IntegerConfigurationElement(INTERVAL, "Interval", path: "Advanced").WithValue(10).WithValidationRule(new IntegerValidationRule(0, 100))
            );
        }
    }
}
