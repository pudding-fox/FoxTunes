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
        public MinidiscReport(MinidiscBehaviour behaviour, IDevice device, IActions actions)
        {
            this.Behaviour = behaviour;
            this.Device = device;
            this.Actions = actions;
        }

        public MinidiscBehaviour Behaviour { get; private set; }

        public IDevice Device { get; private set; }

        public IActions Actions { get; private set; }

        public override string Title
        {
            get
            {
                return string.Format("{0} ({1}%)", this.Actions.UpdatedDisc.Title, this.Actions.UpdatedDisc.GetCapacity().PercentUsed);
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
                return this.Actions.UpdatedDisc.Tracks.Select(track => new MinidiscReportRow(track));
            }
        }

        public override string ActionName
        {
            get
            {
                if (this.Actions.Count > 0)
                {
                    return Strings.MinidiscReport_Write;
                }
                return string.Empty;
            }
        }

        public override Task<bool> Action()
        {
            if (this.Actions.Count > 0)
            {
#if NET40
                return TaskEx.Run(async () =>
#else
                return Task.Run(async () =>
#endif
                {
                    var success = await this.Behaviour.WriteDisc(this.Device, this.Actions).ConfigureAwait(false);
                    if (!success)
                    {
                        //If disc was not written then show this report again.
                        await this.Behaviour.ReportEmitter.Send(this).ConfigureAwait(false);
                        return false;
                    }
                    return true;
                });
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

        public class MinidiscReportRow : ReportComponentRow
        {
            public MinidiscReportRow(ITrack track)
            {
                this.Track = track;
            }

            public ITrack Track { get; private set; }

            public override string[] Values
            {
                get
                {
                    return new[]
                    {
                        this.Track.Name,
                        this.Track.Time.ToString(@"mm\:ss"),
                        Enum.GetName(typeof(Compression), this.Track.Compression),
                        string.IsNullOrEmpty(this.Track.Location) ? Strings.MinidiscReport_None : Strings.MinidiscReport_Add
                    };
                }
            }
        }
    }
}
