using System.Collections.Generic;
using System.Linq;

namespace FoxTunes.Templates
{
    public partial class LibraryHierarchyBuilder
    {
        public LibraryHierarchyBuilder(IEnumerable<string> metaDataNames)
        {
            this.MetaDataNames = metaDataNames.ToArray();
        }

        public string[] MetaDataNames { get; private set; }
    }
}
