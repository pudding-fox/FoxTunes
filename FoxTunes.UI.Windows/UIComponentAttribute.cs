using System;

namespace FoxTunes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class UIComponentAttribute : Attribute
    {
        public UIComponentAttribute(string id, string slot, string name = null, string description = null)
        {
            this.Id = id;
            this.Slot = slot;
            this.Name = name;
            this.Description = description;
        }

        public string Id { get; private set; }

        public string Slot { get; private set; }

        public string Name { get; private set; }

        public string Description { get; private set; }
    }
}
