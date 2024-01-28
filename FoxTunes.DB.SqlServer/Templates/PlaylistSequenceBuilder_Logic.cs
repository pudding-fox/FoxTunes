using FoxDb.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes.Templates
{
    public partial class PlaylistSequenceBuilder
    {
        public PlaylistSequenceBuilder(IDatabase database, IEnumerable<string> metaDataNames)
        {
            this.Database = database;
            this.MetaDataNames = metaDataNames.ToArray();
        }

        public IDatabase Database { get; private set; }

        public string[] MetaDataNames { get; private set; }
    }
}
