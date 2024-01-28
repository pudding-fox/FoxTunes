using System;

namespace FoxTunes
{
    public class UIPlaylistColumnProvider
    {
        public const string PLACEHOLDER = "00000000-0000-0000-0000-000000000000";

        public UIPlaylistColumnProvider(UIPlaylistColumnProviderAttribute attribute, Type type)
        {
            this.Id = attribute.Id;
            this.Name = attribute.Name;
            this.Description = attribute.Description;
            this.Type = type;
        }

        public string Id { get; private set; }

        public string Name { get; private set; }

        public string Description { get; private set; }

        public Type Type { get; private set; }
    }
}
