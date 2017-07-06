using FoxTunes.Interfaces;
using TagLib;

namespace FoxTunes
{
    public class TagLibMetaDataSource : BaseComponent, IMetaDataSource
    {
        private TagLibMetaDataSource()
        {
            this.Items = new MetaDataItems();
        }

        public TagLibMetaDataSource(string fileName)
            : this()
        {
            this.FileName = fileName;
        }

        public string FileName { get; private set; }

        public IMetaDataItems Items { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            var file = File.Create(this.FileName);
            this.Add("Album", file.Tag.Album);
            this.Add("AlbumArtists", file.Tag.AlbumArtists);
            this.Add("AlbumArtistsSort", file.Tag.AlbumArtistsSort);
            this.Add("AlbumSort", file.Tag.AlbumSort);
            this.Add("AmazonId", file.Tag.AmazonId);
#pragma warning disable 612, 618
            this.Add("Artists", file.Tag.Artists);
#pragma warning restore 612, 618
            this.Add("BeatsPerMinute", file.Tag.BeatsPerMinute);
            this.Add("Comment", file.Tag.Comment);
            this.Add("Composers", file.Tag.Composers);
            this.Add("ComposersSort", file.Tag.ComposersSort);
            this.Add("Conductor", file.Tag.Conductor);
            this.Add("Copyright", file.Tag.Copyright);
            this.Add("Disc", file.Tag.Disc);
            this.Add("DiscCount", file.Tag.DiscCount);
            this.Add("FirstAlbumArtist", file.Tag.FirstAlbumArtist);
            this.Add("FirstAlbumArtistSort", file.Tag.FirstAlbumArtistSort);
#pragma warning disable 612, 618
            this.Add("FirstArtist", file.Tag.FirstArtist);
#pragma warning restore 612, 618
            this.Add("FirstComposer", file.Tag.FirstComposer);
            this.Add("FirstComposerSort", file.Tag.FirstComposerSort);
            this.Add("FirstGenre", file.Tag.FirstGenre);
            this.Add("FirstPerformer", file.Tag.FirstPerformer);
            this.Add("FirstPerformerSort", file.Tag.FirstPerformerSort);
            this.Add("Genres", file.Tag.Genres);
            this.Add("Grouping", file.Tag.Grouping);
            this.Add("JoinedAlbumArtists", file.Tag.JoinedAlbumArtists);
#pragma warning disable 612, 618
            this.Add("JoinedArtists", file.Tag.JoinedArtists);
#pragma warning restore 612, 618
            this.Add("JoinedComposers", file.Tag.JoinedComposers);
            this.Add("JoinedGenres", file.Tag.JoinedGenres);
            this.Add("JoinedPerformers", file.Tag.JoinedPerformers);
            this.Add("JoinedPerformersSort", file.Tag.JoinedPerformersSort);
            this.Add("Lyrics", file.Tag.Lyrics);
            this.Add("MusicBrainzArtistId", file.Tag.MusicBrainzArtistId);
            this.Add("MusicBrainzDiscId", file.Tag.MusicBrainzDiscId);
            this.Add("MusicBrainzReleaseArtistId", file.Tag.MusicBrainzReleaseArtistId);
            this.Add("MusicBrainzReleaseCountry", file.Tag.MusicBrainzReleaseCountry);
            this.Add("MusicBrainzReleaseId", file.Tag.MusicBrainzReleaseId);
            this.Add("MusicBrainzReleaseStatus", file.Tag.MusicBrainzReleaseStatus);
            this.Add("MusicBrainzReleaseType", file.Tag.MusicBrainzReleaseType);
            this.Add("MusicBrainzTrackId", file.Tag.MusicBrainzTrackId);
            this.Add("MusicIpId", file.Tag.MusicIpId);
            this.Add("Performers", file.Tag.Performers);
            this.Add("PerformersSort", file.Tag.PerformersSort);
            this.Add("Pictures", file.Tag.Pictures);
            this.Add("Title", file.Tag.Title);
            this.Add("TitleSort", file.Tag.TitleSort);
            this.Add("Track", file.Tag.Track);
            this.Add("TrackCount", file.Tag.TrackCount);
            this.Add("Year", file.Tag.Year);
            base.InitializeComponent(core); ;
        }

        private void Add(string name, IPicture[] value)
        {
            this.Items.Add(new MetaDataItem(name, value));
        }

        private void Add(string name, uint value)
        {
            this.Items.Add(new MetaDataItem(name, value));
        }

        private void Add(string name, string[] values)
        {
            this.Items.Add(new MetaDataItem(name, values));
        }

        private void Add(string name, string value)
        {
            this.Items.Add(new MetaDataItem(name, value));
        }
    }
}
