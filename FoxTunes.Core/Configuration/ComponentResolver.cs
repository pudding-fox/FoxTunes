using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using System.Linq;

namespace FoxTunes
{
    public class ComponentResolver : IComponentResolver
    {
        public static readonly string FILE_NAME = string.Format("{0}.config", Assembly.GetEntryAssembly().Location);

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

        public void Add(string slot)
        {
            var components = ComponentRegistry.Instance.GetComponents(slot);
            foreach (var component in components)
            {
                var attribute = component.GetType().GetCustomAttribute<ComponentAttribute>();
                if (attribute == null)
                {
                    continue;
                }
                var name = attribute.Name;
                if (string.IsNullOrEmpty(name))
                {
                    name = component.GetType().FullName;
                }
                this.Add(slot, attribute.Id, name);
            }
        }

        private void Add(string slot, string id, string name)
        {
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
            appSettings.Add(new XComment(string.Format("Uncomment the next element to use component {0}.", name)));
            appSettings.Add(new XComment(new XElement("add", new XAttribute("key", slot), new XAttribute("value", id)).ToString()));
            document.Save(FILE_NAME);
        }

        public static readonly IComponentResolver Instance = new ComponentResolver();
    }
}
