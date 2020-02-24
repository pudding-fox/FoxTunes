using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System.Linq;
using System.Text;

namespace FoxTunes.Templates
{
    public partial class GetLibraryHierarchyNodes
    {
        public GetLibraryHierarchyNodes(IDatabase database, IFilterParserResult filter)
        {
            this.Database = database;
            this.Filter = filter;
        }

        public IDatabase Database { get; private set; }

        public IFilterParserResult Filter { get; private set; }
    }
}
