using System.Windows;
using System.Windows.Controls;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for LibraryTree.xaml
    /// </summary>
    public partial class LibraryTree : UserControl
    {
        public LibraryTree()
        {
            this.InitializeComponent();
        }

        protected virtual void DragSourceInitialized(object sender, TreeViewExtensions.DragSourceInitializedEventArgs e)
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
