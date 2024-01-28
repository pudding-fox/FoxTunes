using FoxTunes.Interfaces;

namespace FoxTunes
{
    public class MetaDataItem : PersistableComponent, INamedValue
    {
        public MetaDataItem()
        {

        }

        public MetaDataItem(string name)
        {
            this.Name = name;
        }

        public string Name { get; set; }

        public int? NumericValue { get; set; }

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

        public static readonly MetaDataItem Empty = new MetaDataItem();
    }
}
