using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace FoxTunes
{
    [Component("BA77B392-1900-4931-B720-16206B23DDA1", ComponentSlots.Configuration)]
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
            if (this.GetSection(section.Id) != null)
            {
                return;
            }
            this.Sections.Add(section);
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
            var section = this.GetSection(sectionId);
            if (section == null)
            {
                return default(T);
            }
            return section.Elements.FirstOrDefault(element => string.Equals(element.Id, elementId, StringComparison.OrdinalIgnoreCase)) as T;
        }
    }
}
