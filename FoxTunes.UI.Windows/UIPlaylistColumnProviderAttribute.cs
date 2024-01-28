using System;

namespace FoxTunes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class UIPlaylistColumnProviderAttribute : Attribute
    {
        public UIPlaylistColumnProviderAttribute(string id, string name = null, string description = null)
        {
            this.Id = id;
            this.Name = name;
            this.Description = description;
        }

        public string Id { get; private set; }

        public string Name { get; private set; }

        public string Description { get; private set; }
    }
}
