using FoxTunes.Interfaces;
using ManagedBass.Cd;
using System;
using System.Collections.Generic;

namespace FoxTunes
{
    public class BassCdMetaDataSourceCdTextStrategy : BassCdMetaDataSourceStrategy
    {
        private static IDictionary<string, string> NAMES = new Dictionary<string, string>()
        {
            { "TITLE", CommonMetaData.Title },
            { "PERFORMER", CommonMetaData.AlbumArtist },
            { "COMPOSER", CommonMetaData.Composer },
            { "GENRE", CommonMetaData.Genre }
        };

        public BassCdMetaDataSourceCdTextStrategy(int drive)
            : base(drive)
        {

        }

        public CdTextParser Parser { get; private set; }

        public override bool InitializeComponent()
        {
            Logger.Write(this, LogLevel.Debug, "Querying CD-TEXT for drive: {0}", this.Drive);
            this.Parser = new CdTextParser(BassCd.GetIDText(this.Drive));
            if (this.Parser.Count == 0)
            {
                Logger.Write(this, LogLevel.Debug, "CD-TEXT did not return any information for drive: {0}", this.Drive);
                return false;
            }
            return base.InitializeComponent();
        }

        public override IEnumerable<MetaDataItem> GetMetaDatas(int track)
        {
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (this.Parser.Contains(track))
            {
                foreach (var element in this.Parser.Get(track))
                {
                    var name = GetName(element.Key);
                    names.Add(name);
                    yield return new MetaDataItem(name, MetaDataItemType.Tag)
                    {
                        TextValue = element.Value
                    };
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
