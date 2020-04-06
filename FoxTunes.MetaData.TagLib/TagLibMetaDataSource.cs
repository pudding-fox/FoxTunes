using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TagLib;

namespace FoxTunes
{
    public class TagLibMetaDataSource : BaseComponent, IMetaDataSource
    {
        const int TIMEOUT = 1000;

        public static ArtworkType ArtworkTypes = ArtworkType.FrontCover;

        public static readonly KeyLock<string> KeyLock = new KeyLock<string>();

        //10MB
        public static int MAX_TAG_SIZE = 10240000;

        //2MB
        public static int MAX_IMAGE_SIZE = 2048000;

        public IConfiguration Configuration { get; private set; }

        public BooleanConfigurationElement EmbeddedImages { get; private set; }

        public BooleanConfigurationElement LooseImages { get; private set; }

        public SelectionConfigurationElement ImagesPreference { get; private set; }

        public BooleanConfigurationElement CopyImages { get; private set; }

        public BooleanConfigurationElement Extended { get; private set; }

        public BooleanConfigurationElement MusicBrainz { get; private set; }

        public BooleanConfigurationElement Lyrics { get; private set; }

        public BooleanConfigurationElement Popularimeter { get; private set; }

        public SelectionConfigurationElement Write { get; private set; }

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
            this.ImagesPreference = this.Configuration.GetElement<SelectionConfigurationElement>(
                MetaDataBehaviourConfiguration.SECTION,
                MetaDataBehaviourConfiguration.IMAGES_PREFERENCE
            );
            this.CopyImages = this.Configuration.GetElement<BooleanConfigurationElement>(
                MetaDataBehaviourConfiguration.SECTION,
                MetaDataBehaviourConfiguration.COPY_IMAGES_ELEMENT
            );
            this.Extended = this.Configuration.GetElement<BooleanConfigurationElement>(
                MetaDataBehaviourConfiguration.SECTION,
                MetaDataBehaviourConfiguration.READ_EXTENDED_TAGS
            );
            this.MusicBrainz = this.Configuration.GetElement<BooleanConfigurationElement>(
                MetaDataBehaviourConfiguration.SECTION,
                MetaDataBehaviourConfiguration.READ_MUSICBRAINZ_TAGS
            );
            this.Lyrics = this.Configuration.GetElement<BooleanConfigurationElement>(
                MetaDataBehaviourConfiguration.SECTION,
                MetaDataBehaviourConfiguration.READ_LYRICS_TAGS
            );
            this.Popularimeter = this.Configuration.GetElement<BooleanConfigurationElement>(
                MetaDataBehaviourConfiguration.SECTION,
                MetaDataBehaviourConfiguration.READ_POPULARIMETER_TAGS
            );
            this.Write = this.Configuration.GetElement<SelectionConfigurationElement>(
                MetaDataBehaviourConfiguration.SECTION,
                MetaDataBehaviourConfiguration.WRITE_ELEMENT
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
            var collect = default(bool);
            var metaData = new List<MetaDataItem>();
            Logger.Write(this, LogLevel.Trace, "Reading meta data for file: {0}", fileName);
            try
            {
                using (var file = this.Create(fileName))
                {
                    if (file.InvariantStartPosition > MAX_TAG_SIZE)
                    {
                        collect = true;
                    }
                    if (file.Tag != null)
                    {
                        this.AddTags(metaData, file.Tag);
                    }
                    if (file.Properties != null)
                    {
                        this.AddProperties(metaData, file.Properties);
                    }
                    if (this.Popularimeter.Value)
                    {
                        this.Try(() => PopularimeterManager.Read(this, metaData, file), this.ErrorHandler);
                    }
                    await ImageManager.Read(this, metaData, file).ConfigureAwait(false);
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
            finally
            {
                if (collect)
                {
                    //If we encountered a large meta data section (>10MB) then we need to try to reclaim the memory.
                    GC.Collect();
                }
            }
            return metaData;
        }

        public async Task SetMetaData(string fileName, IEnumerable<MetaDataItem> metaData, Func<MetaDataItem, bool> predicate)
        {
            if (MetaDataBehaviourConfiguration.GetWriteBehaviour(this.Write.Value) == WriteBehaviour.None)
            {
                Logger.Write(this, LogLevel.Warn, "Writing is disabled: {0}", fileName);
                return;
            }
            if (!this.IsSupported(fileName))
            {
                Logger.Write(this, LogLevel.Warn, "Unsupported file format: {0}", fileName);
                return;
            }
            var metaDataItems = default(IEnumerable<MetaDataItem>);
            lock (metaData)
            {
                metaDataItems = metaData.Where(predicate).ToArray();
                if (!metaDataItems.Any())
                {
                    //Nothing to update.
                    return;
                }
            }
            var collect = default(bool);
            using (var file = this.Create(fileName))
            {
                foreach (var metaDataItem in metaDataItems)
                {
                    switch (metaDataItem.Type)
                    {
                        case MetaDataItemType.Tag:
                            this.SetTag(metaDataItem, file, file.Tag);
                            break;
                        case MetaDataItemType.Image:
                            if (file.InvariantStartPosition > MAX_TAG_SIZE)
                            {
                                Logger.Write(this, LogLevel.Warn, "Not exporting images to file \"{0}\" due to size: {1} > {2}", file.Name, file.InvariantStartPosition, MAX_TAG_SIZE);
                                collect = true;
                            }
                            else
                            {
                                await this.SetImage(metaDataItem, file, file.Tag).ConfigureAwait(false);
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
            this.Try(() => this.AddTag(metaData, CommonMetaData.BeatsPerMinute, tag.BeatsPerMinute.ToString()), this.ErrorHandler);

            if (this.Extended.Value)
            {
                this.Try(() => this.AddTag(metaData, CommonMetaData.MusicIpId, tag.MusicIpId), this.ErrorHandler);
                this.Try(() => this.AddTag(metaData, CommonMetaData.AmazonId, tag.AmazonId), this.ErrorHandler);
                this.Try(() => this.AddTag(metaData, CommonMetaData.Comment, tag.Comment), this.ErrorHandler);
                this.Try(() => this.AddTag(metaData, CommonMetaData.Copyright, tag.Copyright), this.ErrorHandler);
                this.Try(() => this.AddTag(metaData, CommonMetaData.Grouping, tag.Grouping), this.ErrorHandler);
            }

            if (this.MusicBrainz.Value)
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

            if (this.Lyrics.Value)
            {
                this.Try(() => this.AddTag(metaData, CommonMetaData.Lyrics, tag.Lyrics), this.ErrorHandler);
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

        private void AddProperty(IList<MetaDataItem> metaData, string name, string value)
        {
            if (!this.HasValue(value))
            {
                return;
            }
            metaData.Add(new MetaDataItem(name, MetaDataItemType.Property) { Value = value.Trim() });
        }

        private async Task<bool> AddImages(IList<MetaDataItem> metaData, string fileName)
        {
            var types = ArtworkType.None;
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
                    if (ArtworkTypes.HasFlag(types |= type))
                    {
                        //We have everything we need.
                        return true;
                    }
                }
            }
            return types != ArtworkType.None;
        }

        private async Task<bool> AddImages(IList<MetaDataItem> metaData, string name, File file, Tag tag, IPicture[] pictures)
        {
            var types = ArtworkType.None;
            try
            {
                foreach (var picture in pictures.OrderBy(picture => GetPicturePriority(picture)))
                {
                    var type = GetArtworkType(picture.Type);
                    if (!ArtworkTypes.HasFlag(type) || types.HasFlag(type))
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
                        Value = await this.ImportImage(file, tag, picture, type, false).ConfigureAwait(false)
                    });
                    if (ArtworkTypes.HasFlag(types |= type))
                    {
                        //We have everything we need.
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to read pictures: {0} => {1}", file.Name, e.Message);
            }
            return types != ArtworkType.None;
        }

        private async Task<string> ImportImage(File file, Tag tag, IPicture picture, ArtworkType type, bool overwrite)
        {
            var id = this.GetPictureId(file, tag, type);
            return await this.ImportImage(picture, id, overwrite).ConfigureAwait(false);
        }

        private async Task<string> ImportImage(IPicture value, string id, bool overwrite)
        {
            var prefix = this.GetType().Name;
            var result = default(string);
            if (overwrite || !FileMetaDataStore.Exists(prefix, id, out result))
            {
                //TODO: Setting throwOnTimeout = false so we ignore synchronization timeout.
                //TODO: I think there exists a deadlock bug in KeyLock but I haven't been able to prove it.
                using (KeyLock.Lock(id, TIMEOUT, false))
                {
                    if (overwrite || !FileMetaDataStore.Exists(prefix, id, out result))
                    {
                        return await FileMetaDataStore.WriteAsync(prefix, id, value.Data.Data).ConfigureAwait(false);
                    }
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
                //TODO: Setting throwOnTimeout = false so we ignore synchronization timeout.
                //TODO: I think there exists a deadlock bug in KeyLock but I haven't been able to prove it.
                using (KeyLock.Lock(id, TIMEOUT, false))
                {
                    if (overwrite || !FileMetaDataStore.Exists(prefix, id, out result))
                    {
                        return await FileMetaDataStore.WriteAsync(prefix, id, fileName).ConfigureAwait(false);
                    }
                }
            }
            return result;
        }

        private void SetTag(MetaDataItem metaDataItem, File file, Tag tag)
        {
            //TODO: Make this case insensitive.
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
                case CommonMetaData.LastPlayed:
                    if (MetaDataBehaviourConfiguration.GetWriteBehaviour(this.Write.Value).HasFlag(WriteBehaviour.Statistics))
                    {
                        PopularimeterManager.Write(this, metaDataItem, file);
                    }
                    break;
                case CommonMetaData.Performer:
                    tag.Performers = new[] { metaDataItem.Value };
                    break;
                case CommonMetaData.PlayCount:
                    if (MetaDataBehaviourConfiguration.GetWriteBehaviour(this.Write.Value).HasFlag(WriteBehaviour.Statistics))
                    {
                        PopularimeterManager.Write(this, metaDataItem, file);
                    }
                    break;
                case CommonMetaData.Rating:
                    PopularimeterManager.Write(this, metaDataItem, file);
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

        private async Task SetImage(MetaDataItem metaDataItem, File file, Tag tag)
        {
            var index = default(int);
            var pictures = new List<IPicture>(tag.Pictures);
            if (this.HasImage(metaDataItem.Name, tag, pictures, out index))
            {
                if (!string.IsNullOrEmpty(metaDataItem.Value))
                {
                    await this.ReplaceImage(metaDataItem, file, tag, pictures, index).ConfigureAwait(false);
                }
                else
                {
                    this.RemoveImage(metaDataItem, tag, pictures, index);
                }
            }
            else if (!string.IsNullOrEmpty(metaDataItem.Value))
            {
                await this.AddImage(metaDataItem, file, tag, pictures).ConfigureAwait(false);
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

        private async Task AddImage(MetaDataItem metaDataItem, File file, Tag tag, IList<IPicture> pictures)
        {
            pictures.Add(await this.CreateImage(metaDataItem, file, tag).ConfigureAwait(false));
        }

        private async Task ReplaceImage(MetaDataItem metaDataItem, File file, Tag tag, IList<IPicture> pictures, int index)
        {
            pictures[index] = await this.CreateImage(metaDataItem, file, tag).ConfigureAwait(false);
        }

        private void RemoveImage(MetaDataItem metaDataItem, Tag tag, IList<IPicture> pictures, int index)
        {
            pictures.RemoveAt(index);
        }

        private async Task<IPicture> CreateImage(MetaDataItem metaDataItem, File file, Tag tag)
        {
            var type = GetArtworkType(metaDataItem.Name);
            var picture = new Picture(metaDataItem.Value)
            {
                Type = GetPictureType(type),
                MimeType = MimeMapping.Instance.GetMimeType(metaDataItem.Value)
            };
            metaDataItem.Value = await this.ImportImage(file, tag, picture, type, true).ConfigureAwait(false);
            return picture;
        }

        private void ErrorHandler(Exception e)
        {
            Logger.Write(this, LogLevel.Error, "Failed to read meta data: {0}", e.Message);
        }

        private string GetPictureId(File file, Tag tag, ArtworkType type)
        {
            //Year + (Album | Directory) + Type
            var hashCode = default(int);
            unchecked
            {
                if (tag.Year != 0)
                {
                    hashCode = (hashCode * 29) + tag.Year.GetHashCode();
                }
                if (!string.IsNullOrEmpty(tag.Album))
                {
                    hashCode += tag.Album.ToLower().GetHashCode();
                }
                else
                {
                    hashCode += global::System.IO.Path.GetDirectoryName(file.Name).ToLower().GetHashCode();
                }
                hashCode = (hashCode * 29) + type.GetHashCode();
            }
            return Math.Abs(hashCode).ToString();
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

        public static class ImageManager
        {
            public static async Task<bool> Read(TagLibMetaDataSource source, IList<MetaDataItem> metaDatas, File file)
            {
                var embedded = source.EmbeddedImages.Value;
                var loose = source.LooseImages.Value;
                if (embedded && loose)
                {
                    switch (MetaDataBehaviourConfiguration.GetImagesPreference(source.ImagesPreference.Value))
                    {
                        default:
                        case ImagePreference.Embedded:
                            return await ReadEmbedded(source, metaDatas, file).ConfigureAwait(false) || await ReadLoose(source, metaDatas, file).ConfigureAwait(false);
                        case ImagePreference.Loose:
                            return await ReadLoose(source, metaDatas, file).ConfigureAwait(false) || await ReadEmbedded(source, metaDatas, file).ConfigureAwait(false);
                    }
                }
                else if (embedded)
                {
                    return await ReadEmbedded(source, metaDatas, file).ConfigureAwait(false);
                }
                else if (loose)
                {
                    return await ReadLoose(source, metaDatas, file).ConfigureAwait(false);
                }
                return false;
            }

            private static Task<bool> ReadEmbedded(TagLibMetaDataSource source, IList<MetaDataItem> metaDatas, File file)
            {
                if (file.InvariantStartPosition > MAX_TAG_SIZE)
                {
                    Logger.Write(source, LogLevel.Warn, "Not importing images from file \"{0}\" due to size: {1} > {2}", file.Name, file.InvariantStartPosition, MAX_TAG_SIZE);
                }
                else
                {
                    var pictures = file.Tag.Pictures;
                    if (pictures != null)
                    {
                        return source.AddImages(
                            metaDatas,
                            CommonMetaData.Pictures,
                            file,
                            file.Tag,
                            pictures
                        );
                    }
                }
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.FromResult(false);
#endif
            }

            private static Task<bool> ReadLoose(TagLibMetaDataSource source, IList<MetaDataItem> metaDatas, File file)
            {
                return source.AddImages(metaDatas, file.Name);
            }
        }

        public static class PopularimeterManager
        {
            const byte RATING_0 = 0;

            const byte RATING_1 = 1;

            const byte RATING_2 = 64;

            const byte RATING_3 = 128;

            const byte RATING_4 = 196;

            const byte RATING_5 = 255;

            public static void Read(TagLibMetaDataSource source, IList<MetaDataItem> metaData, File file)
            {
                var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                //If it's an Id3v2 tag then try to read the popularimeter frame.
                //It can contain a rating and a play counter.
                if (file.TagTypes.HasFlag(TagTypes.Id3v2))
                {
                    var tag = GetTag<global::TagLib.Id3v2.Tag>(file, TagTypes.Id3v2);
                    if (tag == null)
                    {
                        return;
                    }
                    foreach (var frame in tag.GetFrames<global::TagLib.Id3v2.PopularimeterFrame>())
                    {
                        ReadPopularimeterFrame(frame, result);
                    }
                }
                //If we didn't find a popularimeter frame then attempt to read the rating from a custom tag.
                if (!result.ContainsKey(CommonMetaData.Rating))
                {
                    var rating = ReadCustomTag(CommonMetaData.Rating, file);
                    if (!string.IsNullOrEmpty(rating))
                    {
                        result.Add(CommonMetaData.Rating, rating);
                    }
                }
                //If we didn't find a popularimeter frame then attempt to read the play count from a custom tag.
                if (!result.ContainsKey(CommonMetaData.PlayCount))
                {
                    var playCount = ReadCustomTag(CommonMetaData.PlayCount, file);
                    if (!string.IsNullOrEmpty(playCount))
                    {
                        result.Add(CommonMetaData.PlayCount, playCount);
                    }
                }
                //Popularimeter frame does not support last played, attempt to read the play count from a custom tag.
                //if (!result.ContainsKey(CommonMetaData.LastPlayed))
                {
                    var lastPlayed = ReadCustomTag(CommonMetaData.LastPlayed, file);
                    if (!string.IsNullOrEmpty(lastPlayed))
                    {
                        result.Add(CommonMetaData.LastPlayed, lastPlayed);
                    }
                }
                //Copy our informations back to the meta data collection.
                foreach (var key in result.Keys)
                {
                    var value = result[key];
                    source.AddTag(metaData, key, value);
                }
            }

            private static void ReadPopularimeterFrame(global::TagLib.Id3v2.PopularimeterFrame frame, IDictionary<string, string> result)
            {
                switch (frame.Rating)
                {
                    case RATING_1:
                        result.Add(CommonMetaData.Rating, "1");
                        break;
                    case RATING_2:
                        result.Add(CommonMetaData.Rating, "2");
                        break;
                    case RATING_3:
                        result.Add(CommonMetaData.Rating, "3");
                        break;
                    case RATING_4:
                        result.Add(CommonMetaData.Rating, "4");
                        break;
                    case RATING_5:
                        result.Add(CommonMetaData.Rating, "5");
                        break;
                }
                if (frame.PlayCount > 0)
                {
                    result.Add(CommonMetaData.PlayCount, Convert.ToString(frame.PlayCount));
                }
            }

            public static void Write(TagLibMetaDataSource source, MetaDataItem metaDataItem, File file)
            {
                if (file.TagTypes.HasFlag(TagTypes.Id3v2) && new[] { CommonMetaData.Rating, CommonMetaData.PlayCount }.Contains(metaDataItem.Name, true))
                {
                    var tag = GetTag<global::TagLib.Id3v2.Tag>(file, TagTypes.Id3v2);
                    var frames = tag.GetFrames<global::TagLib.Id3v2.PopularimeterFrame>();
                    if (frames != null && frames.Any())
                    {
                        foreach (var frame in frames)
                        {
                            WritePopularimeterFrame(frame, metaDataItem);
                        }
                    }
                    else
                    {
                        var frame = new global::TagLib.Id3v2.PopularimeterFrame(string.Empty);
                        WritePopularimeterFrame(frame, metaDataItem);
                        tag.AddFrame(frame);
                    }
                }
                else
                {
                    WriteCustomTag(metaDataItem.Name, metaDataItem.Value, file);
                }
            }

            private static void WritePopularimeterFrame(global::TagLib.Id3v2.PopularimeterFrame frame, MetaDataItem metaDataItem)
            {
                if (string.Equals(metaDataItem.Name, CommonMetaData.Rating, StringComparison.OrdinalIgnoreCase))
                {
                    switch (Convert.ToByte(metaDataItem.Value))
                    {
                        case 0:
                            frame.Rating = RATING_0;
                            break;
                        case 1:
                            frame.Rating = RATING_1;
                            break;
                        case 2:
                            frame.Rating = RATING_2;
                            break;
                        case 3:
                            frame.Rating = RATING_3;
                            break;
                        case 4:
                            frame.Rating = RATING_4;
                            break;
                        case 5:
                            frame.Rating = RATING_5;
                            break;
                    }
                }
                else if (string.Equals(metaDataItem.Name, CommonMetaData.PlayCount, StringComparison.OrdinalIgnoreCase))
                {
                    frame.PlayCount = Convert.ToUInt64(metaDataItem.Value);
                }
            }

            private static T GetTag<T>(File file, TagTypes tagTypes) where T : Tag
            {
                return file.GetTag(tagTypes) as T;
            }

            private static string ReadCustomTag(string name, File file)
            {
                var key = GetCustomTagName(name);
                if (file.TagTypes.HasFlag(TagTypes.Apple))
                {
                    var tag = GetTag<global::TagLib.Mpeg4.AppleTag>(file, TagTypes.Apple);
                    if (tag != null)
                    {
                        return tag.GetDashBox("com.apple.iTunes", key);
                    }
                }
                else if (file.TagTypes.HasFlag(TagTypes.Xiph))
                {
                    var tag = GetTag<global::TagLib.Ogg.XiphComment>(file, TagTypes.Xiph);
                    if (tag != null)
                    {
                        return tag.GetFirstField(key);
                    }
                }
                else if (file.TagTypes.HasFlag(TagTypes.Ape))
                {
                    var tag = GetTag<global::TagLib.Ape.Tag>(file, TagTypes.Ape);
                    if (tag != null)
                    {
                        var item = tag.GetItem(key);
                        if (item != null)
                        {
                            return item.ToStringArray().FirstOrDefault();
                        }
                    }
                }
                //Not implemented.
                return null;
            }

            private static void WriteCustomTag(string name, string value, File file)
            {
                var key = GetCustomTagName(name);
                if (file.TagTypes.HasFlag(TagTypes.Apple))
                {
                    const string PREFIX = "\0\0\0\0";
                    const string MEAN = "com.apple.iTunes";
                    var apple = GetTag<global::TagLib.Mpeg4.AppleTag>(file, TagTypes.Apple);
                    if (apple != null)
                    {
                        while (apple.GetDashBox(MEAN, key) != null)
                        {
                            apple.SetDashBox(MEAN, key, string.Empty);
                        }
                        apple.SetDashBox(PREFIX + MEAN, PREFIX + key, value);
                    }
                }
                else if (file.TagTypes.HasFlag(TagTypes.Xiph))
                {
                    var xiph = GetTag<global::TagLib.Ogg.XiphComment>(file, TagTypes.Xiph);
                    if (xiph != null)
                    {
                        xiph.SetField(key, new[] { value });
                    }
                }
                else if (file.TagTypes.HasFlag(TagTypes.Ape))
                {
                    var ape = GetTag<global::TagLib.Ape.Tag>(file, TagTypes.Ape);
                    if (ape != null)
                    {
                        ape.SetValue(key, value);
                    }
                }
                //Not implemented.
            }
        }

        private static string GetCustomTagName(string name)
        {
            if (string.Equals(name, CommonMetaData.LastPlayed, StringComparison.OrdinalIgnoreCase))
            {
                //TODO: I can't work out what the standard is for this value.
                return "last_played_timestamp";
            }
            else if (string.Equals(name, CommonMetaData.PlayCount, StringComparison.OrdinalIgnoreCase))
            {
                return "play_count";
            }
            return name.ToLower();
        }
    }
}
