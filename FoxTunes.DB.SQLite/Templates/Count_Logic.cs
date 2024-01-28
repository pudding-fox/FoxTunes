namespace FoxTunes.Templates
{
    public partial class Count
    {
        public Count(string table)
        {
            this.Table = table;
        }

        public string Table { get; private set; }
    }
}
