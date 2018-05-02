using System;
using System.Collections.Generic;
using ManagedBass.Cd;
using FoxTunes.Interfaces;

namespace FoxTunes
{
    public class BassCdMetaDataSourceCddaStrategy : BassCdMetaDataSourceStrategy
    {
        private static IDictionary<string, string> NAMES = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "TTITLE", CommonMetaData.Title },
            { "DARTIST", CommonMetaData.FirstAlbumArtist },
            { "DALBUM", CommonMetaData.Album },
            { "DGENRE", CommonMetaData.FirstGenre },
            { "DYEAR", CommonMetaData.Year }
        };

        public BassCdMetaDataSourceCddaStrategy(int drive, string host)
            : base(drive)
        {
            if (!string.Equals(BassCd.CDDBServer, host, StringComparison.OrdinalIgnoreCase))
            {
                BassCd.CDDBServer = host;
            }
        }

        public CddbTextParser Parser { get; private set; }

        public override bool InitializeComponent()
        {
            Logger.Write(this, LogLevel.Debug, "Querying CDDB for drive: {0}", this.Drive);
            this.Parser = new CddbTextParser(BassCd.GetID(this.Drive, CDID.CDDB), BassCd.GetID(this.Drive, CDID.Read));
            if (this.Parser.Count == 0)
            {
                Logger.Write(this, LogLevel.Debug, "CDDB did not return any information for drive: {0}", this.Drive);
                return false;
            }
            return base.InitializeComponent();
        }

        public override IEnumerable<MetaDataItem> GetMetaDatas(int track)
        {
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var index in new[] { -1, track })
            {
                if (this.Parser.Contains(index))
                {
                    foreach (var element in this.Parser.Get(index))
                    {
                        var name = GetName(element.Key);
                        names.Add(name);
                        yield return new MetaDataItem(name, MetaDataItemType.Tag)
                        {
                            TextValue = element.Value
                        };
                    }
                }
            }
            foreach (var metaDataItem in base.GetMetaDatas(track))
            {
                if (names.Contains(metaDataItem.Name))
                {
                    continue;
                }
                yield return metaDataItem;
            }
        }

        private static string GetName(string name)
        {
            if (NAMES.ContainsKey(name))
            {
                return NAMES[name];
            }
            return name;
        }
    }
}
