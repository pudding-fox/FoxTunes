using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Linq;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for Playlist.xaml
    /// </summary>
    public partial class Playlist : UserControl
    {
        public Playlist()
        {
            this.InitializeComponent();
        }

        protected virtual void DragSourceInitialized(object sender, ListViewExtensions.DragSourceInitializedEventArgs e)
        {
            var items = (e.Data as IEnumerable).OfType<PlaylistItem>();
            DragDrop.DoDragDrop(
                this,
                items,
                DragDropEffects.Copy
            );
        }
    }
}
