using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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
            });
        }

        protected virtual void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listView = sender as ListView;
            if (listView == null || listView.SelectedItem == null)
            {
                return;
            }
            listView.ScrollIntoView(listView.SelectedItem);
        }
    }
}
