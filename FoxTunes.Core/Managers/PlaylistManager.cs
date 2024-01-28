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
            if (this.PlaybackManager.CurrentStream == null)
            {
                this.CurrentItem = null;
            }
            else if (this.PlaybackManager.CurrentStream.PlaylistItem != this.CurrentItem)
            {
                this.CurrentItem = this.PlaybackManager.CurrentStream.PlaylistItem;
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
            if (this.IsNavigating)
            {
                return;
            }
            try
            {
                this.IsNavigating = true; ;
                var index = default(int);
                if (this.CurrentItem == null)
                {
                    index = 0;
                }
                else
                {
                    index = this.Playlist.Set.IndexOf(this.CurrentItem) + 1;
                }
                if (index >= this.Playlist.Set.Count)
                {
                    index = 0;
                }
                await this.Play(this.Playlist.Set[index]);
            }
            finally
            {
                this.IsNavigating = false;
            }
        }

        public async Task Previous()
        {
            if (this.IsNavigating)
            {
                return;
            }
            try
            {
                this.IsNavigating = true; ;
                var index = default(int);
                if (this.CurrentItem == null)
                {
                    index = 0;
                }
                else
                {
                    index = this.Playlist.Set.IndexOf(this.CurrentItem) - 1;
                }
                if (index < 0)
                {
                    index = this.Playlist.Set.Count - 1;
                }
                await this.Play(this.Playlist.Set[index]);
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
                        return;
                    }
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
