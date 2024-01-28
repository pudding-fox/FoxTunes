using System;
using System.Collections.ObjectModel;

namespace FoxTunes
{
    [Serializable]
    public class ConfigurationSection : BaseComponent
    {
        public ConfigurationSection()
        {
            this.Elements = new ObservableCollection<ConfigurationElement>();
        }

        public ConfigurationSection(string id, string name = null, string description = null)
            : this()
        {
            this.Id = id;
            this.Name = name;
            this.Description = description;
        }

        public string Id { get; private set; }

        public string Name { get; private set; }

        public string Description { get; private set; }

        public ObservableCollection<ConfigurationElement> Elements { get; private set; }

        public ConfigurationSection WithElement(ConfigurationElement element)
        {
            this.Elements.Add(element);
            return this;
        }
    }
}
