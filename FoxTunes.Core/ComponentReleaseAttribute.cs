using FoxTunes.Interfaces;
using System;

namespace FoxTunes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ComponentReleaseAttribute : Attribute
    {
        public ComponentReleaseAttribute(ReleaseType releaseType)
        {
            this.ReleaseType = releaseType;
        }

        public ReleaseType ReleaseType { get; private set; }
    }
}
