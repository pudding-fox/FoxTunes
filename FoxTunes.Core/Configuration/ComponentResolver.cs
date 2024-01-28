using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Xml.Linq;

namespace FoxTunes
{
    public class ComponentResolver : IComponentResolver
    {
        public static readonly string FILE_NAME = GetFileName();

        protected static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        private static string GetFileName()
        {
            var assembly = Assembly.GetEntryAssembly();
            if (assembly == null)
            {
                return "<No Config>";
            }
            return string.Format("{0}.config", assembly.Location);
        }

        static ComponentResolver()
        {
            Slots = new Dictionary<string, string>();
        }

        public static IDictionary<string, string> Slots { get; private set; }

        public string Get(string slot)
        {
            if (string.IsNullOrEmpty(slot))
            {
                return ComponentSlots.None;
            }
            var id = default(string);
            if (Slots.TryGetValue(slot, out id))
            {
                return id;
            }
            id = ConfigurationManager.AppSettings.Get(slot);
            if (string.IsNullOrEmpty(id))
            {
                return ComponentSlots.None;
            }
            return id;
        }

        public void Add(string slot, Type defaultComponent, IEnumerable<Type> components)
        {
            if (!Publication.IsPortable)
            {
                return;
            }
            foreach (var component in components)
            {
                var attribute = default(ComponentAttribute);
                if (!component.HasCustomAttribute<ComponentAttribute>(out attribute))
                {
                    continue;
                }
                var id = attribute.Id;
                if (string.IsNullOrEmpty(id))
                {
                    continue;
                }
                var name = attribute.Name;
                if (string.IsNullOrEmpty(name))
                {
                    name = component.FullName;
                }
                var @default = component == defaultComponent;
                this.Add(slot, id, name, @default);
            }
        }

        private void Add(string slot, string id, string name, bool @default)
        {
            if (!File.Exists(FILE_NAME))
            {
                Logger.Write(typeof(ComponentResolver), LogLevel.Warn, "Config file \"{0}\" does not exist, cannot add resolution.", FILE_NAME);
                return;
            }
            var document = XDocument.Load(FILE_NAME);
            var appSettings = document.Root.Element("appSettings");
            if (appSettings == null)
            {
                appSettings = new XElement("appSettings");
                document.Root.AddFirst(appSettings);
            }
            if (appSettings.ToString().Contains(id, true))
            {
                return;
            }
            if (@default)
            {
                appSettings.Add(new XComment(string.Format("Comment out the next element to not use component {0}.", name)));
                appSettings.Add(new XElement("add", new XAttribute("key", slot), new XAttribute("value", id)));
            }
            else
            {
                appSettings.Add(new XComment(string.Format("Uncomment the next element to use component {0}.", name)));
                appSettings.Add(new XComment(new XElement("add", new XAttribute("key", slot), new XAttribute("value", id)).ToString()));
            }
            document.Save(FILE_NAME);
        }

        public static readonly IComponentResolver Instance = new ComponentResolver();
    }
}
