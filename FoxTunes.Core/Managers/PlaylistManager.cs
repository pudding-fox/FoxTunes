using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

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

        public void Add(IEnumerable<string> paths)
        {
            var fileNames = new List<string>();
            foreach (var path in paths)
            {
                if (Directory.Exists(path))
                {
                    fileNames.AddRange(Directory.GetFiles(path, "*.*", SearchOption.AllDirectories));
                }
                else if (File.Exists(path))
                {
                    fileNames.Add(path);
                }
            }
            this.AddFiles(fileNames);
        }

        protected virtual void AddFiles(IEnumerable<string> fileNames)
        {
            var query =
                from fileName in fileNames
                where this.PlaybackManager.IsSupported(fileName)
                select this.PlaylistItemFactory.Create(fileName);
            this.Playlist.Set.AddRange(this.OrderBy(query));
            this.Database.SaveChanges();
            this.OnUpdated();
        }

        public void Add(IEnumerable<LibraryItem> libraryItems)
        {
            var query =
                from libraryItem in libraryItems
                where this.PlaybackManager.IsSupported(libraryItem.FileName)
                select this.PlaylistItemFactory.Create(libraryItem);
            this.Playlist.Set.AddRange(this.OrderBy(query));
            this.Database.SaveChanges();
            this.OnUpdated();
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

        private IEnumerable<PlaylistItem> OrderBy(IEnumerable<PlaylistItem> playlistItems)
        {
            var query =
                from playlistItem in playlistItems
                orderby
                    Path.GetDirectoryName(playlistItem.FileName),
                    playlistItem.MetaDatas.Value<int>(CommonMetaData.Track),
                    playlistItem.FileName
                select playlistItem;
            return query;
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
            this.Playlist.Set.Clear();
            this.Database.SaveChanges();
            this.OnUpdated();
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
    }
}
