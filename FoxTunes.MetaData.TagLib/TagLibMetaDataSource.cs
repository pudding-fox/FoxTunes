using FoxTunes.Interfaces;
using System;
using System.Collections.ObjectModel;
using TagLib;

namespace FoxTunes
{
    public class TagLibMetaDataSource : BaseComponent, IMetaDataSource
    {
        private TagLibMetaDataSource()
        {
            this.MetaDatas = new ObservableCollection<MetaDataItem>();
            this.Properties = new ObservableCollection<PropertyItem>();
        }

        public TagLibMetaDataSource(string fileName)
            : this()
        {
            this.FileName = fileName;
        }

        public string FileName { get; private set; }

        public ObservableCollection<MetaDataItem> MetaDatas { get; private set; }

        public ObservableCollection<PropertyItem> Properties { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            var file = File.Create(this.FileName);
            this.AddMetaDatas(file.Tag);
            this.AddProperties(file.Properties);
            base.InitializeComponent(core);
        }

        private void AddMetaDatas(Tag tag)
        {
            this.AddMetaData("Album", tag.Album);
            this.AddMetaData("AlbumArtists", tag.AlbumArtists);
            this.AddMetaData("AlbumArtistsSort", tag.AlbumArtistsSort);
            this.AddMetaData("AlbumSort", tag.AlbumSort);
            this.AddMetaData("AmazonId", tag.AmazonId);
#pragma warning disable 612, 618
            this.AddMetaData("Artists", tag.Artists);
#pragma warning restore 612, 618
            this.AddMetaData("BeatsPerMinute", tag.BeatsPerMinute);
            this.AddMetaData("Comment", tag.Comment);
            this.AddMetaData("Composers", tag.Composers);
            this.AddMetaData("ComposersSort", tag.ComposersSort);
            this.AddMetaData("Conductor", tag.Conductor);
            this.AddMetaData("Copyright", tag.Copyright);
            this.AddMetaData("Disc", tag.Disc);
            this.AddMetaData("DiscCount", tag.DiscCount);
            this.AddMetaData("FirstAlbumArtist", tag.FirstAlbumArtist);
            this.AddMetaData("FirstAlbumArtistSort", tag.FirstAlbumArtistSort);
#pragma warning disable 612, 618
            this.AddMetaData("FirstArtist", tag.FirstArtist);
#pragma warning restore 612, 618
            this.AddMetaData("FirstComposer", tag.FirstComposer);
            this.AddMetaData("FirstComposerSort", tag.FirstComposerSort);
            this.AddMetaData("FirstGenre", tag.FirstGenre);
            this.AddMetaData("FirstPerformer", tag.FirstPerformer);
            this.AddMetaData("FirstPerformerSort", tag.FirstPerformerSort);
            this.AddMetaData("Genres", tag.Genres);
            this.AddMetaData("Grouping", tag.Grouping);
            this.AddMetaData("JoinedAlbumArtists", tag.JoinedAlbumArtists);
#pragma warning disable 612, 618
            this.AddMetaData("JoinedArtists", tag.JoinedArtists);
#pragma warning restore 612, 618
            this.AddMetaData("JoinedComposers", tag.JoinedComposers);
            this.AddMetaData("JoinedGenres", tag.JoinedGenres);
            this.AddMetaData("JoinedPerformers", tag.JoinedPerformers);
            this.AddMetaData("JoinedPerformersSort", tag.JoinedPerformersSort);
            this.AddMetaData("Lyrics", tag.Lyrics);
            this.AddMetaData("MusicBrainzArtistId", tag.MusicBrainzArtistId);
            this.AddMetaData("MusicBrainzDiscId", tag.MusicBrainzDiscId);
            this.AddMetaData("MusicBrainzReleaseArtistId", tag.MusicBrainzReleaseArtistId);
            this.AddMetaData("MusicBrainzReleaseCountry", tag.MusicBrainzReleaseCountry);
            this.AddMetaData("MusicBrainzReleaseId", tag.MusicBrainzReleaseId);
            this.AddMetaData("MusicBrainzReleaseStatus", tag.MusicBrainzReleaseStatus);
            this.AddMetaData("MusicBrainzReleaseType", tag.MusicBrainzReleaseType);
            this.AddMetaData("MusicBrainzTrackId", tag.MusicBrainzTrackId);
            this.AddMetaData("MusicIpId", tag.MusicIpId);
            this.AddMetaData("Performers", tag.Performers);
            this.AddMetaData("PerformersSort", tag.PerformersSort);
            this.AddMetaData("Pictures", tag.Pictures);
            this.AddMetaData("Title", tag.Title);
            this.AddMetaData("TitleSort", tag.TitleSort);
            this.AddMetaData("Track", tag.Track);
            this.AddMetaData("TrackCount", tag.TrackCount);
            this.AddMetaData("Year", tag.Year);
        }

        private void AddMetaData(string name, IPicture[] value)
        {
            //Nothing to do.
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
            this.MetaDatas.Add(new MetaDataItem(name) { NumericValue = value.Value });
        }

        private void AddMetaData(string name, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }
            this.MetaDatas.Add(new MetaDataItem(name) { TextValue = value });
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
            this.AddProperty("AudioBitrate", properties.AudioBitrate);
            this.AddProperty("AudioChannels", properties.AudioChannels);
            this.AddProperty("AudioSampleRate", properties.AudioSampleRate);
            this.AddProperty("BitsPerSample", properties.BitsPerSample);
            this.AddProperty("Description", properties.Description);
            this.AddProperty("Duration", properties.Duration);
            this.AddProperty("PhotoHeight", properties.PhotoHeight);
            this.AddProperty("PhotoQuality", properties.PhotoQuality);
            this.AddProperty("PhotoWidth", properties.PhotoWidth);
            this.AddProperty("VideoHeight", properties.VideoHeight);
            this.AddProperty("VideoWidth", properties.VideoWidth);
        }

        private void AddProperty(string name, int? value)
        {
            if (!value.HasValue)
            {
                return;
            }
            this.Properties.Add(new PropertyItem(name) { NumericValue = value });
        }

        private void AddProperty(string name, TimeSpan? value)
        {
            if (!value.HasValue)
            {
                return;
            }
            this.Properties.Add(new PropertyItem(name) { NumericValue = Convert.ToInt32(value.Value.TotalMilliseconds) });
        }

        private void AddProperty(string name, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }
            this.Properties.Add(new PropertyItem(name) { TextValue = value });
        }
    }
}
