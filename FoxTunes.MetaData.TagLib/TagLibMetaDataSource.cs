using FoxTunes.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TagLib;

namespace FoxTunes
{
    public class TagLibMetaDataSource : BaseComponent, IMetaDataSource
    {
        public static MetaDataCategory Categories = MetaDataCategory.Standard | MetaDataCategory.First;

        private TagLibMetaDataSource()
        {
            this.MetaDatas = new ObservableCollection<MetaDataItem>();
        }

        public TagLibMetaDataSource(string fileName)
            : this()
        {
            this.FileName = fileName;
        }

        public string FileName { get; private set; }

        public ObservableCollection<MetaDataItem> MetaDatas { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            Logger.Write(this, LogLevel.Trace, "Reading meta data for file: {0}", this.FileName);
            var file = File.Create(this.FileName);
            if (file.PossiblyCorrupt)
            {
                foreach (var reason in file.CorruptionReasons)
                {
                    Logger.Write(this, LogLevel.Debug, "Meta data corruption detected: {0} => {1}", this.FileName, reason);
                }
            }
            this.AddMetaDatas(file.Tag);
            this.AddProperties(file.Properties);
            this.AddImages(file.Tag);
            base.InitializeComponent(core);
        }

        private void AddMetaDatas(Tag tag)
        {
            if (Categories.HasFlag(MetaDataCategory.Standard))
            {
                this.AddMetaData(CommonMetaData.Album, tag.Album);
                this.AddMetaData(CommonMetaData.AlbumArtists, tag.AlbumArtists);
#pragma warning disable 612, 618
                this.AddMetaData(CommonMetaData.Artists, tag.Artists);
#pragma warning restore 612, 618
                this.AddMetaData(CommonMetaData.Composers, tag.Composers);
                this.AddMetaData(CommonMetaData.Conductor, tag.Conductor);
                this.AddMetaData(CommonMetaData.Disc, tag.Disc);
                this.AddMetaData(CommonMetaData.DiscCount, tag.DiscCount);
                this.AddMetaData(CommonMetaData.Genres, tag.Genres);
                this.AddMetaData(CommonMetaData.Performers, tag.Performers);
                this.AddMetaData(CommonMetaData.Title, tag.Title);
                this.AddMetaData(CommonMetaData.Track, tag.Track);
                this.AddMetaData(CommonMetaData.TrackCount, tag.TrackCount);
                this.AddMetaData(CommonMetaData.Year, tag.Year);
            }

            if (Categories.HasFlag(MetaDataCategory.First))
            {
                this.AddMetaData(CommonMetaData.FirstAlbumArtist, tag.FirstAlbumArtist);
#pragma warning disable 612, 618
                this.AddMetaData(CommonMetaData.FirstArtist, tag.FirstArtist);
#pragma warning restore 612, 618
                this.AddMetaData(CommonMetaData.FirstComposer, tag.FirstComposer);
                this.AddMetaData(CommonMetaData.FirstGenre, tag.FirstGenre);
                this.AddMetaData(CommonMetaData.FirstPerformer, tag.FirstPerformer);
            }

            if (Categories.HasFlag(MetaDataCategory.Joined))
            {
                this.AddMetaData(CommonMetaData.JoinedAlbumArtists, tag.JoinedAlbumArtists);
#pragma warning disable 612, 618
                this.AddMetaData(CommonMetaData.JoinedArtists, tag.JoinedArtists);
#pragma warning restore 612, 618
                this.AddMetaData(CommonMetaData.JoinedComposers, tag.JoinedComposers);
                this.AddMetaData(CommonMetaData.JoinedGenres, tag.JoinedGenres);
                this.AddMetaData(CommonMetaData.JoinedPerformers, tag.JoinedPerformers);
            }

            if (Categories.HasFlag(MetaDataCategory.Extended))
            {
                this.AddMetaData(CommonMetaData.MusicIpId, tag.MusicIpId);
                this.AddMetaData(CommonMetaData.AmazonId, tag.AmazonId);
                this.AddMetaData(CommonMetaData.BeatsPerMinute, tag.BeatsPerMinute);
                this.AddMetaData(CommonMetaData.Comment, tag.Comment);
                this.AddMetaData(CommonMetaData.Copyright, tag.Copyright);
                this.AddMetaData(CommonMetaData.Grouping, tag.Grouping);
                this.AddMetaData(CommonMetaData.Lyrics, tag.Lyrics);
            }

            if (Categories.HasFlag(MetaDataCategory.MusicBrainz))
            {
                this.AddMetaData(CommonMetaData.MusicBrainzArtistId, tag.MusicBrainzArtistId);
                this.AddMetaData(CommonMetaData.MusicBrainzDiscId, tag.MusicBrainzDiscId);
                this.AddMetaData(CommonMetaData.MusicBrainzReleaseArtistId, tag.MusicBrainzReleaseArtistId);
                this.AddMetaData(CommonMetaData.MusicBrainzReleaseCountry, tag.MusicBrainzReleaseCountry);
                this.AddMetaData(CommonMetaData.MusicBrainzReleaseId, tag.MusicBrainzReleaseId);
                this.AddMetaData(CommonMetaData.MusicBrainzReleaseStatus, tag.MusicBrainzReleaseStatus);
                this.AddMetaData(CommonMetaData.MusicBrainzReleaseType, tag.MusicBrainzReleaseType);
                this.AddMetaData(CommonMetaData.MusicBrainzTrackId, tag.MusicBrainzTrackId);
            }

            if (Categories.HasFlag(MetaDataCategory.Sort))
            {
                this.AddMetaData(CommonMetaData.TitleSort, tag.TitleSort);
                this.AddMetaData(CommonMetaData.PerformersSort, tag.PerformersSort);
                this.AddMetaData(CommonMetaData.JoinedPerformersSort, tag.JoinedPerformersSort);
                this.AddMetaData(CommonMetaData.ComposersSort, tag.ComposersSort);
                this.AddMetaData(CommonMetaData.AlbumArtistsSort, tag.AlbumArtistsSort);
                this.AddMetaData(CommonMetaData.AlbumSort, tag.AlbumSort);
                if (Categories.HasFlag(MetaDataCategory.First))
                {
                    this.AddMetaData(CommonMetaData.FirstPerformerSort, tag.FirstPerformerSort);
                    this.AddMetaData(CommonMetaData.FirstComposerSort, tag.FirstComposerSort);
                    this.AddMetaData(CommonMetaData.FirstAlbumArtistSort, tag.FirstAlbumArtistSort);
                }
            }
        }

        private void AddMetaData(string name, uint? value)
        {
            this.AddMetaData(name, (int?)value);
        }

        private void AddMetaData(string name, int? value)
        {
            if (!value.HasValue)
            {
                return;
            }
            this.MetaDatas.Add(new MetaDataItem(name, MetaDataItemType.Tag) { NumericValue = value.Value });
        }

        private void AddMetaData(string name, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }
            this.MetaDatas.Add(new MetaDataItem(name, MetaDataItemType.Tag) { TextValue = value.Trim() });
        }

        private void AddMetaData(string name, string[] values)
        {
            foreach (var value in values)
            {
                this.AddMetaData(name, value);
            }
        }

        private void AddProperties(Properties properties)
        {
            if (Categories.HasFlag(MetaDataCategory.Standard))
            {
                this.AddProperty(CommonProperties.Duration, properties.Duration);
                this.AddProperty(CommonProperties.AudioBitrate, properties.AudioBitrate);
                this.AddProperty(CommonProperties.AudioChannels, properties.AudioChannels);
                this.AddProperty(CommonProperties.AudioSampleRate, properties.AudioSampleRate);
                this.AddProperty(CommonProperties.BitsPerSample, properties.BitsPerSample);
            }
            if (Categories.HasFlag(MetaDataCategory.MultiMedia))
            {
                this.AddProperty(CommonProperties.Description, properties.Description);
                this.AddProperty(CommonProperties.PhotoHeight, properties.PhotoHeight);
                this.AddProperty(CommonProperties.PhotoQuality, properties.PhotoQuality);
                this.AddProperty(CommonProperties.PhotoWidth, properties.PhotoWidth);
                this.AddProperty(CommonProperties.VideoHeight, properties.VideoHeight);
                this.AddProperty(CommonProperties.VideoWidth, properties.VideoWidth);
            }
        }

        private void AddProperty(string name, int? value)
        {
            if (!value.HasValue)
            {
                return;
            }
            this.MetaDatas.Add(new MetaDataItem(name, MetaDataItemType.Property) { NumericValue = value });
        }

        private void AddProperty(string name, TimeSpan? value)
        {
            if (!value.HasValue)
            {
                return;
            }
            this.MetaDatas.Add(new MetaDataItem(name, MetaDataItemType.Property) { NumericValue = Convert.ToInt32(value.Value.TotalMilliseconds) });
        }

        private void AddProperty(string name, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }
            this.MetaDatas.Add(new MetaDataItem(name, MetaDataItemType.Property) { TextValue = value.Trim() });
        }

        private void AddImages(Tag tag)
        {
            this.AddImage(CommonMetaData.Pictures, tag.Pictures).Wait();
        }

        private async Task AddImage(string name, IPicture[] values)
        {
            if (values == null)
            {
                return;
            }
            foreach (var value in values)
            {
                var id = global::System.IO.Path.GetDirectoryName(this.FileName);
                var fileName = default(string);
                if (!FileMetaDataStore.Exists(id, out fileName))
                {
                    Logger.Write(this, LogLevel.Trace, "Extracted image from meta data: {0} => {1}", this.FileName, fileName);
                    fileName = await FileMetaDataStore.Write(id, value.Data.Data);
                }
                else
                {
                    Logger.Write(this, LogLevel.Trace, "Re-using image from store: {0} => {1}", this.FileName, fileName);
                }
                this.AddImage(fileName, value);
            }
        }

        private void AddImage(string fileName, IPicture value)
        {
            this.MetaDatas.Add(new MetaDataItem(Enum.GetName(typeof(PictureType), value.Type), MetaDataItemType.Image) { FileValue = fileName });
        }
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
