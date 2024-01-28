using System.Collections;
using System.Linq;
using System.Windows;

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
        }

        protected virtual void DragSourceInitialized(object sender, ListViewExtensions.DragSourceInitializedEventArgs e)
        {
            var items = (e.Data as IEnumerable).OfType<PlaylistItem>();
            if (!items.Any())
            {
                return;
            }
            DragDrop.DoDragDrop(
                this,
                items,
                DragDropEffects.Copy
            );
        }

        protected virtual void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = this.FindResource<global::FoxTunes.ViewModel.Playlist>("ViewModel");
            if (viewModel == null)
            {
                return;
            }
            var task = viewModel.RefreshColumns();
        }
    }
}
