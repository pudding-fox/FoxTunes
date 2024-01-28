using System;

namespace FoxTunes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class BassEncoderAttribute : Attribute
    {
        public BassEncoderAttribute(string name)
        {
            this.Name = name;
        }

        public string Name { get; private set; }
    }
}
