using System;
using System.Threading.Tasks;
using System.Windows;

namespace FoxTunes.ViewModel
{
    public class TabPlaylist : GridPlaylist
    {
        public static readonly DependencyProperty PlaylistProperty = DependencyProperty.Register(
            "Playlist",
            typeof(Playlist),
            typeof(TabPlaylist),
            new PropertyMetadata(new PropertyChangedCallback(OnPlaylistChanged))
        );

        public static Playlist GetPlaylist(TabPlaylist source)
        {
            return (Playlist)source.GetValue(PlaylistProperty);
        }

        public static void SetPlaylist(TabPlaylist source, Playlist value)
        {
            source.SetValue(PlaylistProperty, value);
        }

        public static void OnPlaylistChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var tabPlaylist = sender as TabPlaylist;
            if (tabPlaylist == null)
            {
                return;
            }
            tabPlaylist.OnPlaylistChanged();
        }

        public Playlist Playlist
        {
            get
            {
                return this.GetValue(PlaylistProperty) as Playlist;
            }
            set
            {
                this.SetValue(PlaylistProperty, value);
            }
        }

        protected virtual void OnPlaylistChanged()
        {
            this.Dispatch(this.Refresh);
            if (this.PlaylistChanged != null)
            {
                this.PlaylistChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Playlist");
        }

        public event EventHandler PlaylistChanged;

        protected override async Task<Playlist> GetPlaylist()
        {
            var playlist = default(Playlist);
            await Windows.Invoke(() => playlist = this.Playlist).ConfigureAwait(false);
            return playlist;
        }

        protected override Freezable CreateInstanceCore()
        {
            return new TabPlaylist();
        }
    }
}
