using System.Linq;

namespace FoxTunes
{
    public class TagLibMetaDataItem : MetaDataItem
    {
        public TagLibMetaDataItem(string name, object value)
            : this(name, new[] { value })
        {

        }

        public TagLibMetaDataItem(string name, object[] values)
        {
            this.Name = name;
            this.RawValues = values;
        }

        public object[] RawValues { get; private set; }

        public override object Value
        {
            get
            {
                return this.RawValues.FirstOrDefault();
            }
        }

        public override object[] Values
        {
            get
            {
                return this.RawValues;
            }
        }
    }
}
