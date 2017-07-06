using FoxTunes.Interfaces;
using System.Linq;

namespace FoxTunes
{
    public class MetaDataItem : BaseComponent, IMetaDataItem
    {
        public MetaDataItem(string name, object value)
            : this(name, new[] { value })
        {

        }

        public MetaDataItem(string name, object[] values)
        {
            this.Name = name;
            this.RawValues = values;
        }

        public string Name { get; private set; }

        public object[] RawValues { get; private set; }

        public object Value
        {
            get
            {
                return this.RawValues.FirstOrDefault();
            }
        }

        public object[] Values
        {
            get
            {
                return this.RawValues;
            }
        }
    }
}
