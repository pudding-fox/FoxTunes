using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace FoxTunes
{
    [Component("BA77B392-1900-4931-B720-16206B23DDA1", ComponentSlots.Configuration, priority: ComponentAttribute.PRIORITY_HIGH)]
    public class Configuration : StandardComponent, IConfiguration
    {
        private static readonly string ConfigurationFileName = Path.Combine(
            Path.GetDirectoryName(typeof(Configuration).Assembly.Location),
            "Settings.dat"
        );

        public Configuration()
        {
            this.Load();
        }

        public ObservableCollection<ConfigurationSection> Sections { get; private set; }

        public void RegisterSection(ConfigurationSection section)
        {
            if (this.Contains(section.Id))
            {
                this.Update(section);
            }
            else
            {
                this.Add(section);
            }
        }

        private bool Contains(string id)
        {
            return this.GetSection(id) != null;
        }

        private void Add(ConfigurationSection section)
        {
            Logger.Write(this, LogLevel.Debug, "Adding configuration section: {0} => {1}", section.Id, section.Name);
            this.Sections.Add(section);
        }

        private void Update(ConfigurationSection section)
        {
            Logger.Write(this, LogLevel.Debug, "Updating configuration section: {0} => {1}", section.Id, section.Name);
            var existing = this.GetSection(section.Id);
            existing.Update(section);
        }

        public void Load()
        {
            this.Sections = new ObservableCollection<ConfigurationSection>();
            if (!File.Exists(ConfigurationFileName))
            {
                return;
            }
            try
            {
                using (var stream = File.OpenRead(ConfigurationFileName))
                {
                    var formatter = new BinaryFormatter();
                    this.Sections = new ObservableCollection<ConfigurationSection>(
                        (IEnumerable<ConfigurationSection>)formatter.Deserialize(stream)
                    );
                }
            }
            catch
            {
                //Nothing can be done.
            }
        }

        public void Save()
        {
            using (var stream = File.OpenWrite(ConfigurationFileName))
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, this.Sections.ToArray());
            }
        }

        public ConfigurationSection GetSection(string sectionId)
        {
            return this.Sections.FirstOrDefault(section => string.Equals(section.Id, sectionId, StringComparison.OrdinalIgnoreCase));
        }

        public T GetElement<T>(string sectionId, string elementId) where T : ConfigurationElement
        {
            return this.GetElement(sectionId, elementId) as T;
        }

        public ConfigurationElement GetElement(string sectionId, string elementId)
        {
            var section = this.GetSection(sectionId);
            if (section == null)
            {
                return default(ConfigurationElement);
            }
            return section.GetElement(elementId);
        }
    }
}
