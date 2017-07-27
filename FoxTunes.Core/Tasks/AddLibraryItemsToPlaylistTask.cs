using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FoxTunes
{
    public class AddLibraryItemsToPlaylistTask : PlaylistTask
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

        public override void InitializeComponent(ICore core)
        {
            this.Playlist = core.Components.Playlist;
            this.PlaybackManager = core.Managers.Playback;
            this.PlaylistItemFactory = core.Factories.PlaylistItem;
            base.InitializeComponent(core);
        }

        protected override void OnRun()
        {
            this.AddItems();
            this.SaveChanges();
        }

        private void AddItems()
        {
            this.SetName("Processing files");
            this.SetPosition(0);
            this.SetCount(this.LibraryItems.Count());
            var interval = Math.Max(Convert.ToInt32(this.Count * 0.01), 1);
            var position = 0;
            var query =
                from libraryItem in this.LibraryItems
                where this.PlaybackManager.IsSupported(libraryItem.FileName)
                select this.PlaylistItemFactory.Create(libraryItem);
            foreach(var playlistItem in this.OrderBy(query))
            {
                this.ForegroundTaskRunner.Run(() => this.Playlist.Set.Add(playlistItem));
                if (position % interval == 0)
                {
                    this.SetDescription(Path.GetFileName(playlistItem.FileName));
                    this.SetPosition(position);
                }
                position++;
            }
            this.SetPosition(this.Count);
        }
    }
}
