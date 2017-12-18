using System.Collections.Generic;

namespace FoxTunes.Templates
{
    public partial class PivotViewBuilder
    {
        public PivotViewBuilder(string table, IEnumerable<string> keyColumns, IEnumerable<string> nameColumns, IEnumerable<string> valueColumns, IEnumerable<string> values)
        {
            this.Table = table;
            this.KeyColumns = keyColumns;
            this.NameColumns = nameColumns;
            this.ValueColumns = valueColumns;
            this.Values = values;
        }

        public string Table { get; private set; }

        public IEnumerable<string> KeyColumns { get; private set; }

        public IEnumerable<string> NameColumns { get; private set; }

        public IEnumerable<string> ValueColumns { get; private set; }

        public IEnumerable<string> Values { get; private set; }
    }
}
