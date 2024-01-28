using System;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for DefaultPlaylist.xaml
    /// </summary>
    [UIComponent("12023E31-CB53-4F9C-8A5B-A0593706F37E", UIComponentSlots.BOTTOM_CENTER, "Playlist")]
    public partial class DefaultPlaylist : UIComponentBase
    {
        public static readonly DependencyProperty PlaylistProperty = DependencyProperty.Register(
            "Playlist",
            typeof(Playlist),
            typeof(DefaultPlaylist),
            new PropertyMetadata(new PropertyChangedCallback(OnPlaylistChanged))
        );

        public static Playlist GetPlaylist(DefaultPlaylist source)
        {
            return (Playlist)source.GetValue(PlaylistProperty);
        }

        public static void SetPlaylist(DefaultPlaylist source, Playlist value)
        {
            source.SetValue(PlaylistProperty, value);
        }

        public static void OnPlaylistChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var defaultPlaylist = sender as DefaultPlaylist;
            if (defaultPlaylist == null)
            {
                return;
            }
            defaultPlaylist.OnPlaylistChanged();
        }

        public DefaultPlaylist()
        {
            this.InitializeComponent();
#if NET40
            //VirtualizingStackPanel.IsVirtualizingWhenGrouping is not supported.
            //The behaviour which requires this feature is disabled.
#else
            VirtualizingStackPanel.SetIsVirtualizingWhenGrouping(this.ListView, true);
#endif
            this.IsVisibleChanged += this.OnIsVisibleChanged;
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
            if (this.PlaylistChanged != null)
            {
                this.PlaylistChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Playlist");
        }

        public event EventHandler PlaylistChanged;

        protected virtual void DragSourceInitialized(object sender, ListViewExtensions.DragSourceInitializedEventArgs e)
        {
            var items = (e.Data as IEnumerable).OfType<PlaylistItem>();
            if (!items.Any())
            {
                return;
            }
            DragDrop.DoDragDrop(
                this.ListView,
                items,
                DragDropEffects.Copy
            );
        }

        protected virtual async void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            await Windows.Invoke(() =>
            {
                if (this.ListView.SelectedItem != null)
                {
                    this.ListView.ScrollIntoView(this.ListView.SelectedItem);
                }
            }).ConfigureAwait(false);
        }

        protected virtual void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listView = sender as ListView;
            if (listView == null || listView.SelectedItem == null)
            {
                return;
            }
            if (listView.SelectedItems != null && listView.SelectedItems.Count > 0)
            {
                //When multi-selecting don't mess with the scroll position.
                return;
            }
            listView.ScrollIntoView(listView.SelectedItem);
        }

        protected virtual void OnGroupHeaderMouseDown(object sender, MouseButtonEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null)
            {
                return;
            }
            var group = element.DataContext as CollectionViewGroup;
            if (group == null)
            {
                return;
            }
            this.ListView.SelectedItems.Clear();
            foreach (var item in group.Items)
            {
                this.ListView.SelectedItems.Add(item);
            }
        }
    }
}