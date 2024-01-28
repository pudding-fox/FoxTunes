using FoxTunes.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace FoxTunes
{
    [Serializable]
    public class ConfigurationSection : BaseComponent, ISerializable
    {
        public ConfigurationSection()
        {
            this.Elements = new ObservableCollection<ConfigurationElement>();
            this.Flags = ConfigurationSectionFlags.None;
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
            this.Add(element);
            return this;
        }

        private ConfigurationSectionFlags _Flags { get; set; }

        public ConfigurationSectionFlags Flags
        {
            get
            {
                return this._Flags;
            }
            set
            {
                this._Flags = value;
                this.OnFlagsChanged();
            }
        }

        protected virtual void OnFlagsChanged()
        {
            if (this.FlagsChanged != null)
            {
                this.FlagsChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Flags");
        }

        public event EventHandler FlagsChanged;

        public ConfigurationSection WithFlags(ConfigurationSectionFlags flags)
        {
            this.Flags = flags;
            return this;
        }

        public void Update(ConfigurationSection section, bool create)
        {
            foreach (var element in section.Elements.ToArray())
            {
                if (!this.Contains(element.Id))
                {
                    if (!create)
                    {
                        //If config was created by a component that is no longer loaded then it will be lost here.
                        //TODO: Add the config but hide it so it's preserved but not displayed.
                        Logger.Write(this, LogLevel.Warn, "Configuration element \"{0}\" no longer exists.", element.Id);
                        continue;
                    }
                    this.Add(element);
                }
                else
                {
                    this.Update(element, create);
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
            element.Error += this.OnError;
            this.Elements.Add(element);
        }

        private void Update(ConfigurationElement element, bool create)
        {
            Logger.Write(this, LogLevel.Debug, "Updating configuration element: {0} => {1}", element.Id, element.Name);
            var existing = this.GetElement(element.Id);
            existing.Update(element, create);
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

        public void Reset()
        {
            foreach (var element in this.Elements)
            {
                element.Reset();
            }
        }

        #region ISerializable

        protected ConfigurationSection(SerializationInfo info, StreamingContext context)
        {
            this.Id = info.GetString(nameof(this.Id));
            this.Elements = (ObservableCollection<ConfigurationElement>)info.GetValue(nameof(this.Elements), typeof(ObservableCollection<ConfigurationElement>));
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(this.Id), this.Id);
            info.AddValue(nameof(this.Elements), this.Elements);
        }

        #endregion
    }

    [Flags]
    public enum ConfigurationSectionFlags : byte
    {
        None = 0,
        System = 1
    }
}
