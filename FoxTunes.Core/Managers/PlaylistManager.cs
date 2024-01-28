using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes.Managers
{
    public class PlaylistManager : StandardManager, IPlaylistManager
    {
        private volatile bool IsNavigating = false;

        public ICore Core { get; private set; }

        public IPlaylist Playlist { get; private set; }

        public IDatabase Database { get; private set; }

        public IPlaylistItemFactory PlaylistItemFactory { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.Playlist = core.Components.Playlist;
            this.Database = core.Components.Database;
            this.PlaylistItemFactory = core.Factories.PlaylistItem;
            this.PlaybackManager = core.Managers.Playback;
            this.PlaybackManager.CurrentStreamChanged += this.PlaybackManager_CurrentStreamChanged;
            base.InitializeComponent(core);
        }

        protected virtual void PlaybackManager_CurrentStreamChanged(object sender, EventArgs e)
        {
            Logger.Write(this, LogLevel.Debug, "Playback manager output stream changed, updating current playlist item.");
            if (this.PlaybackManager.CurrentStream == null)
            {
                this.CurrentItem = null;
                Logger.Write(this, LogLevel.Debug, "Playback manager output stream is empty. Cleared current playlist item.");
            }
            else if (this.PlaybackManager.CurrentStream.PlaylistItem != this.CurrentItem)
            {
                this.CurrentItem = this.PlaybackManager.CurrentStream.PlaylistItem;
                Logger.Write(this, LogLevel.Debug, "Updated current playlist item: {0} => {1}", this.CurrentItem.Id, this.CurrentItem.FileName);
            }
        }

        public Task Add(IEnumerable<string> paths)
        {
            var task = new AddPathsToPlaylistTask(paths);
            task.InitializeComponent(this.Core);
            this.OnBackgroundTask(task);
            return task.Run().ContinueWith(_ => this.OnUpdated());
        }

        public Task Add(IEnumerable<LibraryItem> libraryItems)
        {
            var task = new AddLibraryItemsToPlaylistTask(libraryItems);
            task.InitializeComponent(this.Core);
            this.OnBackgroundTask(task);
            return task.Run().ContinueWith(_ => this.OnUpdated());
        }

        protected virtual void OnUpdated()
        {
            if (this.Updated == null)
            {
                return;
            }
            this.Updated(this, EventArgs.Empty);
        }

        public event EventHandler Updated = delegate { };

        public async Task Next()
        {
            Logger.Write(this, LogLevel.Debug, "Navigating to next playlist item.");
            if (this.IsNavigating)
            {
                Logger.Write(this, LogLevel.Debug, "Already navigating, ignoring request.");
                return;
            }
            try
            {
                this.IsNavigating = true; ;
                var index = default(int);
                if (this.CurrentItem == null)
                {
                    Logger.Write(this, LogLevel.Debug, "Current playlist item is empty, assuming first item.");
                    index = 0;
                }
                else
                {
                    index = this.Playlist.Set.IndexOf(this.CurrentItem) + 1;
                    Logger.Write(this, LogLevel.Debug, "Next playlist item is index: {0}", index);
                }
                if (index >= this.Playlist.Set.Count)
                {
                    index = 0;
                    Logger.Write(this, LogLevel.Debug, "Index was too large, wrapping around to first item.");
                }
                var playlistItem = this.Playlist.Set[index];
                Logger.Write(this, LogLevel.Debug, "Playing playlist item: {0} => {1}", playlistItem.Id, playlistItem.FileName);
                await this.Play(playlistItem);
            }
            finally
            {
                this.IsNavigating = false;
            }
        }

        public async Task Previous()
        {
            Logger.Write(this, LogLevel.Debug, "Navigating to previous playlist item.");
            if (this.IsNavigating)
            {
                Logger.Write(this, LogLevel.Debug, "Already navigating, ignoring request.");
                return;
            }
            try
            {
                this.IsNavigating = true; ;
                var index = default(int);
                if (this.CurrentItem == null)
                {
                    Logger.Write(this, LogLevel.Debug, "Current playlist item is empty, assuming first item.");
                    index = 0;
                }
                else
                {
                    index = this.Playlist.Set.IndexOf(this.CurrentItem) - 1;
                    Logger.Write(this, LogLevel.Debug, "Previous playlist item is index: {0}", index);
                }
                if (index < 0)
                {
                    index = this.Playlist.Set.Count - 1;
                    Logger.Write(this, LogLevel.Debug, "Index was too small, wrapping around to last item.");
                }
                var playlistItem = this.Playlist.Set[index];
                Logger.Write(this, LogLevel.Debug, "Playing playlist item: {0} => {1}", playlistItem.Id, playlistItem.FileName);
                await this.Play(playlistItem);
            }
            finally
            {
                this.IsNavigating = false;
            }
        }

        private Task Play(PlaylistItem playlistItem)
        {
            return this.PlaybackManager
                .Load(playlistItem, true)
                .ContinueWith(_ =>
                {
                    if (this.PlaybackManager.CurrentStream.PlaylistItem != playlistItem)
                    {
                        Logger.Write(this, LogLevel.Warn, "Expected current output stream to be {0} => {1} but it was {2} => {3}", playlistItem.Id, playlistItem.FileName, this.PlaybackManager.CurrentStream.PlaylistItem.Id, this.PlaybackManager.CurrentStream.PlaylistItem.FileName);
                        return;
                    }
                    Logger.Write(this, LogLevel.Debug, "Playing current output stream: {0} => {1}", playlistItem.Id, playlistItem.FileName);
                    this.PlaybackManager.CurrentStream.Play();
                });
        }

        public Task Clear()
        {
            var task = new ClearPlaylistTask();
            task.InitializeComponent(this.Core);
            this.OnBackgroundTask(task);
            return task.Run().ContinueWith(_ => this.OnUpdated());
        }

        private PlaylistItem _CurrentItem { get; set; }

        public PlaylistItem CurrentItem
        {
            get
            {
                return this._CurrentItem;
            }
            private set
            {
                this._CurrentItem = value;
                this.OnCurrentItemChanged();
            }
        }

        protected virtual void OnCurrentItemChanged()
        {
            if (this.CurrentItemChanged != null)
            {
                this.CurrentItemChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("CurrentItem");
        }

        public event EventHandler CurrentItemChanged = delegate { };

        protected virtual void OnBackgroundTask(IBackgroundTask backgroundTask)
        {
            if (this.BackgroundTask == null)
            {
                return;
            }
            this.BackgroundTask(this, new BackgroundTaskEventArgs(backgroundTask));
        }

        public event BackgroundTaskEventHandler BackgroundTask = delegate { };
    }
}
