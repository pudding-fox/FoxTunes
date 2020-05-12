using FoxDb;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes
{
    public class CueSheet
    {
        public CueSheet(string fileName, IEnumerable<CueSheetFile> files, IEnumerable<CueSheetTag> tags)
        {
            this.FileName = fileName;
            this.Files = files.ToArray();
            this.Tags = tags.ToArray();
        }

        public string FileName { get; private set; }

        public CueSheetFile[] Files { get; private set; }

        public CueSheetTag[] Tags { get; private set; }

        public static string GetTrackLength(CueSheetTrack currentTrack, CueSheetTrack nextTrack)
        {
            var currentTrackPosition = CueSheetIndex.ToTimeSpan(currentTrack.Index.Time);
            var nextTrackPosition = CueSheetIndex.ToTimeSpan(nextTrack.Index.Time);
            var trackLength = nextTrackPosition - currentTrackPosition;
            return CueSheetIndex.FromTimeSpan(trackLength);
        }
    }

    public class CueSheetFile
    {
        public CueSheetFile(string path, string format, IEnumerable<CueSheetTrack> tracks)
        {
            this.Path = path;
            this.Format = format;
            this.Tracks = tracks.ToArray();
        }

        public string Path { get; private set; }

        public string Format { get; private set; }

        public CueSheetTrack[] Tracks { get; private set; }
    }

    public class CueSheetTrack
    {
        public CueSheetTrack(string number, string type, IEnumerable<CueSheetIndex> indexes, IEnumerable<CueSheetTag> tags)
        {
            this.Number = number;
            this.Type = type;
            this.Indexes = indexes.ToArray();
            this.Tags = tags.ToArray();
        }

        public string Number { get; private set; }

        public string Type { get; private set; }

        public CueSheetIndex[] Indexes { get; private set; }

        public CueSheetTag[] Tags { get; private set; }

        public CueSheetIndex Index
        {
            get
            {
                switch (this.Indexes.Length)
                {
                    case 0:
                        return null;
                    case 1:
                        return this.Indexes[0];
                    default:
                        return this.Indexes[1];
                }
            }
        }
    }

    public class CueSheetIndex
    {
        public CueSheetIndex(string position, string time)
        {
            this.Position = position;
            this.Time = time;
        }

        public string Position { get; private set; }

        public string Time { get; private set; }

        public static string FromTimeSpan(TimeSpan time)
        {
            var minutes = (time.Hours * 60) + time.Minutes;
            var seconds = time.Seconds;
            var frames = Convert.ToInt32(time.Milliseconds * 0.075);
            return string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, frames);
        }

        public static TimeSpan ToTimeSpan(string time)
        {
            if (string.IsNullOrEmpty(time))
            {
                return TimeSpan.Zero;
            }
            var minutes = default(int);
            var seconds = default(int);
            var milliseconds = default(int);
            var parts = time.Split(':');
            if (parts.Length > 0)
            {
                int.TryParse(parts[0], out minutes);
            }
            if (parts.Length > 1)
            {
                int.TryParse(parts[1], out seconds);
            }
            if (parts.Length > 2)
            {
                var frames = default(int);
                int.TryParse(parts[2], out frames);
                milliseconds = Convert.ToInt32(frames / 0.075);
            }
            return new TimeSpan(0, 0, minutes, seconds, milliseconds);
        }
    }

    public class CueSheetTag
    {
        public CueSheetTag(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }

        public string Name { get; private set; }

        public string Value { get; private set; }
    }
}
