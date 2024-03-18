using System.Windows;
using System.Windows.Controls;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for MiniPlaylist.xaml
    /// </summary>
    [UIComponent("19AB38D6-AFBB-48D8-9E76-EF6B2BFD75DA", role: UIComponentRole.Playlist)]
    public partial class MiniPlaylist : UIComponentBase
    {
        public MiniPlaylist()
        {
            this.InitializeComponent();
        }

        protected virtual void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!this.IsVisible)
            {
                return;
            }
            if (this.ListBox.SelectedItem != null)
            {
                this.ListBox.ScrollIntoView(this.ListBox.SelectedItem);
            }
        }

        protected virtual void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!this.IsVisible)
            {
                return;
            }
            var listBox = sender as ListBox;
            if (listBox == null || listBox.SelectedItem == null)
            {
                return;
            }
            listBox.ScrollIntoView(listBox.SelectedItem);
        }
    }
}
