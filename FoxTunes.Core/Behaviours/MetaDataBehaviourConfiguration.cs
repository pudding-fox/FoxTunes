using System;
using System.Collections.Generic;

namespace FoxTunes
{
    public static class MetaDataBehaviourConfiguration
    {
        public const string SECTION = "C1EBA337-A333-444D-9AAE-D8CE36C39605";

        public const string ENABLE_ELEMENT = "AAAA3F1B-A3CC-47B9-858C-6005176C4E7F";

        public const string READ_EMBEDDED_IMAGES = "BBBBF119-ABA3-43A7-BEDD-E0640B9A2BC4";

        public const string READ_LOOSE_IMAGES = "CCCCB0DB-9D21-4066-96E4-E677E5DDE2FA";

        public const string LOOSE_IMAGES_FRONT = "DDDDADD6-BCB5-4E5D-830E-242B8557E5CB";

        public const string LOOSE_IMAGES_BACK = "EEEEADD6-BCB5-4E5D-830E-242B8557E5CB";

        public const string COPY_IMAGES_ELEMENT = "FFFFA875-8195-49AF-A1A4-2EAB2294A6D8";

        public const string THREADS_ELEMENT = "GGGG16BD-B4A7-4F9D-B4DA-F81E932F6DD9";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION, "Meta Data")
                .WithElement(
                    new BooleanConfigurationElement(ENABLE_ELEMENT, "Enabled").WithValue(true))
                .WithElement(
                    new BooleanConfigurationElement(READ_EMBEDDED_IMAGES, "Embedded Images").WithValue(true))
                .WithElement(
                    new BooleanConfigurationElement(READ_LOOSE_IMAGES, "File Images").WithValue(true))
                .WithElement(
                    new TextConfigurationElement(LOOSE_IMAGES_FRONT, "Front Cover").WithValue("front, cover, folder"))
                .WithElement(
                    new TextConfigurationElement(LOOSE_IMAGES_BACK, "Back Cover").WithValue("back"))
                .WithElement(
                    new BooleanConfigurationElement(COPY_IMAGES_ELEMENT, "Copy Images").WithValue(true))
                .WithElement(
                    new IntegerConfigurationElement(THREADS_ELEMENT, "Background Threads").WithValue(Environment.ProcessorCount).WithValidationRule(new IntegerValidationRule(1, 32))
            );
            StandardComponents.Instance.Configuration.GetElement<BooleanConfigurationElement>(SECTION, ENABLE_ELEMENT).ConnectValue(value => UpdateConfiguration());
            StandardComponents.Instance.Configuration.GetElement<BooleanConfigurationElement>(SECTION, READ_LOOSE_IMAGES).ConnectValue(value => UpdateConfiguration());
        }

        private static void UpdateConfiguration()
        {
            var enabled = StandardComponents.Instance.Configuration.GetElement<BooleanConfigurationElement>(SECTION, ENABLE_ELEMENT).Value;
            var looseImages = StandardComponents.Instance.Configuration.GetElement<BooleanConfigurationElement>(SECTION, READ_LOOSE_IMAGES).Value;
            if (enabled)
            {
                StandardComponents.Instance.Configuration.GetElement(SECTION, READ_EMBEDDED_IMAGES).Show();
                StandardComponents.Instance.Configuration.GetElement(SECTION, READ_LOOSE_IMAGES).Show();
                if (looseImages)
                {
                    StandardComponents.Instance.Configuration.GetElement(SECTION, LOOSE_IMAGES_FRONT).Show();
                    StandardComponents.Instance.Configuration.GetElement(SECTION, LOOSE_IMAGES_BACK).Show();
                    StandardComponents.Instance.Configuration.GetElement(SECTION, COPY_IMAGES_ELEMENT).Show();
                }
                else
                {
                    StandardComponents.Instance.Configuration.GetElement(SECTION, LOOSE_IMAGES_FRONT).Hide();
                    StandardComponents.Instance.Configuration.GetElement(SECTION, LOOSE_IMAGES_BACK).Hide();
                    StandardComponents.Instance.Configuration.GetElement(SECTION, COPY_IMAGES_ELEMENT).Hide();
                }
                StandardComponents.Instance.Configuration.GetElement(SECTION, THREADS_ELEMENT).Show();
            }
            else
            {
                StandardComponents.Instance.Configuration.GetElement(SECTION, READ_EMBEDDED_IMAGES).Hide();
                StandardComponents.Instance.Configuration.GetElement(SECTION, READ_LOOSE_IMAGES).Hide();
                StandardComponents.Instance.Configuration.GetElement(SECTION, LOOSE_IMAGES_FRONT).Hide();
                StandardComponents.Instance.Configuration.GetElement(SECTION, LOOSE_IMAGES_BACK).Hide();
                StandardComponents.Instance.Configuration.GetElement(SECTION, COPY_IMAGES_ELEMENT).Hide();
                StandardComponents.Instance.Configuration.GetElement(SECTION, THREADS_ELEMENT).Hide();
            }
        }
    }
}
