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

        //2MB
        public static int MAX_IMAGE_SIZE = 2048000;

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
                using (var file = this.Create(fileName))
                {
                    this.AddTags(metaData, file.Tag);
                    this.AddProperties(metaData, file.Properties);
                    await this.AddImages(metaData, CommonMetaData.Pictures, file, file.Tag, file.Tag.Pictures);
                }
            }
            catch (UnsupportedFormatException)
            {
                Logger.Write(this, LogLevel.Warn, "Unsupported file format: {0}", fileName);
            }
            return metaData;
        }

        public async Task SetMetaData(string fileName, IEnumerable<MetaDataItem> metaData)
        {
            try
            {
                using (var file = this.Create(fileName))
                {
                    await this.SetMetaDatas(metaData, file.Tag);
                    file.Save();
                }
            }
            catch (UnsupportedFormatException)
            {
                Logger.Write(this, LogLevel.Warn, "Unsupported file format: {0}", fileName);
            }
        }

        protected virtual bool IsSupported(string fileName)
        {
            var mimeType = string.Format("taglib/{0}", fileName.GetExtension());
            return FileTypes.AvailableTypes.ContainsKey(mimeType);
        }

        protected virtual bool IsSupported(File file, Tag tag, IPicture picture)
        {
            if (picture.Data.Data.Length > MAX_IMAGE_SIZE)
            {
                Logger.Write(this, LogLevel.Warn, "Not importing image from file \"{0}\" due to size: {1} > {2}", file.Name, picture.Data.Data.Length, MAX_IMAGE_SIZE);
                return false;
            }
            return true;
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

        private void AddTags(IList<MetaDataItem> metaData, Tag tag)
        {
            if (Categories.HasFlag(MetaDataCategory.Standard))
            {
                this.AddTag(metaData, CommonMetaData.Album, tag.Album);
                this.AddTag(metaData, CommonMetaData.Artist, tag.FirstAlbumArtist);
                this.AddTag(metaData, CommonMetaData.Composer, tag.FirstComposer);
                this.AddTag(metaData, CommonMetaData.Conductor, tag.Conductor);
                this.AddTag(metaData, CommonMetaData.Disc, tag.Disc.ToString());
                this.AddTag(metaData, CommonMetaData.DiscCount, tag.DiscCount.ToString());
                this.AddTag(metaData, CommonMetaData.Genre, tag.FirstGenre);
                this.AddTag(metaData, CommonMetaData.Performer, tag.FirstPerformer);
                this.AddTag(metaData, CommonMetaData.Title, tag.Title);
                this.AddTag(metaData, CommonMetaData.Track, tag.Track.ToString());
                this.AddTag(metaData, CommonMetaData.TrackCount, tag.TrackCount.ToString());
                this.AddTag(metaData, CommonMetaData.Year, tag.Year.ToString());
            }

            if (Categories.HasFlag(MetaDataCategory.Extended))
            {
                this.AddTag(metaData, CommonMetaData.MusicIpId, tag.MusicIpId);
                this.AddTag(metaData, CommonMetaData.AmazonId, tag.AmazonId);
                this.AddTag(metaData, CommonMetaData.BeatsPerMinute, tag.BeatsPerMinute.ToString());
                this.AddTag(metaData, CommonMetaData.Comment, tag.Comment);
                this.AddTag(metaData, CommonMetaData.Copyright, tag.Copyright);
                this.AddTag(metaData, CommonMetaData.Grouping, tag.Grouping);
                this.AddTag(metaData, CommonMetaData.Lyrics, tag.Lyrics);
            }

            if (Categories.HasFlag(MetaDataCategory.MusicBrainz))
            {
                this.AddTag(metaData, CommonMetaData.MusicBrainzArtistId, tag.MusicBrainzArtistId);
                this.AddTag(metaData, CommonMetaData.MusicBrainzDiscId, tag.MusicBrainzDiscId);
                this.AddTag(metaData, CommonMetaData.MusicBrainzReleaseArtistId, tag.MusicBrainzReleaseArtistId);
                this.AddTag(metaData, CommonMetaData.MusicBrainzReleaseCountry, tag.MusicBrainzReleaseCountry);
                this.AddTag(metaData, CommonMetaData.MusicBrainzReleaseId, tag.MusicBrainzReleaseId);
                this.AddTag(metaData, CommonMetaData.MusicBrainzReleaseStatus, tag.MusicBrainzReleaseStatus);
                this.AddTag(metaData, CommonMetaData.MusicBrainzReleaseType, tag.MusicBrainzReleaseType);
                this.AddTag(metaData, CommonMetaData.MusicBrainzTrackId, tag.MusicBrainzTrackId);
            }

            if (Categories.HasFlag(MetaDataCategory.Sort))
            {
                this.AddTag(metaData, CommonMetaData.TitleSort, tag.TitleSort);
                this.AddTag(metaData, CommonMetaData.PerformerSort, tag.FirstPerformerSort);
                this.AddTag(metaData, CommonMetaData.ComposerSort, tag.FirstComposerSort);
                this.AddTag(metaData, CommonMetaData.ArtistSort, tag.FirstAlbumArtistSort);
                this.AddTag(metaData, CommonMetaData.AlbumSort, tag.AlbumSort);
            }
        }

        private void AddTag(IList<MetaDataItem> metaData, string name, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }
            metaData.Add(new MetaDataItem(name, MetaDataItemType.Tag) { Value = value.Trim() });
        }

        private void AddProperties(IList<MetaDataItem> metaData, Properties properties)
        {
            if (Categories.HasFlag(MetaDataCategory.Standard))
            {
                this.AddProperty(metaData, CommonProperties.Duration, properties.Duration.TotalMilliseconds.ToString());
                this.AddProperty(metaData, CommonProperties.AudioBitrate, properties.AudioBitrate.ToString());
                this.AddProperty(metaData, CommonProperties.AudioChannels, properties.AudioChannels.ToString());
                this.AddProperty(metaData, CommonProperties.AudioSampleRate, properties.AudioSampleRate.ToString());
                this.AddProperty(metaData, CommonProperties.BitsPerSample, properties.BitsPerSample.ToString());
            }
            if (Categories.HasFlag(MetaDataCategory.MultiMedia))
            {
                this.AddProperty(metaData, CommonProperties.Description, properties.Description);
                this.AddProperty(metaData, CommonProperties.PhotoHeight, properties.PhotoHeight.ToString());
                this.AddProperty(metaData, CommonProperties.PhotoQuality, properties.PhotoQuality.ToString());
                this.AddProperty(metaData, CommonProperties.PhotoWidth, properties.PhotoWidth.ToString());
                this.AddProperty(metaData, CommonProperties.VideoHeight, properties.VideoHeight.ToString());
                this.AddProperty(metaData, CommonProperties.VideoWidth, properties.VideoWidth.ToString());
            }
        }

        private void AddProperty(IList<MetaDataItem> metaData, string name, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }
            metaData.Add(new MetaDataItem(name, MetaDataItemType.Property) { Value = value.Trim() });
        }

        private async Task AddImages(IList<MetaDataItem> metaData, string name, File file, Tag tag, IPicture[] pictures)
        {
            if (pictures == null)
            {
                return;
            }
            var types = new List<ArtworkType>();
            foreach (var picture in pictures)
            {
                var type = GetArtworkType(picture.Type);
                if (!ArtworkTypes.HasFlag(type) || types.Contains(type))
                {
                    continue;
                }
                if (!this.IsSupported(file, tag, picture))
                {
                    continue;
                }
                metaData.Add(new MetaDataItem(Enum.GetName(typeof(ArtworkType), type), MetaDataItemType.Image)
                {
                    Value = await this.ImportImage(tag, picture, type, false)
                });
                types.Add(type);
            }
        }

        private async Task<string> ImportImage(Tag tag, IPicture picture, ArtworkType type, bool overwrite)
        {
            return await this.AddImage(picture, picture.Data.Checksum.ToString(), overwrite);
        }

        private async Task<string> AddImage(IPicture value, string id, bool overwrite)
        {
            var prefix = this.GetType().Name;
            var fileName = default(string);
            if (overwrite || !FileMetaDataStore.Exists(prefix, id, out fileName))
            {
#if NET40
                Semaphore.Wait();
#else
                await Semaphore.WaitAsync();
#endif
                try
                {
                    if (overwrite || !FileMetaDataStore.Exists(prefix, id, out fileName))
                    {
                        return await FileMetaDataStore.WriteAsync(prefix, id, value.Data.Data);
                    }
                }
                finally
                {
                    Semaphore.Release();
                }
            }
            return fileName;
        }

        private async Task SetMetaDatas(IEnumerable<MetaDataItem> metaDataItems, Tag tag)
        {
            foreach (var metaDataItem in metaDataItems)
            {
                switch (metaDataItem.Type)
                {
                    case MetaDataItemType.Tag:
                        this.SetTag(metaDataItem, tag);
                        break;
                    case MetaDataItemType.Image:
                        await this.SetImage(metaDataItem, tag);
                        break;
                }
            }
        }

        private void SetTag(MetaDataItem metaDataItem, Tag tag)
        {
            switch (metaDataItem.Name)
            {
                case CommonMetaData.Album:
                    tag.Album = metaDataItem.Value;
                    break;
                case CommonMetaData.Artist:
                    tag.AlbumArtists = new[] { metaDataItem.Value };
                    break;
                case CommonMetaData.Composer:
                    tag.Composers = new[] { metaDataItem.Value };
                    break;
                case CommonMetaData.Conductor:
                    tag.Conductor = metaDataItem.Value;
                    break;
                case CommonMetaData.Disc:
                    tag.Disc = Convert.ToUInt32(metaDataItem.Value);
                    break;
                case CommonMetaData.DiscCount:
                    tag.DiscCount = Convert.ToUInt32(metaDataItem.Value);
                    break;
                case CommonMetaData.Genre:
                    tag.Genres = new[] { metaDataItem.Value };
                    break;
                case CommonMetaData.Performer:
                    tag.Performers = new[] { metaDataItem.Value };
                    break;
                case CommonMetaData.Title:
                    tag.Title = metaDataItem.Value;
                    break;
                case CommonMetaData.Track:
                    tag.Track = Convert.ToUInt32(metaDataItem.Value);
                    break;
                case CommonMetaData.TrackCount:
                    tag.TrackCount = Convert.ToUInt32(metaDataItem.Value);
                    break;
                case CommonMetaData.Year:
                    tag.Year = Convert.ToUInt32(metaDataItem.Value);
                    break;
            }
        }

        private async Task SetImage(MetaDataItem metaDataItem, Tag tag)
        {
            var index = default(int);
            var pictures = new List<IPicture>(tag.Pictures);
            if (this.HasImage(metaDataItem.Name, tag, pictures, out index))
            {
                if (!string.IsNullOrEmpty(metaDataItem.Value))
                {
                    await this.ReplaceImage(metaDataItem, tag, pictures, index);
                }
                else
                {
                    this.RemoveImage(metaDataItem, tag, pictures, index);
                }
            }
            else if (!string.IsNullOrEmpty(metaDataItem.Value))
            {
                await this.AddImage(metaDataItem, tag, pictures);
            }
            pictures.Sort(PictureComparer.Instance);
            tag.Pictures = pictures.ToArray();
        }

        private bool HasImage(string name, Tag tag, IList<IPicture> pictures, out int index)
        {
            var type = GetArtworkType(name);
            for (var a = 0; a < pictures.Count; a++)
            {
                if (pictures[a] != null && GetArtworkType(pictures[a].Type) == type)
                {
                    index = a;
                    return true;
                }
            }
            index = default(int);
            return false;
        }

        private async Task AddImage(MetaDataItem metaDataItem, Tag tag, IList<IPicture> pictures)
        {
            pictures.Add(await this.CreateImage(metaDataItem, tag));
        }

        private async Task ReplaceImage(MetaDataItem metaDataItem, Tag tag, IList<IPicture> pictures, int index)
        {
            pictures[index] = await this.CreateImage(metaDataItem, tag);
        }

        private void RemoveImage(MetaDataItem metaDataItem, Tag tag, IList<IPicture> pictures, int index)
        {
            pictures.RemoveAt(index);
        }

        private async Task<IPicture> CreateImage(MetaDataItem metaDataItem, Tag tag)
        {
            var type = GetArtworkType(metaDataItem.Name);
            var picture = new Picture(metaDataItem.Value)
            {
                Type = GetPictureType(type)
            };
            metaDataItem.Value = await this.ImportImage(tag, picture, type, true);
            return picture;
        }

        public static readonly IDictionary<ArtworkType, PictureType> ArtworkTypeMapping = new Dictionary<ArtworkType, PictureType>()
        {
            { ArtworkType.FrontCover, PictureType.FrontCover },
            { ArtworkType.BackCover, PictureType.BackCover },
        };

        public static PictureType GetPictureType(ArtworkType artworkType)
        {
            var pictureType = default(PictureType);
            if (ArtworkTypeMapping.TryGetValue(artworkType, out pictureType))
            {
                return pictureType;
            }
            return PictureType.NotAPicture;
        }

        public static readonly IDictionary<PictureType, ArtworkType> PictureTypeMapping = new Dictionary<PictureType, ArtworkType>()
        {
            { PictureType.FrontCover, ArtworkType.FrontCover },
            { PictureType.BackCover, ArtworkType.BackCover },
        };

        public static ArtworkType GetArtworkType(string name)
        {
            var artworkType = default(ArtworkType);
            if (Enum.TryParse<ArtworkType>(name, out artworkType))
            {
                return artworkType;
            }
            return ArtworkType.Unknown;
        }

        public static ArtworkType GetArtworkType(PictureType pictureType)
        {
            var artworkType = default(ArtworkType);
            if (PictureTypeMapping.TryGetValue(pictureType, out artworkType))
            {
                return artworkType;
            }
            return ArtworkType.Unknown;
        }

        private class PictureComparer : IComparer<IPicture>
        {
            public int Compare(IPicture x, IPicture y)
            {
                var a = this.GetPriority(x);
                var b = this.GetPriority(y);
                if (a == b)
                {
                    return 0;
                }
                if (a > b)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            }

            protected virtual byte GetPriority(IPicture picture)
            {
                switch (picture.Type)
                {
                    case PictureType.FrontCover:
                        return 0;
                    case PictureType.BackCover:
                        return 1;
                }
                return 255;
            }

            public static readonly IComparer<IPicture> Instance = new PictureComparer();
        }
    }
}
