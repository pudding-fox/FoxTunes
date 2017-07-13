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

        public uint? NumericValue { get; set; }

        public string TextValue { get; set; }

        public string FileValue { get; set; }

        public object Value
        {
            get
            {
                if (this.NumericValue.HasValue)
                {
                    return this.NumericValue.Value;
                }
                if (this.TextValue != null)
                {
                    return this.TextValue;
                }
                if (this.FileValue != null)
                {
                    return this.FileValue;
                }
                return null;
            }
        }
    }
}
