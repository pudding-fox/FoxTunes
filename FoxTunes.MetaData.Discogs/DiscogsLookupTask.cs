using FoxTunes.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public abstract class DiscogsLookupTask : DiscogsTask
    {
        protected DiscogsLookupTask(Discogs.ReleaseLookup[] releaseLookups)
        {
            this.LookupItems = releaseLookups;
        }

        public override bool Visible
        {
            get
            {
                return true;
            }
        }

        public override bool Cancellable
        {
            get
            {
                return true;
            }
        }

        public Discogs.ReleaseLookup[] LookupItems { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public DoubleConfigurationElement MinConfidence { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Configuration = core.Components.Configuration;
            this.MinConfidence = this.Configuration.GetElement<DoubleConfigurationElement>(
                DiscogsBehaviourConfiguration.SECTION,
                DiscogsBehaviourConfiguration.MIN_CONFIDENCE
            );
            base.InitializeComponent(core);
        }

        protected override Task OnStarted()
        {
            this.Name = Strings.LookupArtworkTask_Name;
            this.Position = 0;
            this.Count = this.LookupItems.Length;
            return base.OnStarted();
        }

        protected override async Task OnRun()
        {
            var position = 0;
            foreach (var releaseLookup in this.LookupItems)
            {
                this.Description = string.Format("{0} - {1}", releaseLookup.Artist, releaseLookup.Album);
                this.Position = position;
                if (this.IsCancellationRequested)
                {
                    releaseLookup.Status = Discogs.ReleaseLookupStatus.Cancelled;
                    continue;
                }
                if (string.IsNullOrEmpty(releaseLookup.Artist) || string.IsNullOrEmpty(releaseLookup.Album))
                {
                    Logger.Write(this, LogLevel.Warn, "Cannot fetch releases, search requires at least an artist and album tag.");
                    releaseLookup.AddError(Strings.LookupArtworkTask_InsufficiantData);
                    releaseLookup.Status = Discogs.ReleaseLookupStatus.Failed;
                    continue;
                }
                try
                {
                    releaseLookup.Status = Discogs.ReleaseLookupStatus.Processing;
                    var success = await this.Lookup(releaseLookup).ConfigureAwait(false);
                    if (success)
                    {
                        releaseLookup.Status = Discogs.ReleaseLookupStatus.Complete;
                    }
                    else
                    {
                        releaseLookup.AddError(Strings.LookupArtworkTask_NotFound);
                        releaseLookup.Status = Discogs.ReleaseLookupStatus.Failed;
                    }
                }
                catch (Exception e)
                {
                    Logger.Write(this, LogLevel.Error, "Failed to lookup artwork: {0}", e.Message);
                    releaseLookup.AddError(e.Message);
                    releaseLookup.Status = Discogs.ReleaseLookupStatus.Failed;
                }
                position++;
            }
        }

        protected virtual async Task<bool> Lookup(Discogs.ReleaseLookup releaseLookup)
        {
            Logger.Write(this, LogLevel.Debug, "Fetching master releases for album: {0} - {1}", releaseLookup.Artist, releaseLookup.Album);
            var releases = await this.Discogs.GetReleases(releaseLookup.Artist, releaseLookup.Album, true).ConfigureAwait(false);
            if (!releases.Any())
            {
                Logger.Write(this, LogLevel.Warn, "No master releases for album: {0} - {1}, fetching others", releaseLookup.Artist, releaseLookup.Album);
                releases = await this.Discogs.GetReleases(releaseLookup.Artist, releaseLookup.Album, false).ConfigureAwait(false);
            }
            Logger.Write(this, LogLevel.Debug, "Ranking releases for album: {0} - {1}", releaseLookup.Artist, releaseLookup.Album);
            releaseLookup.Release = releases.ToDictionary(
                //Map results to similarity
                release => release,
                release => release.Similarity(releaseLookup.Artist, releaseLookup.Album)
            ).Where(
                //Where they have the required confidence.
                pair => pair.Value >= this.MinConfidence.Value
            ).OrderByDescending(
                //Order by highest confidence first.
                pair => pair.Value
            ).ThenByDescending(
                //Then by largest cover image.
                pair => pair.Key.CoverSize
            ).ThenByDescending(
                //Then by largest thumb size.
                pair => pair.Key.ThumbSize
            ).Select(
                //Select result.
                pair => pair.Key
            ).FirstOrDefault();
            if (releaseLookup.Release != null)
            {
                Logger.Write(this, LogLevel.Debug, "Best match for album {0} - {1}: {2}", releaseLookup.Artist, releaseLookup.Album, releaseLookup.Release.Url);
                return await this.OnLookupSuccess(releaseLookup).ConfigureAwait(false);
            }
            else
            {
                Logger.Write(this, LogLevel.Warn, "No matches for album {0} - {1}.", releaseLookup.Artist, releaseLookup.Album);
            }
            return false;
        }

        protected abstract Task<bool> OnLookupSuccess(Discogs.ReleaseLookup releaseLookup);
    }
}
