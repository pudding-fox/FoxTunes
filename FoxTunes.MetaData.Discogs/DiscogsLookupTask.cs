using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
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

        public ILibraryManager LibraryManager { get; private set; }

        public IMetaDataManager MetaDataManager { get; private set; }

        public IHierarchyManager HierarchyManager { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.LibraryManager = core.Managers.Library;
            this.MetaDataManager = core.Managers.MetaData;
            this.HierarchyManager = core.Managers.Hierarchy;
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
            Logger.Write(this, LogLevel.Debug, "Fetching releases for album: {0} - {1}", releaseLookup.Artist, releaseLookup.Album);
            var releases = await this.Discogs.GetReleases(releaseLookup.Artist, releaseLookup.Album).ConfigureAwait(false);
            Logger.Write(this, LogLevel.Debug, "Ranking releases for album: {0} - {1}", releaseLookup.Artist, releaseLookup.Album);
            //Get the top release by title similarity, then by largest available image.
            releaseLookup.Release = releases
                .OrderByDescending(release => release.Similarity(releaseLookup.Artist, releaseLookup.Album))
                .ThenByDescending(release => release.CoverSize)
                .ThenByDescending(release => release.ThumbSize)
                .FirstOrDefault();
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

        protected virtual void UpdateMetaData(Discogs.ReleaseLookup releaseLookup, string name, string value, MetaDataItemType type)
        {
            foreach (var fileData in releaseLookup.FileDatas)
            {
                lock (fileData.MetaDatas)
                {
                    bool updated = false;
                    foreach (var metaDataItem in fileData.MetaDatas)
                    {
                        if (string.Equals(metaDataItem.Name, name, StringComparison.OrdinalIgnoreCase) && metaDataItem.Type == type)
                        {
                            metaDataItem.Value = value;
                            updated = true;
                            break;
                        }
                    }
                    if (!updated)
                    {
                        fileData.MetaDatas.Add(new MetaDataItem(name, type)
                        {
                            Value = value
                        });
                    }
                }
            }
        }

        protected virtual async Task SaveMetaData(params string[] names)
        {
            var libraryItems = new List<LibraryItem>();
            var playlistItems = new List<PlaylistItem>();
            foreach (var releaseLookup in this.LookupItems)
            {
                if (releaseLookup.Status != Discogs.ReleaseLookupStatus.Complete)
                {
                    continue;
                }

                libraryItems.AddRange(releaseLookup.FileDatas.OfType<LibraryItem>());
                playlistItems.AddRange(releaseLookup.FileDatas.OfType<PlaylistItem>());
            }
            if (libraryItems.Any())
            {
                await this.MetaDataManager.Save(libraryItems, true, false, names).ConfigureAwait(false);
            }
            if (playlistItems.Any())
            {
                await this.MetaDataManager.Save(playlistItems, true, false, names).ConfigureAwait(false);
            }
            await this.HierarchyManager.Clear(LibraryItemStatus.Import, false).ConfigureAwait(false);
            await this.HierarchyManager.Build(LibraryItemStatus.Import).ConfigureAwait(false);
            await this.LibraryManager.SetStatus(libraryItems, LibraryItemStatus.None).ConfigureAwait(false);
        }
    }
}
