using FoxTunes.Interfaces;
using MD.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class MinidiscReport : BaseComponent, IReport, IActionable
    {
        public MinidiscReport(MinidiscBehaviour behaviour, IDevice device, IActions actions)
        {
            this.Behaviour = behaviour;
            this.Device = device;
            this.Actions = actions;
            this.Tracks = actions.UpdatedDisc.Tracks.ToDictionary(track => Guid.NewGuid());
        }

        public MinidiscBehaviour Behaviour { get; private set; }

        public IDevice Device { get; private set; }

        public IActions Actions { get; private set; }

        public Dictionary<Guid, ITrack> Tracks { get; private set; }

        public string Title
        {
            get
            {
                return string.Format("{0} ({1}%)", this.Actions.UpdatedDisc.Title, this.Actions.UpdatedDisc.GetCapacity().PercentUsed);
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
                    //Nothing to do.
                };
            }
        }

        #region IActionable

        string IActionable.Description
        {
            get
            {
                if (this.Actions.Count > 0)
                {
                    return Strings.MinidiscReport_Write;
                }
                else
                {
                    return Strings.MinidiscReport_Close;
                }
            }
        }

        Task<bool> IActionable.Task
        {
            get
            {
                if (this.Actions.Count > 0)
                {
                    return this.Behaviour.WriteDisc(this.Device, this.Actions);
                }
                else
                {
#if NET40
                    return TaskEx.FromResult(true);
#else
                    return Task.FromResult(true);
#endif
                }
            }
        }

        #endregion

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
                        string.IsNullOrEmpty(this.Track.Location) ? Strings.MinidiscReport_None : Strings.MinidiscReport_Add
                    };
                }
            }
        }
    }
}
