namespace FoxTunes.Templates
{
    public partial class GetTableInfo
    {
        public GetTableInfo(string table)
        {
            this.Table = table;
        }

        public string Table { get; private set; }
    }
}
