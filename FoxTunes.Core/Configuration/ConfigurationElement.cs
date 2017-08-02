using System;

namespace FoxTunes
{
    [Serializable]
    public abstract class ConfigurationElement : BaseComponent
    {
        protected ConfigurationElement(string id, string name = null, string description = null)
        {
            this.Id = id;
            this.Name = name;
            this.Description = description;
        }

        public string Id { get; private set; }

        public string Name { get; private set; }

        public string Description { get; private set; }

        public abstract void ConnectValue<T>(Action<T> action);
    }
}
