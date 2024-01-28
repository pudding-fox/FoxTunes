using FoxTunes.Interfaces;
using ManagedBass.ZipStream;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class ArchivePlaylistItemFactory : BaseComponent
    {
        public ArchivePlaylistItemFactory(string fileName)
        {
            this.FileName = fileName;
        }

        public string FileName { get; private set; }

        public IOutput Output { get; private set; }

        public IMetaDataSourceFactory MetaDataSourceFactory { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Output = core.Components.Output;
            this.MetaDataSourceFactory = core.Factories.MetaDataSource;
            base.InitializeComponent(core);
        }

        public async Task<PlaylistItem[]> Create()
        {
            var playlistItems = new List<PlaylistItem>();
            var archive = default(IntPtr);
            if (Archive.Create(out archive))
            {
                try
                {
                    if (Archive.Open(archive, this.FileName))
                    {
                        await this.Create(archive, playlistItems).ConfigureAwait(false);
                    }
                    else
                    {
                        //TODO: Warn.
                    }
                }
                finally
                {
                    Archive.Release(archive);
                }
            }
            else
            {
                //TODO: Warn.
            }
            return playlistItems.ToArray();
        }

        protected virtual async Task Create(IntPtr archive, List<PlaylistItem> playlistItems)
        {
            var count = default(int);
            var directoryName = Path.GetDirectoryName(this.FileName);
            var metaDataSource = this.MetaDataSourceFactory.Create();
            if (Archive.GetEntryCount(archive, out count))
            {
                for (var a = 0; a < count; a++)
                {
                    var entry = default(Archive.ArchiveEntry);
                    if (Archive.GetEntry(archive, out entry, a))
                    {
                        if (!this.Output.IsSupported(entry.path))
                        {
                            continue;
                        }
                        var fileName = ArchiveUtils.CreateUrl(this.FileName, entry.path);
                        var playlistItem = new PlaylistItem()
                        {
                            DirectoryName = directoryName,
                            FileName = fileName
                        };
                        try
                        {
                            using (var fileAbstraction = ArchiveFileAbstraction.Create(this.FileName, entry.path, a))
                            {
                                if (fileAbstraction.IsOpen)
                                {
                                    playlistItem.MetaDatas = (
                                        await metaDataSource.GetMetaData(fileAbstraction).ConfigureAwait(false)
                                    ).ToList();
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Write(this, LogLevel.Debug, "Failed to read meta data from file \"{0}\": {1}", this.FileName, e.Message);
                        }
                        this.EnsureMetaData(a, entry, playlistItem);
                        playlistItems.Add(playlistItem);
                    }
                    else
                    {
                        //TODO: Warn.
                    }
                }
            }
            else
            {
                //TODO: Warn.
            }
        }

        protected virtual void EnsureMetaData(int index, Archive.ArchiveEntry entry, PlaylistItem playlistItem)
        {
            var hasTrack = false;
            var hasTitle = false;
            if (playlistItem.MetaDatas != null)
            {
                var metaData = playlistItem.MetaDatas.ToDictionary(
                    metaDataItem => metaDataItem.Name,
                    metaDataItem => metaDataItem.Value,
                    StringComparer.OrdinalIgnoreCase
                );
                hasTrack = metaData.ContainsKey(CommonMetaData.Track);
                hasTitle = metaData.ContainsKey(CommonMetaData.Title);
            }
            else
            {
                playlistItem.MetaDatas = new List<MetaDataItem>();
            }
            if (!hasTrack)
            {
                playlistItem.MetaDatas.Add(new MetaDataItem(CommonMetaData.Track, MetaDataItemType.Tag)
                {
                    Value = Convert.ToString(index + 1)
                });
            }
            if (!hasTitle)
            {
                playlistItem.MetaDatas.Add(new MetaDataItem(CommonMetaData.Title, MetaDataItemType.Tag)
                {
                    Value = entry.path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).LastOrDefault()
                });
            }
        }
    }
}