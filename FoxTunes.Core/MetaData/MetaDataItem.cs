using FoxTunes.Interfaces;
using System.Text.RegularExpressions;

namespace FoxTunes
{
    public class MetaDataItem : PersistableComponent, INamedValue
    {
        public MetaDataItem()
        {

        }

        public MetaDataItem(string name, MetaDataItemType type)
        {
            this.Name = name;
            this.Type = type;
        }

        public string Name { get; set; }

        public MetaDataItemType Type { get; set; }

        public string Value { get; set; }

        public bool IsNumeric
        {
            get
            {
                if (string.IsNullOrEmpty(this.Value))
                {
                    return false;
                }
                foreach (char c in this.Value)
                {
                    if (c < '0' || c > '9')
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public bool IsFile
        {
            get
            {
                if (string.IsNullOrEmpty(this.Value))
                {
                    return false;
                }
                var regex = new Regex(@"((?:[a-zA-Z]\:(\\|\/)|file\:\/\/|\\\\|\.(\/|\\))([^\\\/\:\*\?\<\>\""\|]+(\\|\/){0,1})+)");
                var matches = regex.Matches(this.Value);
                for (var a = 0; a < matches.Count; a++)
                {
                    var match = matches[a];
                    if (match.Success)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public static readonly MetaDataItem Empty = new MetaDataItem();
    }

    public enum MetaDataItemType : byte
    {
        None = 0,
        Tag = 1,
        Property = 2,
        Image = 4,
        Statistic = 8,
        All = Tag | Property | Image | Statistic
    }
}
