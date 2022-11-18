using System.Linq;

namespace FoxTunes
{
    public class IntegerConfigurationElement : ConfigurationElement<int>
    {
        public IntegerConfigurationElement(string id, string name = null, string description = null, string path = null) : base(id, name, description, path)
        {
        }

        public int MinValue
        {
            get
            {
                if (this.ValidationRules != null)
                {
                    foreach (var validationRule in this.ValidationRules.OfType<IntegerValidationRule>())
                    {
                        return validationRule.MinValue;
                    }
                }
                return int.MinValue;
            }
        }

        public int MaxValue
        {
            get
            {
                if (this.ValidationRules != null)
                {
                    foreach (var validationRule in this.ValidationRules.OfType<IntegerValidationRule>())
                    {
                        return validationRule.MaxValue;
                    }
                }
                return int.MaxValue;
            }
        }
    }
}