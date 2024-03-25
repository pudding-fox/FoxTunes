using FoxTunes.Interfaces;
using SacdSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class SacdItemFactory<T> : PopulatorBase where T : IFileData, new()
    {
        const string FILE_PREFIX = "bass_sacd.";

        const string FILE_SUFFIX = ".dsf";

        public SacdItemFactory(bool reportProgress) : base(reportProgress)
        {

        }

        public IMetaDataSourceFactory MetaDataSourceFactory { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.MetaDataSourceFactory = core.Factories.MetaDataSource;
            base.InitializeComponent(core);
        }

        public async Task<T[]> Create(IEnumerable<string> paths)
        {
            if (this.ReportProgress)
            {
                this.Name = Strings.SacdItemFactory_Name;
            }

            var items = new List<T>();
            foreach (var path in paths)
            {
                var sacd = SacdFactory.Instance.Create(path);
                sacd.InitialiseComponent();
                foreach (var area in sacd.Areas)
                {
                    foreach (var track in area.Tracks)
                    {
                        await this.Create(sacd, area, track, path, items).ConfigureAwait(false);
                    }
                }
            }
            return items.ToArray();
        }

        protected virtual async Task Create(Sacd sacd, SacdArea area, SacdTrack track, string path, List<T> items)
        {
            var metaDataSource = this.MetaDataSourceFactory.Create();
            if (this.ReportProgress)
            {
                var title = default(string);
                if (!track.Info.TryGetValue(CommonMetaData.Title, out title))
                {
                    title = string.Concat("Track ", area.Tracks.IndexOf(track) + 1);
                }
                this.Description = title;
            }
            var fileName = default(string);
            var directoryName = Path.GetTempPath();
            var extractor = SacdFactory.Instance.Create(sacd, area, track);
            if (!extractor.IsExtracted(directoryName, out fileName))
            {
                if (!extractor.Extract(directoryName, out fileName))
                {
                    return;
                }
            }
            fileName = this.Import(fileName);
            var item = new T()
            {
                DirectoryName = path,
                FileName = fileName
            };
            item.MetaDatas = (
                await metaDataSource.GetMetaData(fileName).ConfigureAwait(false)
            ).ToList();
            this.EnsureMetaData(fileName, sacd, area, track, item);
            items.Add(item);
        }

        protected virtual string Import(string fileName)
        {
            var name = string.Concat(FILE_PREFIX, Math.Abs(fileName.GetHashCode()), FILE_SUFFIX);
            var temp = Path.Combine(Path.GetTempPath(), name);
            if (!File.Exists(temp))
            {
                File.Move(fileName, temp);
            }
            return temp;
        }

        protected virtual void EnsureMetaData(string path, Sacd sacd, SacdArea area, SacdTrack track, T item)
        {
            var hasTrack = false;
            var hasTitle = false;
            if (item.MetaDatas != null)
            {
                var metaData = item.MetaDatas.ToDictionary(
                    metaDataItem => metaDataItem.Name,
                    metaDataItem => metaDataItem.Value,
                    StringComparer.OrdinalIgnoreCase
                );
                hasTrack = metaData.ContainsKey(CommonMetaData.Track);
                hasTitle = metaData.ContainsKey(CommonMetaData.Title);
            }
            else
            {
                item.MetaDatas = new List<MetaDataItem>();
            }
            if (!hasTrack)
            {
                item.MetaDatas.Add(new MetaDataItem(CommonMetaData.Track, MetaDataItemType.Tag)
                {
                    Value = (area.Tracks.IndexOf(track) + 1).ToString()
                });
            }
            if (!hasTitle)
            {
                item.MetaDatas.Add(new MetaDataItem(CommonMetaData.Title, MetaDataItemType.Tag)
                {
                    Value = Path.GetFileNameWithoutExtension(path)
                });
            }
            item.MetaDatas.Add(new MetaDataItem(CustomMetaData.SourceFileName, MetaDataItemType.Tag)
            {
                Value = path
            });
        }
    }
}
