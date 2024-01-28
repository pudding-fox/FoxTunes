using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace FoxTunes
{
    public class PlaylistItemCollection : ObservableCollection<PlaylistItem>
    {
        private const string COUNT = "Count";

        private const string INDEXER = "Item[]";

        public readonly object SyncRoot = new object();

        public PlaylistItemCollection(IEnumerable<PlaylistItem> playlistItems) : base(playlistItems)
        {

        }

        public bool IsSuspended { get; private set; }

        public Action Reset(PlaylistItem[] playlistItems)
        {
            lock (this.SyncRoot)
            {
                this.IsSuspended = true;
                try
                {
                    this.Clear();
                    this.AddRange(playlistItems);
                }
                finally
                {
                    this.IsSuspended = false;
                }
            }
            return () =>
            {
                this.OnPropertyChanged(new PropertyChangedEventArgs(COUNT));
                this.OnPropertyChanged(new PropertyChangedEventArgs(INDEXER));
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            };
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (this.IsSuspended)
            {
                return;
            }
            base.OnCollectionChanged(e);
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (this.IsSuspended)
            {
                return;
            }
            base.OnPropertyChanged(e);
        }
    }
}
