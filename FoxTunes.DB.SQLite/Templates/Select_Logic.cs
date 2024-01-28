using System.Collections.Generic;

namespace FoxTunes.Templates
{
    public partial class Select
    {
        public Select(string table, params string[] filters)
        {
            this.Table = table;
            this.Filters = filters;
        }

        public string Table { get; private set; }

        public IEnumerable<string> Filters { get; private set; }
    }
}
