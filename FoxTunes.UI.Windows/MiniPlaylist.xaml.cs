using System.Windows;
using System.Windows.Controls;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for MiniPlaylist.xaml
    /// </summary>
    public partial class MiniPlaylist : UserControl
    {
        public MiniPlaylist()
        {
            this.InitializeComponent();
            this.IsVisibleChanged += this.OnIsVisibleChanged;
        }

        protected virtual async void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            await Windows.Invoke(() =>
            {
                if (this.ListBox.SelectedItem != null)
                {
                    this.ListBox.ScrollIntoView(this.ListBox.SelectedItem);
                }
            });
        }

        protected virtual void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox == null || listBox.SelectedItem == null)
            {
                return;
            }
            listBox.ScrollIntoView(listBox.SelectedItem);
        }
    }
}
