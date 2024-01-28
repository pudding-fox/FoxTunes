using FoxTunes.Interfaces;

namespace FoxTunes
{
    public class InvocationComponent : IInvocationComponent
    {
        public const string CATEGORY_PLAYLIST = "42BC8F63-202C-4F77-8383-04A54FFFDCD5";

        public const string CATEGORY_LIBRARY = "CE91AB2A-3442-4019-B585-C4DCFEF1DB65";

        public const string CATEGORY_NOTIFY_ICON = "F9E060E6-D791-4E9A-A109-FD145EAECBA2";

        public const byte ATTRIBUTE_SEPARATOR = 1;

        public InvocationComponent(string category, string id, string name = null, string description = null, byte attributes = 0)
        {
            this.Category = category;
            this.Id = id;
            this.Name = name;
            this.Description = description;
            this.Attributes = attributes;
        }

        public string Category { get; private set; }

        public string Id { get; private set; }

        public string Name { get; private set; }

        public string Description { get; private set; }

        public byte Attributes { get; private set; }
    }
}
