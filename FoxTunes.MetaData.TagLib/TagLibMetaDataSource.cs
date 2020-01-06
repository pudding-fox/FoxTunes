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

        //10MB
        public static int MAX_TAG_SIZE = 10240000;

        //2MB
        public static int MAX_IMAGE_SIZE = 2048000;

        static TagLibMetaDataSource()
        {
            Semaphore = new SemaphoreSlim(1, 1);
        }

        public IConfiguration Configuration { get; private set; }

        public BooleanConfigurationElement EmbeddedImages { get; private set; }

        public BooleanConfigurationElement LooseImages { get; private set; }

        public BooleanConfigurationElement CopyImages { get; private set; }

        public IArtworkProvider ArtworkProvider { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Configuration = core.Components.Configuration;
            this.EmbeddedImages = this.Configuration.GetElement<BooleanConfigurationElement>(
                MetaDataBehaviourConfiguration.SECTION,
                MetaDataBehaviourConfiguration.READ_EMBEDDED_IMAGES
            );
            this.LooseImages = this.Configuration.GetElement<BooleanConfigurationElement>(
                MetaDataBehaviourConfiguration.SECTION,
                MetaDataBehaviourConfiguration.READ_LOOSE_IMAGES
            );
            this.CopyImages = this.Configuration.GetElement<BooleanConfigurationElement>(
                MetaDataBehaviourConfiguration.SECTION,
                MetaDataBehaviourConfiguration.COPY_IMAGES_ELEMENT
            );
            this.ArtworkProvider = core.Components.ArtworkProvider;
            base.InitializeComponent(core);
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
                var collect = default(bool);
                var images = default(bool);
                using (var file = this.Create(fileName))
                {
                    if (file.Tag != null)
                    {
                        this.AddTags(metaData, file.Tag);
                    }
                    if (file.Properties != null)
                    {
                        this.AddProperties(metaData, file.Properties);
                    }
                    if (this.EmbeddedImages.Value)
                    {
                        if (file.InvariantStartPosition > MAX_TAG_SIZE)
                        {
                            Logger.Write(this, LogLevel.Warn, "Not importing images from file \"{0}\" due to size: {1} > {2}", file.Name, file.InvariantStartPosition, MAX_TAG_SIZE);
                            collect = true;
                        }
                        else
                        {
                            var pictures = file.Tag.Pictures;
                            if (pictures != null)
                            {
                                images = await this.AddImages(metaData, CommonMetaData.Pictures, file, file.Tag, pictures).ConfigureAwait(false);
                            }
                        }
                    }
                }
                if (collect)
                {
                    //If we encountered a large meta data section (>10MB) then we need to try to reclaim the memory.
                    GC.Collect();
                }
                if (this.LooseImages.Value && !images)
                {
                    await this.AddImages(metaData, fileName).ConfigureAwait(false);
                }
            }
            catch (UnsupportedFormatException)
            {
                Logger.Write(this, LogLevel.Warn, "Unsupported file format: {0}", fileName);
            }
            catch (Exception e)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to read meta data: {0} => {1}", fileName, e.Message);
            }
            return metaData;
        }

        public async Task SetMetaData(string fileName, IEnumerable<MetaDataItem> metaData)
        {
            try
            {
                var collect = default(bool);
                using (var file = this.Create(fileName))
                {
                    foreach (var metaDataItem in metaData)
                    {
                        switch (metaDataItem.Type)
                        {
                            case MetaDataItemType.Tag:
                                this.SetTag(metaDataItem, file.Tag);
                                break;
                            case MetaDataItemType.Image:
                                if (file.InvariantStartPosition > MAX_TAG_SIZE)
                                {
                                    Logger.Write(this, LogLevel.Warn, "Not exporting images to file \"{0}\" due to size: {1} > {2}", file.Name, file.InvariantStartPosition, MAX_TAG_SIZE);
                                    collect = true;
                                }
                                else
                                {
                                    await this.SetImage(metaDataItem, file.Tag).ConfigureAwait(false);
                                }
                                break;
                        }
                    }
                    file.Save();
                }
                if (collect)
                {
                    //If we encountered a large meta data section (>10MB) then we need to try to reclaim the memory.
                    GC.Collect();
                }
            }
            catch (UnsupportedFormatException)
            {
                Logger.Write(this, LogLevel.Warn, "Unsupported file format: {0}", fileName);
            }
            catch (Exception e)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to write meta data: {0} => {1}", fileName, e.Message);
            }
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

        private void AddTags(IList<MetaDataItem> metaData, Tag tag)
        {
            if (Categories.HasFlag(MetaDataCategory.Standard))
            {
                this.Try(() => this.AddTag(metaData, CommonMetaData.Album, tag.Album), this.ErrorHandler);
                this.Try(() =>
                {
                    if (!string.IsNullOrEmpty(tag.FirstAlbumArtist))
                    {
                        this.AddTag(metaData, CommonMetaData.Artist, tag.FirstAlbumArtist);
                    }
                    else if (!string.IsNullOrEmpty(tag.FirstPerformer))
                    {
                        this.AddTag(metaData, CommonMetaData.Artist, tag.FirstPerformer);
                    }
                }, this.ErrorHandler);
                this.Try(() => this.AddTag(metaData, CommonMetaData.Composer, tag.FirstComposer), this.ErrorHandler);
                this.Try(() => this.AddTag(metaData, CommonMetaData.Conductor, tag.Conductor), this.ErrorHandler);
                this.Try(() => this.AddTag(metaData, CommonMetaData.Disc, tag.Disc.ToString()), this.ErrorHandler);
                this.Try(() => this.AddTag(metaData, CommonMetaData.DiscCount, tag.DiscCount.ToString()), this.ErrorHandler);
                this.Try(() => this.AddTag(metaData, CommonMetaData.Genre, tag.FirstGenre), this.ErrorHandler);
                this.Try(() => this.AddTag(metaData, CommonMetaData.Performer, tag.FirstPerformer), this.ErrorHandler);
                this.Try(() => this.AddTag(metaData, CommonMetaData.Title, tag.Title), this.ErrorHandler);
                this.Try(() => this.AddTag(metaData, CommonMetaData.Track, tag.Track.ToString()), this.ErrorHandler);
                this.Try(() => this.AddTag(metaData, CommonMetaData.TrackCount, tag.TrackCount.ToString()), this.ErrorHandler);
                this.Try(() => this.AddTag(metaData, CommonMetaData.Year, tag.Year.ToString()), this.ErrorHandler);
            }

            if (Categories.HasFlag(MetaDataCategory.Extended))
            {
                this.Try(() => this.AddTag(metaData, CommonMetaData.MusicIpId, tag.MusicIpId), this.ErrorHandler);
                this.Try(() => this.AddTag(metaData, CommonMetaData.AmazonId, tag.AmazonId), this.ErrorHandler);
                this.Try(() => this.AddTag(metaData, CommonMetaData.BeatsPerMinute, tag.BeatsPerMinute.ToString()), this.ErrorHandler);
                this.Try(() => this.AddTag(metaData, CommonMetaData.Comment, tag.Comment), this.ErrorHandler);
                this.Try(() => this.AddTag(metaData, CommonMetaData.Copyright, tag.Copyright), this.ErrorHandler);
                this.Try(() => this.AddTag(metaData, CommonMetaData.Grouping, tag.Grouping), this.ErrorHandler);
                this.Try(() => this.AddTag(metaData, CommonMetaData.Lyrics, tag.Lyrics), this.ErrorHandler);
            }

            if (Categories.HasFlag(MetaDataCategory.MusicBrainz))
            {
                this.Try(() => this.AddTag(metaData, CommonMetaData.MusicBrainzArtistId, tag.MusicBrainzArtistId), this.ErrorHandler);
                this.Try(() => this.AddTag(metaData, CommonMetaData.MusicBrainzDiscId, tag.MusicBrainzDiscId), this.ErrorHandler);
                this.Try(() => this.AddTag(metaData, CommonMetaData.MusicBrainzReleaseArtistId, tag.MusicBrainzReleaseArtistId), this.ErrorHandler);
                this.Try(() => this.AddTag(metaData, CommonMetaData.MusicBrainzReleaseCountry, tag.MusicBrainzReleaseCountry), this.ErrorHandler);
                this.Try(() => this.AddTag(metaData, CommonMetaData.MusicBrainzReleaseId, tag.MusicBrainzReleaseId), this.ErrorHandler);
                this.Try(() => this.AddTag(metaData, CommonMetaData.MusicBrainzReleaseStatus, tag.MusicBrainzReleaseStatus), this.ErrorHandler);
                this.Try(() => this.AddTag(metaData, CommonMetaData.MusicBrainzReleaseType, tag.MusicBrainzReleaseType), this.ErrorHandler);
                this.Try(() => this.AddTag(metaData, CommonMetaData.MusicBrainzTrackId, tag.MusicBrainzTrackId), this.ErrorHandler);
            }
        }

        private bool HasValue(string value)
        {
            return !string.IsNullOrEmpty(value) && !string.Equals(value, 0.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        private void AddTag(IList<MetaDataItem> metaData, string name, string value)
        {
            if (!this.HasValue(value))
            {
                return;
            }
            metaData.Add(new MetaDataItem(name, MetaDataItemType.Tag) { Value = value.Trim() });
        }

        private void AddProperties(IList<MetaDataItem> metaData, Properties properties)
        {
            if (Categories.HasFlag(MetaDataCategory.Standard))
            {
                this.Try(() => this.AddProperty(metaData, CommonProperties.Duration, properties.Duration.TotalMilliseconds.ToString()), this.ErrorHandler);
                this.Try(() => this.AddProperty(metaData, CommonProperties.AudioBitrate, properties.AudioBitrate.ToString()), this.ErrorHandler);
                this.Try(() => this.AddProperty(metaData, CommonProperties.AudioChannels, properties.AudioChannels.ToString()), this.ErrorHandler);
                this.Try(() => this.AddProperty(metaData, CommonProperties.AudioSampleRate, properties.AudioSampleRate.ToString()), this.ErrorHandler);
                this.Try(() =>
                {
                    if (properties.BitsPerSample != 0)
                    {
                        this.AddProperty(metaData, CommonProperties.BitsPerSample, properties.BitsPerSample.ToString());
                    }
                    else
                    {
                        //This is special case just for MPEG-4.
                        foreach (var codec in properties.Codecs.OfType<global::TagLib.Mpeg4.IsoAudioSampleEntry>())
                        {
                            if (codec.AudioSampleSize != 0)
                            {
                                this.AddProperty(metaData, CommonProperties.BitsPerSample, codec.AudioSampleSize.ToString());
                                break;
                            }
                        }
                    }
                }, this.ErrorHandler);
            }
            if (Categories.HasFlag(MetaDataCategory.MultiMedia))
            {
                this.Try(() => this.AddProperty(metaData, CommonProperties.Description, properties.Description), this.ErrorHandler);
                this.Try(() => this.AddProperty(metaData, CommonProperties.PhotoHeight, properties.PhotoHeight.ToString()), this.ErrorHandler);
                this.Try(() => this.AddProperty(metaData, CommonProperties.PhotoQuality, properties.PhotoQuality.ToString()), this.ErrorHandler);
                this.Try(() => this.AddProperty(metaData, CommonProperties.PhotoWidth, properties.PhotoWidth.ToString()), this.ErrorHandler);
                this.Try(() => this.AddProperty(metaData, CommonProperties.VideoHeight, properties.VideoHeight.ToString()), this.ErrorHandler);
                this.Try(() => this.AddProperty(metaData, CommonProperties.VideoWidth, properties.VideoWidth.ToString()), this.ErrorHandler);
            }
        }

        private void AddProperty(IList<MetaDataItem> metaData, string name, string value)
        {
            if (!this.HasValue(value))
            {
                return;
            }
            metaData.Add(new MetaDataItem(name, MetaDataItemType.Property) { Value = value.Trim() });
        }

        private async Task AddImages(List<MetaDataItem> metaData, string fileName)
        {
            foreach (var type in new[] { ArtworkType.FrontCover, ArtworkType.BackCover })
            {
                if (!ArtworkTypes.HasFlag(type))
                {
                    continue;
                }
                var value = this.ArtworkProvider.Find(fileName, type);
                if (!string.IsNullOrEmpty(value) && global::System.IO.File.Exists(value))
                {
                    if (this.CopyImages.Value)
                    {
                        value = await this.ImportImage(value, value, false).ConfigureAwait(false);
                    }
                    metaData.Add(new MetaDataItem()
                    {
                        Name = Enum.GetName(typeof(ArtworkType), type),
                        Value = value,
                        Type = MetaDataItemType.Image
                    });
                }
            }
        }

        private async Task<bool> AddImages(IList<MetaDataItem> metaData, string name, File file, Tag tag, IPicture[] pictures)
        {
            if (pictures == null)
            {
                return false;
            }
            var types = new List<ArtworkType>();
            try
            {
                foreach (var picture in pictures.OrderBy(picture => GetPicturePriority(picture)))
                {
                    var type = GetArtworkType(picture.Type);
                    if (!ArtworkTypes.HasFlag(type) || types.Contains(type))
                    {
                        continue;
                    }
                    if (picture.Data.Count > MAX_IMAGE_SIZE)
                    {
                        Logger.Write(this, LogLevel.Warn, "Not importing image from file \"{0}\" due to size: {1} > {2}", file.Name, picture.Data.Count, MAX_IMAGE_SIZE);
                        continue;
                    }
                    metaData.Add(new MetaDataItem(Enum.GetName(typeof(ArtworkType), type), MetaDataItemType.Image)
                    {
                        Value = await this.ImportImage(tag, picture, type, false)
.ConfigureAwait(false)
                    });
                    types.Add(type);
                }
            }
            catch (Exception e)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to read pictures: {0} => {1}", file.Name, e.Message);
            }
            return types.Any();
        }

        private async Task<string> ImportImage(Tag tag, IPicture picture, ArtworkType type, bool overwrite)
        {
            return await this.ImportImage(picture, picture.Data.Checksum.ToString(), overwrite).ConfigureAwait(false);
        }

        private async Task<string> ImportImage(IPicture value, string id, bool overwrite)
        {
            var prefix = this.GetType().Name;
            var result = default(string);
            if (overwrite || !FileMetaDataStore.Exists(prefix, id, out result))
            {
#if NET40
                Semaphore.Wait();
#else
                await Semaphore.WaitAsync().ConfigureAwait(false);
#endif
                try
                {
                    if (overwrite || !FileMetaDataStore.Exists(prefix, id, out result))
                    {
                        return await FileMetaDataStore.WriteAsync(prefix, id, value.Data.Data).ConfigureAwait(false);
                    }
                }
                finally
                {
                    Semaphore.Release();
                }
            }
            return result;
        }

        private async Task<string> ImportImage(string fileName, string id, bool overwrite)
        {
            if (FileMetaDataStore.Contains(fileName))
            {
                return fileName;
            }
            var prefix = this.GetType().Name;
            var result = default(string);
            if (overwrite || !FileMetaDataStore.Exists(prefix, id, out result))
            {
#if NET40
                Semaphore.Wait();
#else
                await Semaphore.WaitAsync().ConfigureAwait(false);
#endif
                try
                {
                    if (overwrite || !FileMetaDataStore.Exists(prefix, id, out result))
                    {
                        return await FileMetaDataStore.WriteAsync(prefix, id, fileName).ConfigureAwait(false);
                    }
                }
                finally
                {
                    Semaphore.Release();
                }
            }
            return result;
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
                    await this.ReplaceImage(metaDataItem, tag, pictures, index).ConfigureAwait(false);
                }
                else
                {
                    this.RemoveImage(metaDataItem, tag, pictures, index);
                }
            }
            else if (!string.IsNullOrEmpty(metaDataItem.Value))
            {
                await this.AddImage(metaDataItem, tag, pictures).ConfigureAwait(false);
            }
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

            /* Unmerged change from project 'FoxTunes.MetaData.TagLib(net461)'
            Before:
                        pictures.Add(await this.CreateImage(metaDataItem, tag));
            After:
                        pictures.Add(await this.CreateImage(metaDataItem, tag).ConfigureAwait(false));
            */
            pictures.Add(await this.CreateImage(metaDataItem, tag).ConfigureAwait(false));
        }

        private async Task ReplaceImage(MetaDataItem metaDataItem, Tag tag, IList<IPicture> pictures, int index)
        {
            pictures[index] = await this.CreateImage(metaDataItem, tag).ConfigureAwait(false);
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
                Type = GetPictureType(type),
                MimeType = MimeMapping.Instance.GetMimeType(metaDataItem.Value)
            };
            metaDataItem.Value = await this.ImportImage(tag, picture, type, true).ConfigureAwait(false);
            return picture;
        }

        private void ErrorHandler(Exception e)
        {
            Logger.Write(this, LogLevel.Error, "Failed to read meta data: {0}", e.Message);
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

        private byte GetPicturePriority(IPicture picture)
        {
            switch (picture.Type)
            {
                case PictureType.FrontCover:
                case PictureType.BackCover:
                    return 0;
            }
            return 255;
        }

        public static readonly IDictionary<PictureType, ArtworkType> PictureTypeMapping = new Dictionary<PictureType, ArtworkType>()
        {
            { PictureType.FrontCover, ArtworkType.FrontCover },
            { PictureType.BackCover, ArtworkType.BackCover },
            { PictureType.NotAPicture, ArtworkType.FrontCover } //This seems to be pretty common...
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
    }
}
