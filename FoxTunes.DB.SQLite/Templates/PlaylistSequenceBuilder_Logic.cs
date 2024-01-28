using FoxDb.Interfaces;
using System;
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

        public string GetColumn(string name)
        {
            return string.Format(
                "Value_{0}_Value",
                this.MetaDataNames.IndexOf(name, StringComparer.OrdinalIgnoreCase)
            );
        }
    }
}
