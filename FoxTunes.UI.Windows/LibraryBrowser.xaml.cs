using System.Windows;
using System.Windows.Controls;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for LibraryBrowser.xaml
    /// </summary>
    public partial class LibraryBrowser : UserControl
    {
        public LibraryBrowser()
        {
            this.InitializeComponent();
        }

        protected virtual void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox == null || listBox.SelectedItem == null)
            {
                return;
            }
            listBox.ScrollIntoView(listBox.SelectedItem);
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

        protected virtual void DragSourceInitialized(object sender, ListBoxExtensions.DragSourceInitializedEventArgs e)
        {
            this.MouseCursorAdorner.Show();
            try
            {
                DragDrop.DoDragDrop(
                    this,
                    e.Data,
                    DragDropEffects.Copy
                );
            }
            finally
            {
                this.MouseCursorAdorner.Hide();
            }
        }
    }
}
