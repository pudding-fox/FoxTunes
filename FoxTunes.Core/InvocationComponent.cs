using FoxTunes.Interfaces;

namespace FoxTunes
{
    public class InvocationComponent : IInvocationComponent
    {
        public const string CATEGORY_PLAYLIST = "42BC8F63-202C-4F77-8383-04A54FFFDCD5";

        public const string CATEGORY_LIBRARY = "CE91AB2A-3442-4019-B585-C4DCFEF1DB65";

        public const string CATEGORY_PLAYBACK = "8302CD2E-B798-4765-81DD-A9E6DA86820A";

        public const string CATEGORY_MINI_PLAYER = "5C3822F2-6843-40B2-9A38-28EA191CC3AF";

        public const string CATEGORY_NOTIFY_ICON = "F9E060E6-D791-4E9A-A109-FD145EAECBA2";

        public const string CATEGORY_SETTINGS = "F51F1E76-255F-45A0-A053-9D57EDECE326";

        public const byte ATTRIBUTE_NONE = 0;

        public const byte ATTRIBUTE_SEPARATOR = 1;

        public const byte ATTRIBUTE_SELECTED = 2;

        public InvocationComponent(string category, string id, string name = null, string description = null, string path = null, byte attributes = 0)
        {
            this.Category = category;
            this.Id = id;
            this.Name = name;
            this.Description = description;
            this.Path = path;
            this.Attributes = attributes;
        }

        public string Category { get; private set; }

        public string Id { get; private set; }

        public string Name { get; private set; }

        public string Description { get; private set; }

        public string Path { get; private set; }

        public byte Attributes { get; private set; }
    }
}
