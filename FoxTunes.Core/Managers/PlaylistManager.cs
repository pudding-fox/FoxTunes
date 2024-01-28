using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace FoxTunes.Managers
{
    public class PlaylistManager : StandardManager, IPlaylistManager
    {
        public IPlaylist Playlist { get; private set; }

        public IDatabase Database { get; private set; }

        public IPlaylistItemFactory PlaylistItemFactory { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public override void InitializeComponent(ICore core)
        {
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

        public void AddDirectory(string directoryName)
        {
            foreach (var fileName in Directory.GetFiles(directoryName))
            {
                this.AddFile(fileName);
            }
        }

        public void AddFile(string fileName)
        {
            if (!this.PlaybackManager.IsSupported(fileName))
            {
                return;
            }
            this.Playlist.Items.Add(this.PlaylistItemFactory.Create(fileName));
        }

        public void Next()
        {
            var index = default(int);
            if (this.CurrentItem == null)
            {
                index = 0;
            }
            else
            {
                index = this.Playlist.Items.IndexOf(this.CurrentItem) + 1;
            }
            if (index >= this.Playlist.Items.Count)
            {
                index = 0;
            }
            this.PlaybackManager.Load(this.Playlist.Items[index].FileName).Play();
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
                index = this.Playlist.Items.IndexOf(this.CurrentItem) - 1;
            }
            if (index < 0)
            {
                index = this.Playlist.Items.Count - 1;
            }
            this.PlaybackManager.Load(this.Playlist.Items[index].FileName).Play();
        }

        public void Clear()
        {
            this.Playlist.Items.Clear();
        }

        protected virtual void UpdateCurrentItem()
        {
            if (this.PlaybackManager.CurrentStream != null)
            {
                foreach (var item in this.Items)
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

        public ObservableCollection<PlaylistItem> Items
        {
            get
            {
                return this.Playlist.Items;
            }
        }
    }
}
