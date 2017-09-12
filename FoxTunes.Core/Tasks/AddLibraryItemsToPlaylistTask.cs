using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class AddLibraryItemsToPlaylistTask : PlaylistTaskBase
    {
        public const string ID = "4E0DD392-1138-4DA8-84C2-69B27D1E34EA";

        public AddLibraryItemsToPlaylistTask(int sequence, IEnumerable<LibraryItem> libraryItems) :
            base(ID)
        {
            this.Sequence = sequence;
            this.LibraryItems = libraryItems;
        }

        public int Sequence { get; private set; }

        public IEnumerable<LibraryItem> LibraryItems { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public IPlaylistItemFactory PlaylistItemFactory { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.PlaybackManager = core.Managers.Playback;
            this.PlaylistItemFactory = core.Factories.PlaylistItem;
            this.SignalEmitter = core.Components.SignalEmitter;
            base.InitializeComponent(core);
        }

        protected override async Task OnRun()
        {
            this.ShiftItems(this.Sequence, this.LibraryItems.Count());
            this.AddItems();
            await this.SaveChanges();
            this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistUpdated));
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
                select this.PlaylistItemFactory.Create(this.Sequence++, libraryItem);
            foreach (var playlistItem in query)
            {
                Logger.Write(this, LogLevel.Debug, "Adding item to playlist: {0} => {1}", playlistItem.Id, playlistItem.FileName);
                this.ForegroundTaskRunner.Run(() => this.Database.Interlocked(() => this.Playlist.PlaylistItemSet.Add(playlistItem)));
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
            this.IsIndeterminate = true;
            Logger.Write(this, LogLevel.Debug, "Saving changes to playlist.");
            return this.Database.Interlocked(async () => await this.Database.SaveChangesAsync());
        }
    }
}
