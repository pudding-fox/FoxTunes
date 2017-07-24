using FoxTunes.Interfaces;

namespace FoxTunes
{
    public class StatisticItem : PersistableComponent, INamedValue
    {
        public StatisticItem()
        {

        }

        public StatisticItem(string name)
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
