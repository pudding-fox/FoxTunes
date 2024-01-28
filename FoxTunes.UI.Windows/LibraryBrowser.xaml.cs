using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for LibraryBrowser.xaml
    /// </summary>
    public partial class LibraryBrowser : UserControl
    {
        private LibraryHierarchyNode SelectedNode { get; set; }

        private Point DragStartPosition { get; set; }

        private bool DragInitialized { get; set; }

        public LibraryBrowser()
        {
            InitializeComponent();
        }

        private void DoDragDrop()
        {
            this.DragInitialized = true;
            this.MouseCursorAdorner.Show();
            try
            {
                DragDrop.DoDragDrop(
                    this,
                    this.SelectedNode,
                    DragDropEffects.Copy
                );
            }
            finally
            {
                this.DragInitialized = false;
                this.MouseCursorAdorner.Hide();
            }
        }

        private void ListView_Selected(object sender, RoutedEventArgs e)
        {
            this.SelectedNode = (e.OriginalSource as ListViewItem).DataContext as LibraryHierarchyNode;
        }

        private void ListViewItem_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }
            this.DragStartPosition = e.GetPosition(null);
        }

        private void ListViewItem_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || this.DragInitialized || this.SelectedNode == null)
            {
                return;
            }
            var position = e.GetPosition(null);
            if (Math.Abs(position.X - this.DragStartPosition.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(position.Y - this.DragStartPosition.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                this.DoDragDrop();
            }
        }
    }
}
