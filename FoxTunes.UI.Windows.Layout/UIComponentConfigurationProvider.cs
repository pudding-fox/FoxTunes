using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;

namespace FoxTunes
{
    public class UIComponentConfigurationProvider : BaseComponent, IConfiguration
    {
        public const string PREFIX = "Configuration_";

        public const string DELIMITER = "_";

        public UIComponentConfigurationProvider(UIComponentConfiguration component)
        {
            this.Component = component;
            this.Sections = new ObservableCollection<ConfigurationSection>();
        }

        public UIComponentConfiguration Component { get; private set; }

        public bool IsLoaded { get; private set; }

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

        public ObservableCollection<ConfigurationSection> Sections { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Configuration = core.Components.Configuration;
            base.InitializeComponent(core);
        }

        public void Load()
        {
            this.Load(this.Profile);
        }

        public void Load(string profile)
        {
            foreach (var section in this.Sections)
            {
                if (section.IsInitialized)
                {
                    continue;
                }
                section.InitializeComponent();
            }
            foreach (var pair in this.Component.MetaData)
            {
                if (!pair.Key.StartsWith(PREFIX, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                var parts = pair.Key.Substring(
                    PREFIX.Length
                ).Split(new[] { DELIMITER }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                {
                    continue;
                }
                var sectionId = parts[0];
                var elementId = parts[1];
                if (!this.Contains(sectionId))
                {
                    Logger.Write(this, LogLevel.Warn, "Configuration section \"{0}\" no longer exists.", sectionId);
                    continue;
                }
                var existing = this.GetSection(sectionId);
                try
                {
                    Logger.Write(this, LogLevel.Debug, "Loading configuration section \"{0}\".", sectionId);
                    this.Load(existing, elementId, pair.Value);
                }
                catch (Exception e)
                {
                    Logger.Write(this, LogLevel.Warn, "Failed to load configuration section \"{0}\": {1}", existing.Id, e.Message);
                }
            }
            this.IsLoaded = true;
        }

        protected virtual void Load(ConfigurationSection section, string id, string value)
        {
            if (!section.Contains(id))
            {
                Logger.Write(this, LogLevel.Warn, "Configuration element \"{0}\" no longer exists.", id);
                return;
            }
            Logger.Write(this, LogLevel.Debug, "Loading configuration element: \"{0}\".", id);
            var existing = section.GetElement(id);
            existing.SetPersistentValue(value);
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
            throw new NotImplementedException();
        }

        public ConfigurationSection GetSection(string sectionId)
        {
            var result = this.Sections.FirstOrDefault(
                section => string.Equals(section.Id, sectionId, StringComparison.OrdinalIgnoreCase)
            );
            if (result == null && this.IsLoaded)
            {
                result = this.Configuration.GetSection(sectionId);
            }
            return result;
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
            var result = section.GetElement(elementId);
            if (result == null && this.IsLoaded)
            {
                result = this.Configuration.GetElement(sectionId, elementId);
            }
            return result;
        }

        public void Reset()
        {
            foreach (var section in this.Sections)
            {
                section.Reset();
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
            foreach (var section in this.Sections)
            {
                var elements = section.Elements.Where(
                    element => element.IsModified
                ).ToArray();
                if (!elements.Any())
                {
                    continue;
                }
                foreach (var element in elements)
                {
                    this.Component.MetaData.TryAdd(
                        string.Concat(PREFIX, section.Id, DELIMITER, element.Id),
                        element.GetPersistentValue()
                    );
                }
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
            this.Sections.Add(section);
        }

        private void Update(ConfigurationSection section)
        {
            Logger.Write(this, LogLevel.Debug, "Updating configuration section: {0} => {1}", section.Id, section.Name);
            var existing = this.GetSection(section.Id);
            existing.Update(section);
        }
    }
}
