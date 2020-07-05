using System;

namespace FoxTunes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class UIComponentAttribute : Attribute
    {
        public UIComponentAttribute(string id, string slot, string name = null, string description = null, UIComponentRole role = UIComponentRole.None)
        {
            this.Id = id;
            this.Slot = slot;
            this.Name = name;
            this.Description = description;
            this.Role = role;
        }

        public string Id { get; private set; }

        public string Slot { get; private set; }

        public string Name { get; private set; }

        public string Description { get; private set; }

        public UIComponentRole Role { get; private set; }
    }

    public enum UIComponentRole : byte
    {
        None,
        LibraryView,
        Hidden
    }
}
