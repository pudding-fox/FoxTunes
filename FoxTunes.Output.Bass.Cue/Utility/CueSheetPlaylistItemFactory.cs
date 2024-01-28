using FoxDb;
using System;
using System.Collections.Generic;
using System.IO;

namespace FoxTunes
{
    public class CueSheetPlaylistItemFactory : BaseComponent
    {
        private static IDictionary<string, string> FILE_NAMES = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "PERFORMER", CommonMetaData.Artist },
            { "DATE", CommonMetaData.Year },
            { "TITLE", CommonMetaData.Album }
        };

        private static IDictionary<string, string> TRACK_NAMES = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "PERFORMER", CommonMetaData.Artist },
            { "DATE", CommonMetaData.Year }
        };

        public PlaylistItem[] Create(CueSheet cueSheet)
        {
            var playlistItems = new List<PlaylistItem>();
            var tracks = cueSheet.Tracks;
            var trackPairs = CueSheet.GetTrackPairs(tracks);
            var directoryName = Path.GetDirectoryName(cueSheet.FileName);
            for (var position = 0; position < trackPairs.GetLength(0); position++)
            {
                var currentTrack = trackPairs[position, 0];
                var nextTrack = trackPairs[position, 1];
                var trackPosition = currentTrack.Index.Time;
                var file = cueSheet.GetFile(currentTrack);
                var fileName = default(string);
                if (nextTrack != null)
                {
                    var trackLength = CueSheet.GetTrackLength(currentTrack, nextTrack);
                    fileName = BassCueStreamProvider.CreateUrl(Path.Combine(directoryName, file.Path), trackPosition, trackLength);
                }
                else
                {
                    fileName = BassCueStreamProvider.CreateUrl(Path.Combine(directoryName, file.Path), trackPosition);
                }
                var playlistItem = new PlaylistItem()
                {
                    DirectoryName = directoryName,
                    FileName = fileName
                };
                playlistItem.MetaDatas = this.GetMetaData(cueSheet, tracks, currentTrack);
                playlistItems.Add(playlistItem);
            }
            return playlistItems.ToArray();
        }

        protected virtual MetaDataItem[] GetMetaData(CueSheet cueSheet, CueSheetTrack[] cueSheetTracks, CueSheetTrack cueSheetTrack)
        {
            var metaDataItems = new List<MetaDataItem>();
            foreach (var tag in cueSheet.Tags)
            {
                var name = default(string);
                if (!FILE_NAMES.TryGetValue(tag.Name, out name))
                {
                    if (!CommonMetaData.Lookup.TryGetValue(tag.Name, out name))
                    {
                        name = tag.Name;
                    }
                }
                metaDataItems.Add(new MetaDataItem(name, MetaDataItemType.Tag)
                {
                    Value = tag.Value
                });
            }
            foreach (var tag in cueSheetTrack.Tags)
            {
                var name = default(string);
                if (!TRACK_NAMES.TryGetValue(tag.Name, out name))
                {
                    if (!CommonMetaData.Lookup.TryGetValue(tag.Name, out name))
                    {
                        name = tag.Name;
                    }
                }
                metaDataItems.Add(new MetaDataItem(name, MetaDataItemType.Tag)
                {
                    Value = tag.Value
                });
            }
            metaDataItems.Add(new MetaDataItem(CommonMetaData.Track, MetaDataItemType.Tag)
            {
                Value = cueSheetTrack.Number
            });
            metaDataItems.Add(new MetaDataItem(CommonMetaData.TrackCount, MetaDataItemType.Tag)
            {
                Value = Convert.ToString(cueSheetTracks.Length)
            });
            //TODO: We could create the CommonProperties.Duration for all but the last track in each file. 
            //TODO: Without understanding the file format we can't determine the length of the last track.
            return metaDataItems.ToArray();
        }
    }
}
