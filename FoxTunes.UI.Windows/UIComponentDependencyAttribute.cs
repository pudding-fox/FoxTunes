using System;

namespace FoxTunes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class UIComponentDependencyAttribute : Attribute
    {
        public UIComponentDependencyAttribute(string section, string element)
        {
            this.Section = section;
            this.Element = element;
        }

        public string Section { get; private set; }

        public string Element { get; private set; }
    }
}
