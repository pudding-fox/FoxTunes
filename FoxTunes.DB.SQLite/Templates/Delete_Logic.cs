namespace FoxTunes.Templates
{
    public partial class Delete
    {
        public Delete(string table)
        {
            this.Table = table;
        }

        public string Table { get; private set; }
    }
}
