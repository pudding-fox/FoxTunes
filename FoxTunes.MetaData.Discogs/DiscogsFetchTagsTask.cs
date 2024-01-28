using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace FoxTunes
{
    public class DiscogsFetchTagsTask : DiscogsLookupTask
    {
        public DiscogsFetchTagsTask(Discogs.ReleaseLookup[] releaseLookups) : base(releaseLookups)
        {
            this.Name = Strings.LookupTagsTask_Name;
        }

        protected override Task<bool> OnLookupSuccess(Discogs.ReleaseLookup releaseLookup)
        {
            return this.ImportTags(releaseLookup);
        }

        protected virtual async Task<bool> ImportTags(Discogs.ReleaseLookup releaseLookup)
        {
            var release = await this.Discogs.GetRelease(releaseLookup.Release).ConfigureAwait(false);
            if (release == null)
            {
                return false;
            }
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var fileData in releaseLookup.FileDatas)
            {
                this.UpdateMetaData(fileData, release, names);
            }
            if (names.Any())
            {
                var libraryItems = releaseLookup.FileDatas.OfType<LibraryItem>().ToArray();
                var playlistItems = releaseLookup.FileDatas.OfType<PlaylistItem>().ToArray();
                if (libraryItems.Any())
                {
                    await this.MetaDataManager.Save(
                        libraryItems,
                        this.WriteTags.Value,
                        false,
                        names.ToArray()
                    ).ConfigureAwait(false);
                }
                if (playlistItems.Any())
                {
                    await this.MetaDataManager.Save(
                        playlistItems,
                        this.WriteTags.Value,
                        false,
                        names.ToArray()
                    ).ConfigureAwait(false);
                }
            }
            return true;
        }

        protected virtual void UpdateMetaData(IFileData fileData, Discogs.ReleaseDetails release, ISet<string> names)
        {
            lock (fileData.MetaDatas)
            {
                var targetMetaData = fileData.MetaDatas.ToDictionary(
                   metaDataItem => metaDataItem.Name,
                   StringComparer.OrdinalIgnoreCase
               );
                var sourceMetaData = this.GetMetaData(
                    fileData,
                    release,
                    this.GetTrackDetails(this.GetTrackNumber(targetMetaData), this.GetTrackTitle(targetMetaData), release),
                    names
                );
                foreach (var pair in sourceMetaData)
                {
                    var metaDataItem = default(MetaDataItem);
                    if (targetMetaData.TryGetValue(pair.Key, out metaDataItem))
                    {
                        metaDataItem.Value = pair.Value.Value;
                    }
                    else
                    {
                        fileData.MetaDatas.Add(pair.Value);
                    }
                }
            }
        }

        protected virtual IDictionary<string, MetaDataItem> GetMetaData(IFileData fileData, Discogs.ReleaseDetails releaseDetails, Discogs.TrackDetails trackDetails, ISet<string> names)
        {
            var artist = default(string);
            var album = default(string);
            var track = default(int);
            var title = default(string);
            var performer = default(string);
            var genre = default(string);
            var year = default(int);
            var isCompilation = default(bool);
            var metaData = new Dictionary<string, MetaDataItem>(StringComparer.OrdinalIgnoreCase);

            if (releaseDetails.Artists.Any())
            {
                //TODO: Multiple artists.
                artist = releaseDetails.Artists.First().Name;
            }
            album = releaseDetails.Title;
            if (trackDetails != null)
            {
                if (!int.TryParse(trackDetails.Position, out track))
                {
                    //TODO: We're assuming that the tracks are in order.
                    track = releaseDetails.Tracks.IndexOf(trackDetails) + 1;
                }
                title = trackDetails.Title;
                if (trackDetails.Artists.Any())
                {
                    //TODO: Multiple artists.
                    var trackArtist = trackDetails.Artists.First().Name;
                    if (string.IsNullOrEmpty(artist))
                    {
                        artist = trackArtist;
                    }
                    else if (!string.Equals(artist, trackArtist, StringComparison.OrdinalIgnoreCase))
                    {
                        performer = trackArtist;
                        //TODO: Is it though?
                        isCompilation = true;
                    }
                }
            }
            genre = releaseDetails.Genres.FirstOrDefault();
            if (!int.TryParse(releaseDetails.Year, out year))
            {
                year = default(int);
            }

            if (!string.IsNullOrEmpty(artist))
            {
                metaData[CommonMetaData.Artist] = new MetaDataItem(CommonMetaData.Artist, MetaDataItemType.Tag) { Value = artist };
                names.Add(CommonMetaData.Artist);
            }
            if (!string.IsNullOrEmpty(album))
            {
                metaData[CommonMetaData.Album] = new MetaDataItem(CommonMetaData.Album, MetaDataItemType.Tag) { Value = album };
                names.Add(CommonMetaData.Album);
            }
            if (track > 0)
            {
                metaData[CommonMetaData.Track] = new MetaDataItem(CommonMetaData.Track, MetaDataItemType.Tag) { Value = track.ToString() };
                names.Add(CommonMetaData.Track);
            }
            if (!string.IsNullOrEmpty(title))
            {
                metaData[CommonMetaData.Title] = new MetaDataItem(CommonMetaData.Title, MetaDataItemType.Tag) { Value = title };
                names.Add(CommonMetaData.Title);
            }
            if (!string.IsNullOrEmpty(performer))
            {
                metaData[CommonMetaData.Performer] = new MetaDataItem(CommonMetaData.Performer, MetaDataItemType.Tag) { Value = performer };
                names.Add(CommonMetaData.Performer);
            }
            if (!string.IsNullOrEmpty(genre))
            {
                metaData[CommonMetaData.Genre] = new MetaDataItem(CommonMetaData.Genre, MetaDataItemType.Tag) { Value = genre };
                names.Add(CommonMetaData.Genre);
            }
            if (year > 0)
            {
                metaData[CommonMetaData.Year] = new MetaDataItem(CommonMetaData.Year, MetaDataItemType.Tag) { Value = year.ToString() };
                names.Add(CommonMetaData.Year);
            }
            if (isCompilation)
            {
                metaData[CommonMetaData.IsCompilation] = new MetaDataItem(CommonMetaData.IsCompilation, MetaDataItemType.Tag) { Value = bool.TrueString };
                names.Add(CommonMetaData.IsCompilation);
            }
            return metaData;
        }

        protected virtual int GetTrackNumber(IDictionary<string, MetaDataItem> metaData)
        {
            var metaDataItem = default(MetaDataItem);
            if (metaData.TryGetValue(CommonMetaData.Track, out metaDataItem))
            {
                var track = default(int);
                if (int.TryParse(metaDataItem.Value, out track))
                {
                    return track;
                }
            }
            return default(int);
        }

        protected virtual string GetTrackTitle(IDictionary<string, MetaDataItem> metaData)
        {
            var metaDataItem = default(MetaDataItem);
            if (metaData.TryGetValue(CommonMetaData.Title, out metaDataItem))
            {
                return metaDataItem.Value;
            }
            return string.Empty;
        }

        protected virtual Discogs.TrackDetails GetTrackDetails(int track, string title, Discogs.ReleaseDetails releaseDetails)
        {
            //If no track number was provided but the release contains only one track then return it.
            if (track == default(int))
            {
                if (releaseDetails.Tracks.Length == 1)
                {
                    return releaseDetails.Tracks.First();
                }
            }
            foreach (var trackDetails in releaseDetails.Tracks)
            {
                //If no track numer was provided then attempt a title match.
                if (track == default(int))
                {
                    if (!string.IsNullOrEmpty(title) && Discogs.Sanitize(title).Similarity(Discogs.Sanitize(trackDetails.Title), true) >= this.MinConfidence.Value)
                    {
                        return trackDetails;
                    }
                }
                //Otherwise attept a position match.
                else
                {
                    var position = default(int);
                    if (int.TryParse(trackDetails.Position, out position))
                    {
                        if (position == track)
                        {
                            return trackDetails;
                        }
                    }
                    else
                    {
                        //TODO: Position is not always numeric.
                    }
                }
            }
            return default(Discogs.TrackDetails);
        }
    }
}
