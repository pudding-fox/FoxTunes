using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TagLib;

namespace FoxTunes
{
    public class TagLibMetaDataSource : BaseComponent, IMetaDataSource
    {
        public static MetaDataCategory Categories = MetaDataCategory.Standard | MetaDataCategory.First;

        public static SemaphoreSlim Semaphore { get; private set; }

        static TagLibMetaDataSource()
        {
            Semaphore = new SemaphoreSlim(1, 1);
        }

        public async Task<IEnumerable<MetaDataItem>> GetMetaData(string fileName)
        {
            if (!this.IsSupported(fileName))
            {
                Logger.Write(this, LogLevel.Warn, "Unsupported file format: {0}", fileName);
                return Enumerable.Empty<MetaDataItem>();
            }
            var metaData = new List<MetaDataItem>();
            Logger.Write(this, LogLevel.Trace, "Reading meta data for file: {0}", fileName);
            try
            {
                var file = this.Create(fileName);
                this.AddMetaDatas(metaData, file.Tag);
                this.AddProperties(metaData, file.Properties);
                await this.AddImages(metaData, CommonMetaData.Pictures, file.Tag, file.Tag.Pictures);
            }
            catch (UnsupportedFormatException)
            {
                Logger.Write(this, LogLevel.Warn, "Unsupported file format: {0}", fileName);
            }
            return metaData;
        }

        protected virtual bool IsSupported(string fileName)
        {
            var mimeType = string.Format("taglib/{0}", fileName.GetExtension());
            return FileTypes.AvailableTypes.ContainsKey(mimeType);
        }

        protected virtual File Create(string fileName)
        {
            var file = File.Create(fileName);
            if (file.PossiblyCorrupt)
            {
                foreach (var reason in file.CorruptionReasons)
                {
                    Logger.Write(this, LogLevel.Debug, "Meta data corruption detected: {0} => {1}", fileName, reason);
                }
            }
            return file;
        }

        private void AddMetaDatas(IList<MetaDataItem> metaData, Tag tag)
        {
            if (Categories.HasFlag(MetaDataCategory.Standard))
            {
                this.AddMetaData(metaData, CommonMetaData.Album, tag.Album);
                this.AddMetaData(metaData, CommonMetaData.AlbumArtists, tag.AlbumArtists);
#pragma warning disable 612, 618
                this.AddMetaData(metaData, CommonMetaData.Artists, tag.Artists);
#pragma warning restore 612, 618
                this.AddMetaData(metaData, CommonMetaData.Composers, tag.Composers);
                this.AddMetaData(metaData, CommonMetaData.Conductor, tag.Conductor);
                this.AddMetaData(metaData, CommonMetaData.Disc, tag.Disc);
                this.AddMetaData(metaData, CommonMetaData.DiscCount, tag.DiscCount);
                this.AddMetaData(metaData, CommonMetaData.Genres, tag.Genres);
                this.AddMetaData(metaData, CommonMetaData.Performers, tag.Performers);
                this.AddMetaData(metaData, CommonMetaData.Title, tag.Title);
                this.AddMetaData(metaData, CommonMetaData.Track, tag.Track);
                this.AddMetaData(metaData, CommonMetaData.TrackCount, tag.TrackCount);
                this.AddMetaData(metaData, CommonMetaData.Year, tag.Year);
            }

            if (Categories.HasFlag(MetaDataCategory.First))
            {
                this.AddMetaData(metaData, CommonMetaData.FirstAlbumArtist, tag.FirstAlbumArtist);
#pragma warning disable 612, 618
                this.AddMetaData(metaData, CommonMetaData.FirstArtist, tag.FirstArtist);
#pragma warning restore 612, 618
                this.AddMetaData(metaData, CommonMetaData.FirstComposer, tag.FirstComposer);
                this.AddMetaData(metaData, CommonMetaData.FirstGenre, tag.FirstGenre);
                this.AddMetaData(metaData, CommonMetaData.FirstPerformer, tag.FirstPerformer);
            }

            if (Categories.HasFlag(MetaDataCategory.Joined))
            {
                this.AddMetaData(metaData, CommonMetaData.JoinedAlbumArtists, tag.JoinedAlbumArtists);
#pragma warning disable 612, 618
                this.AddMetaData(metaData, CommonMetaData.JoinedArtists, tag.JoinedArtists);
#pragma warning restore 612, 618
                this.AddMetaData(metaData, CommonMetaData.JoinedComposers, tag.JoinedComposers);
                this.AddMetaData(metaData, CommonMetaData.JoinedGenres, tag.JoinedGenres);
                this.AddMetaData(metaData, CommonMetaData.JoinedPerformers, tag.JoinedPerformers);
            }

            if (Categories.HasFlag(MetaDataCategory.Extended))
            {
                this.AddMetaData(metaData, CommonMetaData.MusicIpId, tag.MusicIpId);
                this.AddMetaData(metaData, CommonMetaData.AmazonId, tag.AmazonId);
                this.AddMetaData(metaData, CommonMetaData.BeatsPerMinute, tag.BeatsPerMinute);
                this.AddMetaData(metaData, CommonMetaData.Comment, tag.Comment);
                this.AddMetaData(metaData, CommonMetaData.Copyright, tag.Copyright);
                this.AddMetaData(metaData, CommonMetaData.Grouping, tag.Grouping);
                this.AddMetaData(metaData, CommonMetaData.Lyrics, tag.Lyrics);
            }

            if (Categories.HasFlag(MetaDataCategory.MusicBrainz))
            {
                this.AddMetaData(metaData, CommonMetaData.MusicBrainzArtistId, tag.MusicBrainzArtistId);
                this.AddMetaData(metaData, CommonMetaData.MusicBrainzDiscId, tag.MusicBrainzDiscId);
                this.AddMetaData(metaData, CommonMetaData.MusicBrainzReleaseArtistId, tag.MusicBrainzReleaseArtistId);
                this.AddMetaData(metaData, CommonMetaData.MusicBrainzReleaseCountry, tag.MusicBrainzReleaseCountry);
                this.AddMetaData(metaData, CommonMetaData.MusicBrainzReleaseId, tag.MusicBrainzReleaseId);
                this.AddMetaData(metaData, CommonMetaData.MusicBrainzReleaseStatus, tag.MusicBrainzReleaseStatus);
                this.AddMetaData(metaData, CommonMetaData.MusicBrainzReleaseType, tag.MusicBrainzReleaseType);
                this.AddMetaData(metaData, CommonMetaData.MusicBrainzTrackId, tag.MusicBrainzTrackId);
            }

            if (Categories.HasFlag(MetaDataCategory.Sort))
            {
                this.AddMetaData(metaData, CommonMetaData.TitleSort, tag.TitleSort);
                this.AddMetaData(metaData, CommonMetaData.PerformersSort, tag.PerformersSort);
                this.AddMetaData(metaData, CommonMetaData.JoinedPerformersSort, tag.JoinedPerformersSort);
                this.AddMetaData(metaData, CommonMetaData.ComposersSort, tag.ComposersSort);
                this.AddMetaData(metaData, CommonMetaData.AlbumArtistsSort, tag.AlbumArtistsSort);
                this.AddMetaData(metaData, CommonMetaData.AlbumSort, tag.AlbumSort);
                if (Categories.HasFlag(MetaDataCategory.First))
                {
                    this.AddMetaData(metaData, CommonMetaData.FirstPerformerSort, tag.FirstPerformerSort);
                    this.AddMetaData(metaData, CommonMetaData.FirstComposerSort, tag.FirstComposerSort);
                    this.AddMetaData(metaData, CommonMetaData.FirstAlbumArtistSort, tag.FirstAlbumArtistSort);
                }
            }
        }

        private void AddMetaData(IList<MetaDataItem> metaData, string name, uint? value)
        {
            this.AddMetaData(metaData, name, (int?)value);
        }

        private void AddMetaData(IList<MetaDataItem> metaData, string name, int? value)
        {
            if (!value.HasValue)
            {
                return;
            }
            metaData.Add(new MetaDataItem(name, MetaDataItemType.Tag) { NumericValue = value.Value });
        }

        private void AddMetaData(IList<MetaDataItem> metaData, string name, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }
            metaData.Add(new MetaDataItem(name, MetaDataItemType.Tag) { TextValue = value.Trim() });
        }

        private void AddMetaData(IList<MetaDataItem> metaData, string name, string[] values)
        {
            foreach (var value in values)
            {
                this.AddMetaData(metaData, name, value);
            }
        }

        private void AddProperties(IList<MetaDataItem> metaData, Properties properties)
        {
            if (Categories.HasFlag(MetaDataCategory.Standard))
            {
                this.AddProperty(metaData, CommonProperties.Duration, properties.Duration);
                this.AddProperty(metaData, CommonProperties.AudioBitrate, properties.AudioBitrate);
                this.AddProperty(metaData, CommonProperties.AudioChannels, properties.AudioChannels);
                this.AddProperty(metaData, CommonProperties.AudioSampleRate, properties.AudioSampleRate);
                this.AddProperty(metaData, CommonProperties.BitsPerSample, properties.BitsPerSample);
            }
            if (Categories.HasFlag(MetaDataCategory.MultiMedia))
            {
                this.AddProperty(metaData, CommonProperties.Description, properties.Description);
                this.AddProperty(metaData, CommonProperties.PhotoHeight, properties.PhotoHeight);
                this.AddProperty(metaData, CommonProperties.PhotoQuality, properties.PhotoQuality);
                this.AddProperty(metaData, CommonProperties.PhotoWidth, properties.PhotoWidth);
                this.AddProperty(metaData, CommonProperties.VideoHeight, properties.VideoHeight);
                this.AddProperty(metaData, CommonProperties.VideoWidth, properties.VideoWidth);
            }
        }

        private void AddProperty(IList<MetaDataItem> metaData, string name, int? value)
        {
            if (!value.HasValue)
            {
                return;
            }
            metaData.Add(new MetaDataItem(name, MetaDataItemType.Property) { NumericValue = value });
        }

        private void AddProperty(IList<MetaDataItem> metaData, string name, TimeSpan? value)
        {
            if (!value.HasValue)
            {
                return;
            }
            metaData.Add(new MetaDataItem(name, MetaDataItemType.Property) { NumericValue = Convert.ToInt32(value.Value.TotalMilliseconds) });
        }

        private void AddProperty(IList<MetaDataItem> metaData, string name, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }
            metaData.Add(new MetaDataItem(name, MetaDataItemType.Property) { TextValue = value.Trim() });
        }

        private async Task AddImages(IList<MetaDataItem> metaData, string name, Tag tag, IEnumerable<IPicture> pictures)
        {
            if (pictures == null)
            {
                return;
            }
            foreach (var picture in pictures)
            {
                var type = Enum.GetName(typeof(PictureType), picture.Type);
                var id = this.GetImageId(tag, picture, type);
                var fileName = await this.AddImage(picture, id);
                metaData.Add(new MetaDataItem(type, MetaDataItemType.Image)
                {
                    FileValue = fileName
                });
            }
        }

        private async Task<string> AddImage(IPicture value, string id)
        {
            var fileName = default(string);
            if (!FileMetaDataStore.Exists(id, out fileName))
            {
                await Semaphore.WaitAsync();
                try
                {
                    if (!FileMetaDataStore.Exists(id, out fileName))
                    {
                        return await FileMetaDataStore.Write(id, value.Data.Data);
                    }
                }
                finally
                {
                    Semaphore.Release();
                }
            }
            return fileName;
        }

#pragma warning disable 612, 618
        private string GetImageId(Tag tag, IPicture value, string type)
        {
            return string.Format(
                "{0}_{1}_{2}",
                tag.FirstAlbumArtist
                    .IfNullOrEmpty(tag.FirstAlbumArtistSort)
                    .IfNullOrEmpty(tag.FirstArtist),
                tag.Album,
                type
            );
        }
#pragma warning restore 612, 618
    }

    [Flags]
    public enum MetaDataCategory : byte
    {
        None = 0,
        Standard = 1,
        Extended = 2,
        First = 4,
        Sort = 8,
        Joined = 16,
        MusicBrainz = 32,
        MultiMedia = 64
    }
}
