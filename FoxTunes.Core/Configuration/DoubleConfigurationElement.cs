using System;

namespace FoxTunes
{
    [Serializable]
    public class DoubleConfigurationElement : ConfigurationElement<double>
    {
        public DoubleConfigurationElement(string id, string name = null, string description = null, string path = null) : base(id, name, description, path)
        {
        }
    }
}