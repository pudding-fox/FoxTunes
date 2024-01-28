using FoxTunes.Interfaces;
using MD.Net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes
{
    public class MinidiscReport : BaseComponent, IReport
    {
        public MinidiscReport(IDevice device, IDisc disc)
        {
            this.Device = device;
            this.Disc = disc;
            this.Tracks = disc.Tracks.ToDictionary(track => Guid.NewGuid());
        }

        public IDevice Device { get; private set; }

        public IDisc Disc { get; private set; }

        public Dictionary<Guid, ITrack> Tracks { get; private set; }

        public string Title
        {
            get
            {
                return this.Disc.Title;
            }
        }

        public string Description
        {
            get
            {
                return string.Empty;
            }
        }

        public string[] Headers
        {
            get
            {
                return new[]
                {
                    Strings.MinidiscReport_Name,
                    Strings.MinidiscReport_Time,
                    Strings.MinidiscReport_Format,
                    Strings.MinidiscReport_Action
                };
            }
        }

        public IEnumerable<IReportRow> Rows
        {
            get
            {
                return this.Tracks.Select(element => new ReportRow(element.Key, element.Value));
            }
        }

        public Action<Guid> Action
        {
            get
            {
                return key =>
                {
                    var track = default(ITrack);
                    if (!this.Tracks.TryGetValue(key, out track))
                    {
                        return;
                    }
                    this.Play(track);
                };
            }
        }

        protected virtual void Play(ITrack track)
        {
            var toolManager = new ToolManager();
            var playbackManager = new global::MD.Net.PlaybackManager(toolManager);
            playbackManager.Play(this.Device, track);
        }

        public class ReportRow : IReportRow
        {
            public ReportRow(Guid id, ITrack track)
            {
                this.Id = id;
                this.Track = track;
            }

            public Guid Id { get; private set; }

            public ITrack Track { get; private set; }

            public string[] Values
            {
                get
                {
                    return new[]
                    {
                        this.Track.Name,
                        this.Track.Time.ToString(),
                        Enum.GetName(typeof(Compression), this.Track.Compression),
                        "None"
                    };
                }
            }
        }
    }
}
