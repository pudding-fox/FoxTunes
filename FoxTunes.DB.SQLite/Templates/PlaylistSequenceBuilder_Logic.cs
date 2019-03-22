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

        public bool HasColumn(string name)
        {
            return this.MetaDataNames.IndexOf(name, StringComparer.OrdinalIgnoreCase) != -1;
        }

        public string GetColumn(string name)
        {
            var index = this.MetaDataNames.IndexOf(name, StringComparer.OrdinalIgnoreCase);
            if (index == -1)
            {
                throw new NotImplementedException();
            }
            return string.Format("Value_{0}_Value", index);
        }
    }
}
