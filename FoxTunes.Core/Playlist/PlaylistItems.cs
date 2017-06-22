using FoxTunes.Interfaces;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace FoxTunes
{
    public class PlaylistItems : BaseComponent, IPlaylistItems
    {
        private PlaylistItems()
        {
            this.Items = new ObservableCollection<IPlaylistItem>();
            this.Items.CollectionChanged += (sender, e) => this.CollectionChanged(this, e);
        }

        public PlaylistItems(IPlaylist playlist)
            : this()
        {
            this.Playlist = playlist;
        }

        public IPlaylist Playlist { get; private set; }

        private ObservableCollection<IPlaylistItem> Items { get; set; }

        public IPlaylistItem Create(string fileName)
        {
            return new PlaylistItem(this.Playlist, this, fileName);
        }

        public void Add(IPlaylistItem item)
        {
            this.Items.Add(item);
        }

        public void Clear()
        {
            this.Items.Clear();
        }

        public bool Contains(IPlaylistItem item)
        {
            return this.Items.Contains(item);
        }

        public void CopyTo(IPlaylistItem[] array, int arrayIndex)
        {
            this.Items.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get
            {
                return this.Items.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool Remove(IPlaylistItem item)
        {
            return this.Items.Remove(item);
        }

        public int IndexOf(IPlaylistItem item)
        {
            return this.Items.IndexOf(item);
        }

        public IPlaylistItem this[int index]
        {
            get
            {
                return this.Items[index];
            }
            set
            {
                this.Items[index] = value;
            }
        }

        public IEnumerator<IPlaylistItem> GetEnumerator()
        {
            return this.Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.Items.GetEnumerator();
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged = delegate { };
    }
}
