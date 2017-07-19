using FoxTunes.Interfaces;

namespace FoxTunes
{
    public class PropertyItem : PersistableComponent, INamedValue
    {
        public PropertyItem()
        {

        }

        public PropertyItem(string name)
        {
            this.Name = name;
        }

        public string Name { get; set; }

        public int? NumericValue { get; set; }

        public string TextValue { get; set; }

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
                return null;
            }
        }
    }
}
