namespace FoxTunes.Templates
{
    public partial class Find
    {
        public Find(string table)
        {
            this.Table = table;
        }

        public string Table { get; private set; }
    }
}
