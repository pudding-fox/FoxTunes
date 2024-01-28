using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace FoxTunes
{
    [Component("BA77B392-1900-4931-B720-16206B23DDA1", ComponentSlots.Configuration, priority: ComponentAttribute.PRIORITY_HIGH)]
    public class Configuration : StandardComponent, IConfiguration
    {
        private static readonly string ConfigurationFileName = Path.Combine(
            Publication.StoragePath,
            "Settings.dat"
        );

        const int TIMEOUT = 1000;

        public Configuration()
        {
            this.Debouncer = new Debouncer(TIMEOUT);
            this.Sections = new ObservableCollection<ConfigurationSection>();
        }

        public Debouncer Debouncer { get; private set; }

        private static readonly Lazy<ReleaseType> _ReleaseType = new Lazy<ReleaseType>(() =>
        {
            try
            {
                var value = ConfigurationManager.AppSettings.Get("ReleaseType");
                var result = default(ReleaseType);
                if (string.IsNullOrEmpty(value) || !Enum.TryParse<ReleaseType>(value, out result))
                {
                    Logger.Write(typeof(Configuration), LogLevel.Error, "Failed to parse the release type \"{0}\", falling back to default.", value);
                    return ReleaseType.Default;
                }
                return result;
            }
            catch (Exception e)
            {
                Logger.Write(typeof(Configuration), LogLevel.Error, "Failed to read the release type, falling back to default: {0}", e.Message);
                return ReleaseType.Default;
            }
        });

        public ReleaseType ReleaseType
        {
            get
            {
                return _ReleaseType.Value;
            }
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
            section.Error += this.OnError;
            this.Sections.Add(section);
        }

        private void Update(ConfigurationSection section)
        {
            Logger.Write(this, LogLevel.Debug, "Updating configuration section: {0} => {1}", section.Id, section.Name);
            var existing = this.GetSection(section.Id);
            existing.Update(section, true);
        }

        public void Load()
        {
            if (!File.Exists(ConfigurationFileName))
            {
                Logger.Write(this, LogLevel.Debug, "Configuration file \"{0}\" does not exist.", ConfigurationFileName);
                return;
            }
            Logger.Write(this, LogLevel.Debug, "Loading configuration from file \"{0}\".", ConfigurationFileName);
            try
            {
                using (var stream = File.OpenRead(ConfigurationFileName))
                {
                    var formatter = new BinaryFormatter();
                    var sections = (IEnumerable<ConfigurationSection>)formatter.Deserialize(stream);
                    foreach (var section in sections)
                    {
                        if (!this.Contains(section.Id))
                        {
                            //If config was created by a component that is no longer loaded then it will be lost here.
                            //TODO: Add the config but hide it so it's preserved but not displayed.
                            Logger.Write(this, LogLevel.Warn, "Configuration section \"{0}\" no longer exists.", section.Id);
                            continue;
                        }
                        try
                        {
                            Logger.Write(this, LogLevel.Debug, "Loading configuration section \"{0}\".", section.Id);
                            var existing = this.GetSection(section.Id);
                            existing.Update(section, false);
                        }
                        catch (Exception e)
                        {
                            Logger.Write(this, LogLevel.Warn, "Failed to load configuration section \"{0}\": {1}", section.Id, e.Message);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to load configuration: {0}", e.Message);
            }
        }

        public void Save()
        {
            this.Debouncer.Exec(() =>
            {
                try
                {
                    using (var stream = File.OpenWrite(ConfigurationFileName))
                    {
                        var formatter = new BinaryFormatter();
                        formatter.Serialize(stream, this.Sections.ToArray());
                    }
                }
                catch (Exception e)
                {
                    Logger.Write(this, LogLevel.Warn, "Failed to save configuration: {0}", e.Message);
                }
            });
        }

        public void Reset()
        {
            foreach (var section in this.Sections)
            {
                section.Reset();
            }
        }

        public void ConnectDependencies()
        {
            foreach (var section in this.Sections)
            {
                foreach (var element in section.Elements)
                {
                    if (element.Dependencies == null)
                    {
                        continue;
                    }
                    foreach (var pair in element.Dependencies)
                    {
                        this.ConnectDependencies(element, pair.Key, pair.Value);
                    }
                }
            }
        }

        protected virtual void ConnectDependencies(ConfigurationElement element, string sectionId, IEnumerable<string> elementIds)
        {
            var dependencies = elementIds.Select(
                elementId => this.GetElement<BooleanConfigurationElement>(sectionId, elementId)
            ).ToArray();
            var handler = new EventHandler((sender, e) =>
            {
                if (dependencies.All(dependency => dependency != null && dependency.Value))
                {
                    element.Show();
                }
                else
                {
                    element.Hide();
                }
            });
            foreach (var dependency in dependencies)
            {
                dependency.ValueChanged += handler;
            }
            handler(typeof(Configuration), EventArgs.Empty);
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
