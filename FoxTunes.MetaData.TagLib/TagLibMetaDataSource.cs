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
        public static MetaDataCategory Categories = MetaDataCategory.Standard;

        public static ArtworkType ArtworkTypes = ArtworkType.FrontCover;

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
                this.AddMetaData(metaData, CommonMetaData.AlbumArtist, tag.FirstAlbumArtist);
#pragma warning disable 612, 618
                this.AddMetaData(metaData, CommonMetaData.Artist, tag.FirstArtist);
#pragma warning restore 612, 618
                this.AddMetaData(metaData, CommonMetaData.Composer, tag.FirstComposer);
                this.AddMetaData(metaData, CommonMetaData.Conductor, tag.Conductor);
                this.AddMetaData(metaData, CommonMetaData.Disc, tag.Disc);
                this.AddMetaData(metaData, CommonMetaData.DiscCount, tag.DiscCount);
                this.AddMetaData(metaData, CommonMetaData.Genre, tag.FirstGenre);
                this.AddMetaData(metaData, CommonMetaData.Performer, tag.FirstPerformer);
                this.AddMetaData(metaData, CommonMetaData.Title, tag.Title);
                this.AddMetaData(metaData, CommonMetaData.Track, tag.Track);
                this.AddMetaData(metaData, CommonMetaData.TrackCount, tag.TrackCount);
                this.AddMetaData(metaData, CommonMetaData.Year, tag.Year);
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
                this.AddMetaData(metaData, CommonMetaData.PerformerSort, tag.FirstPerformerSort);
                this.AddMetaData(metaData, CommonMetaData.ComposerSort, tag.FirstComposerSort);
                this.AddMetaData(metaData, CommonMetaData.AlbumArtistSort, tag.FirstAlbumArtistSort);
                this.AddMetaData(metaData, CommonMetaData.AlbumSort, tag.AlbumSort);
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
                if (!ArtworkTypes.HasFlag(GetArtworkType(picture.Type)))
                {
                    continue;
                }
                var type = Enum.GetName(typeof(PictureType), picture.Type);
                var id = this.GetImageId(tag, type);
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
#if NET40
                Semaphore.Wait();
#else
                await Semaphore.WaitAsync();
#endif
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
        private string GetImageId(Tag tag, string type)
        {
            var hashCode = default(int);
            //Hopefully this is unique enough. We can't use the artist as compilations are not reliable.
            foreach (var value in new object[] { tag.Year, tag.Album })
            {
                if (value == null)
                {
                    continue;
                }
                hashCode += value.GetHashCode();
            }
            return hashCode.ToString();
        }
#pragma warning restore 612, 618

        public static readonly IDictionary<PictureType, ArtworkType> PictureTypeMapping = new Dictionary<PictureType, ArtworkType>()
        {
            { PictureType.FrontCover, ArtworkType.FrontCover },
            { PictureType.BackCover, ArtworkType.BackCover },
        };

        public static ArtworkType GetArtworkType(PictureType pictureType)
        {
            var artworkType = default(ArtworkType);
            if (PictureTypeMapping.TryGetValue(pictureType, out artworkType))
            {
                return artworkType;
            }
            return ArtworkType.Unknown;
        }
    }
}
