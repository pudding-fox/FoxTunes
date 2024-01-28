using System;

namespace FoxTunes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ComponentAttribute : Attribute
    {
        public const byte PRIORITY_HIGH = 0;

        public const byte PRIORITY_NORMAL = 100;

        public const byte PRIORITY_LOW = 255;

        public ComponentAttribute(string id, string slot, string name = null, string description = null, byte priority = PRIORITY_LOW, bool @default = false)
        {
            this.Id = id;
            this.Slot = slot;
            this.Name = name;
            this.Description = description;
            this.Priority = priority;
            this.Default = @default;
        }

        public string Id { get; private set; }

        public string Slot { get; private set; }

        public string Name { get; private set; }

        public string Description { get; private set; }

        public byte Priority { get; private set; }

        public bool Default { get; private set; }
    }
}
