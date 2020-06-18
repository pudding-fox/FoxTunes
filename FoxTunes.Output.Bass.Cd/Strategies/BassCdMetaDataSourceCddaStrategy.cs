using FoxTunes.Interfaces;
using ManagedBass.Cd;
using System;
using System.Collections.Generic;

namespace FoxTunes
{
    public class BassCdMetaDataSourceCddaStrategy : BassCdMetaDataSourceStrategy
    {
        public static readonly object SyncRoot = new object();

        private static IDictionary<string, string> NAMES = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "TTITLE", CommonMetaData.Title },
            { "DARTIST", CommonMetaData.Artist },
            { "DALBUM", CommonMetaData.Album },
            { "DGENRE", CommonMetaData.Genre },
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
            try
            {
                //The CDDB and Read must be executed together so synchronize.
                lock (SyncRoot)
                {
                    var id = BassCd.GetID(this.Drive, CDID.CDDB);
                    var sequence = BassCd.GetID(this.Drive, CDID.Read);
                    this.Parser = new CddbTextParser(id, sequence);
                }
                if (this.Parser.Count == 0)
                {
                    Logger.Write(this, LogLevel.Debug, "CDDB did not return any information for drive: {0}", this.Drive);
                    return false;
                }
            }
            catch (Exception e)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to query CDDB for drive {0}: {1}", this.Drive, e.Message);
                return false;
            }
            return base.InitializeComponent();
        }

        public override IEnumerable<MetaDataItem> GetMetaDatas(int track)
        {
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var index in new[] { -1, track })
            {
                if (this.Parser != null && this.Parser.Contains(index))
                {
                    foreach (var element in this.Parser.Get(index))
                    {
                        var name = GetName(element.Key);
                        names.Add(name);
                        yield return new MetaDataItem(name, MetaDataItemType.Tag)
                        {
                            Value = element.Value
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
