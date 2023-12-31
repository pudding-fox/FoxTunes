﻿using System.Collections.Generic;

namespace FoxTunes
{
    public static class UIComponentLayoutProviderConfiguration
    {
        public const string SECTION = WindowsUserInterfaceConfiguration.SECTION;

        public const string LAYOUT = WindowsUserInterfaceConfiguration.LAYOUT_ELEMENT;

        public const string MAIN_PRESET = "AAAA6555-FE8E-4E3D-908D-C6F7F19C475D";

        public const string MAIN_LAYOUT = "AAAAE4B-657B-45C2-B365-8FD69498D7C2";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION)
                .WithElement(new SelectionConfigurationElement(MAIN_PRESET, Strings.UIComponentLayoutProviderConfiguration_Preset, path: Strings.UIComponentLayoutProviderConfiguration_Path)
                    .DependsOn(SECTION, LAYOUT, UIComponentLayoutProvider.ID))
                .WithElement(new TextConfigurationElement(MAIN_LAYOUT, Strings.UIComponentLayoutProviderConfiguration_MainLayout, path: Strings.UIComponentLayoutProviderConfiguration_Path)
                    .WithValue(Resources.Main_1)
                    .WithFlags(ConfigurationElementFlags.MultiLine)
                    .DependsOn(SECTION, LAYOUT, UIComponentLayoutProvider.ID)
            );
        }
    }
}
