using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class AddLibraryItemsToPlaylistTask : BackgroundTask
    {
        public const string ID = "4E0DD392-1138-4DA8-84C2-69B27D1E34EA";

        public AddLibraryItemsToPlaylistTask(IEnumerable<LibraryItem> libraryItems) :
            base(ID)
        {
            this.LibraryItems = libraryItems;
        }

        public IEnumerable<LibraryItem> LibraryItems { get; private set; }

        public IPlaylist Playlist { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public IPlaylistItemFactory PlaylistItemFactory { get; private set; }

        public IDatabase Database { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Playlist = core.Components.Playlist;
            this.PlaybackManager = core.Managers.Playback;
            this.PlaylistItemFactory = core.Factories.PlaylistItem;
            this.Database = core.Components.Database;
            base.InitializeComponent(core);
        }

        protected override Task OnRun()
        {
            this.AddItems();
            return this.SaveChanges();
        }

        private void AddItems()
        {
            this.Name = "Processing library items";
            this.Position = 0;
            this.Count = this.LibraryItems.Count();
            var interval = Math.Max(Convert.ToInt32(this.Count * 0.01), 1);
            var position = 0;
            Logger.Write(this, LogLevel.Debug, "Converting library items to playlist items.");
            var query =
                from libraryItem in this.LibraryItems
                where this.PlaybackManager.IsSupported(libraryItem.FileName)
                select this.PlaylistItemFactory.Create(libraryItem);
            foreach (var playlistItem in query)
            {
                Logger.Write(this, LogLevel.Debug, "Adding item to playlist: {0} => {1}", playlistItem.Id, playlistItem.FileName);
                this.ForegroundTaskRunner.Run(() => this.Database.Interlocked(() => this.Playlist.Set.Add(playlistItem)));
                if (position % interval == 0)
                {
                    this.Description = Path.GetFileName(playlistItem.FileName);
                    this.Position = position;
                }
                position++;
            }
            this.Position = this.Count;
        }

        private Task SaveChanges()
        {
            this.Name = "Saving changes";
            this.Position = this.Count;
            Logger.Write(this, LogLevel.Debug, "Saving changes to playlist.");
            return this.Database.Interlocked(() => this.Database.SaveChangesAsync());
        }
    }
}
