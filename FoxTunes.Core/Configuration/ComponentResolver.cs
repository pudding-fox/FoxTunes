using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace FoxTunes
{
    public class ComponentResolver : IComponentResolver
    {
        protected static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        public static string Location
        {
            get
            {
                return Path.GetDirectoryName(typeof(AssemblyResolver).Assembly.Location);
            }
        }

        public static string FileName
        {
            get
            {
                return Path.Combine(Location, "Components.xml");
            }
        }

        public ComponentResolver()
        {
            this.Load();
        }

        public IDictionary<string, ComponentSlot> Slots { get; private set; }

        public bool Enabled
        {
            get
            {
                return Publication.IsPortable;
            }
        }

        public void Load()
        {
            this.Slots = new Dictionary<string, ComponentSlot>(StringComparer.OrdinalIgnoreCase);
            try
            {
                if (File.Exists(FileName))
                {
                    Logger.Write(typeof(ComponentResolver), LogLevel.Debug, "Loading component slots from file: {0}", FileName);
                    using (var stream = File.OpenRead(FileName))
                    {
                        var pairs = Serializer.Load(stream);
                        foreach (var pair in pairs)
                        {
                            if (!this.Slots.TryAdd(pair.Key, pair.Value))
                            {
                                continue;
                            }
                            Logger.Write(typeof(ComponentResolver), LogLevel.Debug, "Loaded component slot: {0} => {1}", pair.Key, pair.Value);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Write(typeof(ComponentResolver), LogLevel.Warn, "Failed to load component slots: {0}", e.Message);
            }
            foreach (var slot in ComponentSlots.All)
            {
                if (!this.Slots.TryAdd(slot, ComponentSlot.None))
                {
                    continue;
                }
                Logger.Write(typeof(ComponentResolver), LogLevel.Debug, "Component slot {0} is not configured.", slot);
            }
        }

        public void Save()
        {
            if (!this.Enabled)
            {
                Logger.Write(typeof(ComponentResolver), LogLevel.Debug, "Cannot save component slots.");
                return;
            }
            Logger.Write(typeof(ComponentResolver), LogLevel.Debug, "Saving component slots from file: {0}", FileName);
            try
            {
                using (var stream = File.Create(FileName))
                {
                    Serializer.Save(stream, this.Slots);
                }
            }
            catch (Exception e)
            {
                Logger.Write(typeof(ComponentResolver), LogLevel.Warn, "Failed to save component slots: {0}", e.Message);
            }
        }

        public bool Get(string slot, out string id)
        {
            var component = default(ComponentSlot);
            if (this.Slots.TryGetValue(slot, out component) && !string.Equals(component.Id, ComponentSlots.None, StringComparison.OrdinalIgnoreCase))
            {
                id = component.Id;
                return true;
            }
            else
            {
                id = ComponentSlots.None;
                return false;
            }
        }

        public void Add(string slot, string id)
        {
            this.Slots[slot] = new ComponentSlot(id);
        }

        public void Remove(string slot)
        {
            this.Add(slot, ComponentSlots.None);
        }

        public void Persist(string slot)
        {
            var component = default(ComponentSlot);
            if (this.Slots.TryGetValue(slot, out component) && !string.Equals(component.Id, ComponentSlots.None, StringComparison.OrdinalIgnoreCase))
            {
                component.Persist = true;
            }
        }

        public static readonly IComponentResolver Instance = new ComponentResolver();

        public class ComponentSlot
        {
            public ComponentSlot(string id)
            {
                this.Id = id;
            }

            public string Id { get; private set; }

            public bool Persist { get; set; }

            public static readonly ComponentSlot None = new ComponentSlot(ComponentSlots.None);
        }

        public static class Serializer
        {
            const string Component = "Component";

            const string Slot = "Slot";

            const string Id = "Id";

            public static IEnumerable<KeyValuePair<string, ComponentSlot>> Load(Stream stream)
            {
                using (var reader = new XmlTextReader(stream))
                {
                    reader.ReadStartElement(Publication.Product);
                    while (reader.IsStartElement(Component))
                    {
                        var key = reader.GetAttribute(Slot);
                        var value = reader.GetAttribute(Id);
                        if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                        {
                            yield return new KeyValuePair<string, ComponentSlot>(key, new ComponentSlot(value));
                        }
                        if (reader.NodeType == XmlNodeType.EndElement && string.Equals(reader.Name, Component))
                        {
                            reader.ReadEndElement();
                        }
                        else
                        {
                            reader.Read();
                        }
                    }
                    if (reader.NodeType == XmlNodeType.EndElement && string.Equals(reader.Name, Publication.Product))
                    {
                        reader.ReadEndElement();
                    }
                    else
                    {
                        reader.Read();
                    }
                }
            }

            public static void Save(Stream stream, IEnumerable<KeyValuePair<string, ComponentSlot>> sequence)
            {
                using (var writer = new XmlTextWriter(stream, Encoding.Default))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement(Publication.Product);
                    foreach (var pair in sequence)
                    {
                        if (!pair.Value.Persist)
                        {
                            continue;
                        }
                        writer.WriteStartElement(Component);
                        writer.WriteAttributeString(Slot, pair.Key);
                        writer.WriteAttributeString(Id, pair.Value.Id);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
            }
        }
    }
}
