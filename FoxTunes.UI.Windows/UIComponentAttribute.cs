using System;

namespace FoxTunes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class UIComponentAttribute : Attribute
    {
        public UIComponentAttribute(string id, string name, string description = null, UIComponentRole role = UIComponentRole.None)
        {
            this.Id = id;
            this.Name = name;
            this.Description = description;
            this.Role = role;
        }

        public string Id { get; private set; }

        public string Name { get; private set; }

        public string Description { get; private set; }

        public UIComponentRole Role { get; private set; }
    }

    public enum UIComponentRole : byte
    {
        None,
        Container,
        Visualization,
        Playback,
        Info,
        System,
        Launcher,
        Playlist,
        Library,
        DSP
    }
}
