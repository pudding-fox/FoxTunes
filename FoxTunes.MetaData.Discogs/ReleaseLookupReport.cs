using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FoxTunes
{
    public class ReleaseLookupReport : BaseComponent, IReport
    {
        public ReleaseLookupReport(IEnumerable<Discogs.ReleaseLookup> releaseLookups)
        {
            this.LookupItems = releaseLookups.ToDictionary(releaseLookup => Guid.NewGuid());
        }

        public Dictionary<Guid, Discogs.ReleaseLookup> LookupItems { get; private set; }

        public IUserInterface UserInterface { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.UserInterface = core.Components.UserInterface;
            base.InitializeComponent(core);
        }

        public string Title
        {
            get
            {
                return Strings.ReleaseLookupReport_Title;
            }
        }

        public string Description
        {
            get
            {
                return string.Join(
                    Environment.NewLine,
                    this.LookupItems.Values.Select(
                        releaseLookup => this.GetDescription(releaseLookup)
                    )
                );
            }
        }

        protected virtual string GetDescription(Discogs.ReleaseLookup releaseLookup)
        {
            var builder = new StringBuilder();
            builder.AppendFormat("{0} - {1}", releaseLookup.Artist, releaseLookup.Album);
            if (releaseLookup.Status != Discogs.ReleaseLookupStatus.Complete && releaseLookup.Errors.Any())
            {
                builder.AppendLine(" -> Error");
                foreach (var error in releaseLookup.Errors)
                {
                    builder.AppendLine('\t' + error);
                }
            }
            else
            {
                builder.AppendLine(" -> OK");
            }
            return builder.ToString();
        }

        public string[] Headers
        {
            get
            {
                return new[]
                {
                        Strings.ReleaseLookupReport_Album,
                        Strings.ReleaseLookupReport_Artist,
                        Strings.ReleaseLookupReport_Status,
                        Strings.ReleaseLookupReport_Release
                    };
            }
        }

        public IEnumerable<IReportRow> Rows
        {
            get
            {
                return this.LookupItems.Select(element => new ReportRow(element.Key, element.Value));
            }
        }

        public Action<Guid> Action
        {
            get
            {
                return key =>
                {
                    var releaseLookup = default(Discogs.ReleaseLookup);
                    if (!this.LookupItems.TryGetValue(key, out releaseLookup) || releaseLookup.Release == null)
                    {
                        return;
                    }
                    var url = new Uri(new Uri("https://www.discogs.com"), releaseLookup.Release.Url).ToString();
                    this.UserInterface.OpenInShell(url);
                };
            }
        }

        public class ReportRow : IReportRow
        {
            public ReportRow(Guid id, Discogs.ReleaseLookup releaseLookup)
            {
                this.Id = id;
                this.ReleaseLookup = releaseLookup;
            }

            public Guid Id { get; private set; }

            public Discogs.ReleaseLookup ReleaseLookup { get; private set; }

            public string[] Values
            {
                get
                {
                    var url = default(string);
                    if (this.ReleaseLookup.Release != null)
                    {
                        url = this.ReleaseLookup.Release.Url;
                    }
                    return new[]
                    {
                            this.ReleaseLookup.Artist,
                            this.ReleaseLookup.Album,
                            Enum.GetName(typeof(Discogs.ReleaseLookupStatus), this.ReleaseLookup.Status),
                            url
                        };
                }
            }
        }
    }
}
