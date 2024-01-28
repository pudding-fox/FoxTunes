using FoxDb.Interfaces;

namespace FoxTunes.Templates
{
    public partial class GetPlaylistMetaData
    {
        public GetPlaylistMetaData(IDatabase database, int count)
        {
            this.Database = database;
            this.Count = count;
        }

        public IDatabase Database { get; private set; }

        public int Count { get; private set; }
    }
}
