using FoxTunes.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;

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

        public void Update(ConfigurationSection section)
        {
            this.Name = section.Name;
            this.Description = section.Description;
            foreach (var element in section.Elements.ToArray())
            {
                if (this.Contains(element.Id))
                {
                    this.Update(element);
                }
                else
                {
                    this.Add(element);
                }
            }
            foreach (var element in this.Elements.ToArray())
            {
                if (!section.Contains(element.Id))
                {
                    this.Hide(element);
                }
            }
        }

        private bool Contains(string id)
        {
            return this.GetElement(id) != null;
        }

        private void Add(ConfigurationElement element)
        {
            Logger.Write(this, LogLevel.Debug, "Adding configuration element: {0} => {1}", element.Id, element.Name);
            this.Elements.Add(element);
        }

        private void Update(ConfigurationElement element)
        {
            Logger.Write(this, LogLevel.Debug, "Updating configuration element: {0} => {1}", element.Id, element.Name);
            var existing = this.GetElement(element.Id);
            existing.Update(element);
        }

        private void Hide(ConfigurationElement element)
        {
            Logger.Write(this, LogLevel.Debug, "Hiding configuration element: {0} => {1}", element.Id, element.Name);
            var existing = this.GetElement(element.Id);
            existing.Hide();
        }

        public T GetElement<T>(string elementId) where T : ConfigurationElement
        {
            return this.GetElement(elementId) as T;
        }

        public ConfigurationElement GetElement(string elementId)
        {
            return this.Elements.FirstOrDefault(element => string.Equals(element.Id, elementId, StringComparison.OrdinalIgnoreCase));
        }
    }
}
