using System;

namespace FoxTunes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class UIComponentAttribute : Attribute
    {
        public UIComponentAttribute(string id, UIComponentRole role = UIComponentRole.None)
        {
            this.Id = id;
            this.Role = role;
        }

        public string Id { get; private set; }

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
