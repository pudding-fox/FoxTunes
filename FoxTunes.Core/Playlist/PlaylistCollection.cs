using FoxDb;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace FoxTunes
{
    public class PlaylistCollection : ObservableCollection<Playlist>
    {
        public readonly object SyncRoot = new object();

        public PlaylistCollection(IEnumerable<Playlist> playlists) : base(playlists)
        {

        }

        public void Update(Playlist[] playlists)
        {
            lock (this.SyncRoot)
            {
                for (var position = this.Count - 1; position >= 0; position--)
                {
                    if (!playlists.Contains(this[position]))
                    {
                        this.RemoveAt(position);
                    }
                }
                for (var position = 0; position < playlists.Length; position++)
                {
                    var index = this.IndexOf(playlists[position]);
                    if (index == -1)
                    {
                        this.Insert(position, playlists[position]);
                    }
                    else
                    {
                        if (index != position)
                        {
                            var playlist = this[index];
                            this.RemoveAt(index);
                            this.Insert(position, playlist);
                        }
                        this[index].Name = playlists[position].Name;
                    }
                }
            }
        }
    }
}
