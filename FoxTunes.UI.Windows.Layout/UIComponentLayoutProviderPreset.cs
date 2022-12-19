using FoxTunes.Interfaces;
using System.Collections.Generic;

namespace FoxTunes
{
    [WindowsUserInterfaceDependency]
    public abstract class UIComponentLayoutProviderPreset : StandardComponent, IUIComponentLayoutProviderPreset, IConfigurableComponent
    {
        public const string CATEGORY_MAIN = "2CE18904-A499-4CBF-8BFA-E4D594F7D37F";

        protected UIComponentLayoutProviderPreset(string id, string category, string name, string layout)
        {
            this.Id = id;
            this.Category = category;
            this.Name = name;
            this.Layout = layout;
        }

        public string Id { get; private set; }

        public string Category { get; private set; }

        public string Name { get; private set; }

        public string Layout { get; private set; }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(GetSection(this.Category))
                .WithElement(new SelectionConfigurationElement(GetPreset(this.Category))
                .WithOptions(new[]
                {
                    new SelectionConfigurationOption(this.Id, this.Name)
                })
            );
        }

        public static string GetSection(string category)
        {
            return UIComponentLayoutProviderConfiguration.SECTION;
        }

        public static string GetPreset(string category)
        {
            return UIComponentLayoutProviderConfiguration.MAIN_PRESET;
        }

        public static string GetLayout(string category)
        {
            return UIComponentLayoutProviderConfiguration.MAIN_LAYOUT;
        }
    }
}
