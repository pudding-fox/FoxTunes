using FoxTunes.Interfaces;
using ManagedBass.Cd;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class CdPlaylistItemFactory : BaseComponent
    {
        public CdPlaylistItemFactory(int drive, bool cdLookup, string cdLookupHost)
        {
            this.Drive = drive;
            this.CdLookup = cdLookup;
            this.CdLookupHost = cdLookupHost;
        }

        public int Drive { get; private set; }

        public bool CdLookup { get; private set; }

        public string CdLookupHost { get; private set; }

        public ICore Core { get; private set; }

        public IMetaDataSource MetaDataSource { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.MetaDataSource = new BassCdMetaDataSource(this.GetStrategy());
            this.MetaDataSource.InitializeComponent(this.Core);
            base.InitializeComponent(core);
        }

        public async Task<PlaylistItem[]> Create()
        {
            var info = default(CDInfo);
            BassUtils.OK(BassCd.GetInfo(this.Drive, out info));
            var id = BassCd.GetID(this.Drive, CDID.CDPlayer);
            var directoryName = string.Format("{0}:\\", info.DriveLetter);
            var playlistItems = new List<PlaylistItem>();
            for (int a = 0, b = BassCd.GetTracks(this.Drive); a < b; a++)
            {
                if (BassCd.GetTrackLength(this.Drive, a) == -1)
                {
                    //Not a music track.
                    continue;
                }
                var fileName = BassCdUtils.CreateUrl(this.Drive, id, a);
                var metaDatas = (
                    await this.MetaDataSource.GetMetaData(fileName).ConfigureAwait(false)
                ).ToArray();
                Logger.Write(this, LogLevel.Debug, "Adding file to playlist: {0}", fileName);
                var playlistItem = new PlaylistItem()
                {
                    DirectoryName = directoryName,
                    FileName = Path.Combine(fileName, this.GetFileName(fileName, a, metaDatas)),
                    Status = PlaylistItemStatus.Import
                };
                playlistItem.MetaDatas = metaDatas;
                playlistItems.Add(playlistItem);
            }
            return playlistItems.ToArray();
        }

        private string GetFileName(string fileName, int track, IEnumerable<MetaDataItem> metaDatas)
        {
            var metaData = metaDatas.ToDictionary(
                metaDataItem => metaDataItem.Name,
                metaDataItem => metaDataItem.Value,
                StringComparer.OrdinalIgnoreCase
            );
            var title = metaData.GetValueOrDefault(CommonMetaData.Title);
            if (!string.IsNullOrEmpty(title))
            {
                var sanitize = new Func<string, string>(value =>
                {
                    const char PLACEHOLDER = '_';
                    var characters = Enumerable.Concat(
                        Path.GetInvalidPathChars(),
                        Path.GetInvalidFileNameChars()
                    );
                    foreach (var character in characters)
                    {
                        value = value.Replace(character, PLACEHOLDER);
                    }
                    return value;
                });
                return string.Format("{0:00} - {1}.cda", track + 1, sanitize(title));
            }
            else
            {
                return string.Format("Track {0}.cda", track + 1);
            }
        }

        private IBassCdMetaDataSourceStrategy GetStrategy()
        {
            if (this.CdLookup)
            {
                var strategy = new BassCdMetaDataSourceCddaStrategy(this.Drive, this.CdLookupHost);
                if (strategy.InitializeComponent())
                {
                    return strategy;
                }
            }
            {
                var strategy = new BassCdMetaDataSourceCdTextStrategy(this.Drive);
                if (strategy.InitializeComponent())
                {
                    return strategy;
                }
            }
            return new BassCdMetaDataSourceStrategy(this.Drive);
        }
    }
}
