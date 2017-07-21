using System;

namespace FoxTunes
{
    [Serializable]
    public class ConfigurationElement : BaseComponent
    {
        public ConfigurationElement(string id, string name = null, string description = null)
        {
            this.Id = id;
            this.Name = name;
            this.Description = description;
        }

        public string Id { get; private set; }

        public string Name { get; private set; }

        public string Description { get; private set; }
    }
}
