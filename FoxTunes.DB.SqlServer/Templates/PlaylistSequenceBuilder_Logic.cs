using FoxDb.Interfaces;
using System;
using System.Collections.Generic;

namespace FoxTunes.Templates
{
    public partial class PlaylistSequenceBuilder
    {
        public PlaylistSequenceBuilder(IDatabase database)
        {
            this.Database = database;
            this.Columns = new[]
            {
                CustomMetaData.VariousArtists,
                CommonMetaData.Artist,
                CommonMetaData.Year,
                CommonMetaData.Album,
                CommonMetaData.Disc,
                CommonMetaData.Track
            };
        }

        public IDatabase Database { get; private set; }

        public IEnumerable<string> Columns { get; private set; }

        public string GetColumn(string name)
        {
            var index = this.Columns.IndexOf(name, StringComparer.OrdinalIgnoreCase);
            if (index == -1)
            {
                throw new NotImplementedException();
            }
            return string.Format("Value_{0}_Value", index);
        }
    }
}
