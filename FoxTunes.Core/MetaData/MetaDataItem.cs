using FoxTunes.Interfaces;
using System.Linq;

namespace FoxTunes
{
    public class MetaDataItem : PersistableComponent
    {
        public MetaDataItem()
        {

        }

        public MetaDataItem(string name)
        {
            this.Name = name;
        }

        public string Name { get; set; }

        public uint NumericValue { get; set; }

        public string TextValue { get; set; }

        public string FileValue { get; set; }
    }
}
