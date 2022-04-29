using FoxTunes.Interfaces;
using MD.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class MinidiscReport : ReportComponent
    {
        public MinidiscReport(MinidiscBehaviour behaviour, IDevice device, IDisc currentDisc, IDisc updatedDisc)
            : this(behaviour, device, currentDisc, GetActions(currentDisc, updatedDisc))
        {

        }

        public MinidiscReport(MinidiscBehaviour behaviour, IDevice device, IDisc disc, IDictionary<ITrack, TrackAction> tracks)
        {
            this.Behaviour = behaviour;
            this.Device = device;
            this.Disc = disc;
            this.Tracks = tracks;
        }

        public MinidiscBehaviour Behaviour { get; private set; }

        public IDevice Device { get; private set; }

        public IDisc Disc { get; private set; }

        public IDictionary<ITrack, TrackAction> Tracks { get; private set; }

        public override string Title
        {
            get
            {
                var disc = GetUpdatedDisc(this.Disc, this.Tracks);
                return string.Format("{0} ({1}%)", disc.Title, disc.GetCapacity().PercentUsed);
            }
        }

        public override string Description
        {
            get
            {
                return string.Empty;
            }
        }

        public override string[] Headers
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

        public override IEnumerable<IReportComponentRow> Rows
        {
            get
            {
                foreach (var pair in this.Tracks.OrderBy(_pair => _pair.Key.Position))
                {
                    yield return new MinidiscReportRow(this, pair.Key, pair.Value);
                }
            }
        }

        public bool HasChanges
        {
            get
            {
                return this.Tracks.Any(pair => pair.Value != TrackAction.None);
            }
        }

        public override IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                if (this.HasChanges)
                {
                    yield return new InvocationComponent(InvocationComponent.CATEGORY_REPORT, ACCEPT, Strings.MinidiscReport_Write, attributes: InvocationComponent.ATTRIBUTE_SYSTEM);
                }
            }
        }

        public override Task InvokeAsync(IInvocationComponent component)
        {
            switch (component.Id)
            {
                case ACCEPT:
                    return this.Write();
            }
            return base.InvokeAsync(component);
        }

        protected virtual async Task Write()
        {
            var currentDisc = this.Disc;
            var updatedDisc = GetUpdatedDisc(currentDisc, this.Tracks);
            var success = await this.Behaviour.WriteDisc(this.Device, currentDisc, updatedDisc).ConfigureAwait(false);
            if (!success)
            {
                //If disc was not written then show this report again.
                await this.Behaviour.ReportEmitter.Send(this).ConfigureAwait(false);
            }
        }

        public void Update(ITrack track, TrackAction action)
        {
            if (this.Tracks[track] == TrackAction.Add && action == TrackAction.Remove)
            {
                //Cancel adding new track.
                this.Tracks.Remove(track);
            }
            else
            {
                this.Tracks[track] = action;
            }
            this.OnTitleChanged();
            this.OnDescriptionChanged();
            this.OnRowsChanged();
        }

        public class MinidiscReportRow : ReportComponentRow
        {
            const string REMOVE = "AAAA";

            const string KEEP = "BBBB";

            public MinidiscReportRow(MinidiscReport report, ITrack track, TrackAction action)
            {
                this.Report = report;
                this.Track = track;
                this.Action = action;
            }

            public MinidiscReport Report { get; private set; }

            public ITrack Track { get; private set; }

            public TrackAction Action { get; private set; }

            public override string[] Values
            {
                get
                {
                    return new[]
                    {
                        this.Track.Name,
                        this.Track.Time.ToString(@"mm\:ss"),
                        GetFormatName(this.Track.Compression),
                        GetActionName(this.Action)
                    };
                }
            }

            public override IEnumerable<IInvocationComponent> Invocations
            {
                get
                {
                    switch (this.Action)
                    {
                        case TrackAction.Add:
                        case TrackAction.None:
                            yield return new InvocationComponent(InvocationComponent.CATEGORY_REPORT, REMOVE, Strings.MinidiscReportRow_Remove);
                            break;
                        case TrackAction.Remove:
                            yield return new InvocationComponent(InvocationComponent.CATEGORY_REPORT, KEEP, Strings.MinidiscReportRow_Keep);
                            break;
                    }
                }
            }

            public override Task InvokeAsync(IInvocationComponent component)
            {
                switch (component.Id)
                {
                    case REMOVE:
                        this.Report.Update(this.Track, TrackAction.Remove);
                        break;
                    case KEEP:
                        this.Report.Update(this.Track, TrackAction.None);
                        break;
                }
                return base.InvokeAsync(component);
            }
        }

        public static IDictionary<ITrack, TrackAction> GetActions(IDisc currentDisc, IDisc updatedDisc)
        {
            var actions = new Dictionary<ITrack, TrackAction>();
            foreach (var track in currentDisc.Tracks)
            {
                if (updatedDisc.Tracks.Contains(track))
                {
                    actions.Add(track, TrackAction.None);
                }
                else
                {
                    actions.Add(track, TrackAction.Remove);
                }
            }
            foreach (var track in updatedDisc.Tracks)
            {
                if (currentDisc.Tracks.Contains(track))
                {
                    //Nothing to do.
                }
                else
                {
                    actions.Add(track, TrackAction.Add);
                }
            }
            return actions;
        }

        public static IDisc GetUpdatedDisc(IDisc currentDisc, IDictionary<ITrack, TrackAction> tracks)
        {
            var updatedDisc = currentDisc.Clone();
            foreach (var pair in tracks.OrderBy(_pair => _pair.Key.Position))
            {
                switch (pair.Value)
                {
                    case TrackAction.Add:
                        updatedDisc.Tracks.Add(pair.Key);
                        break;
                    case TrackAction.Remove:
                        updatedDisc.Tracks.Remove(pair.Key);
                        break;
                }
            }
            return updatedDisc;
        }

        public static string GetFormatName(Compression compression)
        {
            var name = Enum.GetName(typeof(Compression), compression);
            var localized = Strings.ResourceManager.GetString(string.Format("{0}.{1}", typeof(Compression).Name, name));
            if (!string.IsNullOrEmpty(localized))
            {
                return localized;
            }
            return name;
        }

        public static string GetActionName(TrackAction action)
        {
            var name = Enum.GetName(typeof(TrackAction), action);
            var localized = Strings.ResourceManager.GetString(string.Format("{0}.{1}", typeof(TrackAction).Name, name));
            if (!string.IsNullOrEmpty(localized))
            {
                return localized;
            }
            return name;
        }

        public enum TrackAction : byte
        {
            None,
            Add,
            Remove
        }
    }
}
