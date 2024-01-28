using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;

namespace FoxTunes.Managers
{
    public class PlaylistManager : StandardManager, IPlaylistManager
    {
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
            this.UpdateCurrentItem();
        }

        public void Add(IEnumerable<string> paths)
        {
            var task = new AddPathsToPlaylistTask(paths);
            task.InitializeComponent(this.Core);
            task.Completed += (sender, e) =>
            {
                this.OnUpdated();
            };
            this.OnBackgroundTask(task);
            task.Run();
        }

        public void Add(IEnumerable<LibraryItem> libraryItems)
        {
            var task = new AddLibraryItemsToPlaylistTask(libraryItems);
            task.InitializeComponent(this.Core);
            task.Completed += (sender, e) =>
            {
                this.OnUpdated();
            };
            this.OnBackgroundTask(task);
            task.Run();
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

        public void Next()
        {
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
            this.PlaybackManager.Load(this.Playlist.Set[index].FileName).Play();
        }

        public void Previous()
        {
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
            this.PlaybackManager.Load(this.Playlist.Set[index].FileName).Play();
        }

        public void Clear()
        {
            var task = new ClearPlaylistTask();
            task.InitializeComponent(this.Core);
            task.Completed += (sender, e) =>
            {
                this.OnUpdated();
            };
            this.OnBackgroundTask(task);
            task.Run();
        }

        protected virtual void UpdateCurrentItem()
        {
            if (this.PlaybackManager.CurrentStream != null)
            {
                foreach (var item in this.Playlist.Set)
                {
                    if (!string.Equals(item.FileName, this.PlaybackManager.CurrentStream.FileName, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    this.CurrentItem = item;
                    return;
                }
            }
            this.CurrentItem = null;
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
