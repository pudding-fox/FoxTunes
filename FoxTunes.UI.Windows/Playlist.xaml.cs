using FoxDb;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for Playlist.xaml
    /// </summary>
    [UIComponent("12023E31-CB53-4F9C-8A5B-A0593706F37E", UIComponentSlots.BOTTOM_CENTER, "Playlist")]
    public partial class Playlist : UIComponentBase
    {
        public Playlist()
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