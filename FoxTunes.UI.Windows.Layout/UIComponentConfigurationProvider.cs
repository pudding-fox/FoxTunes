using FoxDb;
using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes
{
    public class UIComponentConfigurationProvider : BaseComponent, IConfiguration
    {
        public const string PREFIX = "Configuration_";

        public UIComponentConfigurationProvider(UIComponentConfiguration component)
        {
            this.Component = component;
            this.Sections = new Dictionary<string, ConfigurationSection>(StringComparer.OrdinalIgnoreCase);
            this.Elements = new Dictionary<string, ConfigurationElement>(StringComparer.OrdinalIgnoreCase);
        }

        public UIComponentConfiguration Component { get; private set; }

        public IEnumerable<string> AvailableProfiles
        {
            get
            {
                return Enumerable.Empty<string>();
            }
        }

        public string Profile
        {
            get
            {
                return string.Empty;
            }
        }

        public bool IsDefaultProfile
        {
            get
            {
                return true;
            }
        }

        IEnumerable<ConfigurationSection> IConfiguration.Sections
        {
            get
            {
                return this.Sections.Values;
            }
        }

        public IDictionary<string, ConfigurationSection> Sections { get; private set; }

        public IDictionary<string, ConfigurationElement> Elements { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Configuration = core.Components.Configuration;
            foreach (var section in this.Configuration.Sections)
            {
                this.WithSection(section);
            }
            base.InitializeComponent(core);
        }

        public void Load()
        {
            this.Load(this.Profile);
        }

        public void Load(string profile)
        {
            foreach (var pair in this.Sections)
            {
                if (pair.Value.IsInitialized)
                {
                    continue;
                }
                pair.Value.InitializeComponent();
            }
            foreach (var pair in this.Component.MetaData)
            {
                if (!pair.Key.StartsWith(PREFIX, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                var id = pair.Key.Substring(PREFIX.Length);
                var value = pair.Value;
                this.Load(id, value);
            }
        }

        protected virtual void Load(string id, string value)
        {
            var element = default(ConfigurationElement);
            if (!this.Elements.TryGetValue(id, out element))
            {
                Logger.Write(this, LogLevel.Warn, "Configuration element \"{0}\" no longer exists.", id);
                return;
            }
            Logger.Write(this, LogLevel.Debug, "Loading configuration element: \"{0}\".", id);
            element.SetPersistentValue(value);
        }

        protected virtual void OnSaved()
        {
            if (this.Saved == null)
            {
                return;
            }
            this.Saved(this, EventArgs.Empty);
        }

        public event EventHandler Saved;

        public void Delete()
        {
            throw new NotImplementedException();
        }

        public void Delete(string profile)
        {
            throw new NotImplementedException();
        }

        public void ConnectDependencies()
        {
            foreach (var element in this.Elements.Values)
            {
                element.ConnectDependencies(this);
            }
        }

        public ConfigurationSection GetSection(string sectionId)
        {
            var section = default(ConfigurationSection);
            if (this.Sections.TryGetValue(sectionId, out section))
            {
                return section;
            }
            return default(ConfigurationSection);
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

        public void Reset()
        {
            foreach (var pair in this.Sections)
            {
                pair.Value.Reset();
            }
        }

        public void Save()
        {
            this.Save(this.Profile);
        }

        public void Save(string profile)
        {
            foreach (var pair in this.Component.MetaData)
            {
                if (pair.Key.StartsWith(PREFIX, StringComparison.OrdinalIgnoreCase))
                {
                    this.Component.MetaData.TryRemove(pair.Key);
                }
            }
            foreach (var element in this.Elements.Values)
            {
                if (!element.IsModified)
                {
                    continue;
                }
                var key = string.Concat(PREFIX, element.Id);
                var value = element.GetPersistentValue();
                this.Component.MetaData.TryAdd(key, value);
            }
            this.OnSaved();
        }

        public IConfiguration WithSection(ConfigurationSection section)
        {
            if (this.Contains(section.Id))
            {
                this.Update(section);
            }
            else
            {
                this.Add(section);
            }
            return this;
        }

        public bool Contains(string id)
        {
            return this.GetSection(id) != null;
        }

        private void Add(ConfigurationSection section)
        {
            Logger.Write(this, LogLevel.Debug, "Adding configuration section: {0} => {1}", section.Id, section.Name);
            section = new ConfigurationSectionWrapper(this, section);
            this.Sections.Add(section.Id, section);
        }

        private void Update(ConfigurationSection section)
        {
            Logger.Write(this, LogLevel.Debug, "Updating configuration section: {0} => {1}", section.Id, section.Name);
            var existing = this.GetSection(section.Id);
            existing.Update(section);
        }

        public class ConfigurationSectionWrapper : ConfigurationSection
        {
            public ConfigurationSectionWrapper(UIComponentConfigurationProvider provider, ConfigurationSection section) : base(section.Id, section.Name, section.Description)
            {
                this.Provider = provider;
                this.Elements.AddRange(section.Elements);
                if (this.Provider.IsInitialized)
                {
                    this.Provider.Elements.AddRange(section.Elements);
                }
                this.Flags = section.Flags;
            }

            public UIComponentConfigurationProvider Provider { get; private set; }

            protected override void Add(ConfigurationElement element)
            {
                if (this.Provider.IsInitialized)
                {
                    this.Provider.Elements.Add(element.Id, element);
                }
                base.Add(element);
            }
        }
    }
}
