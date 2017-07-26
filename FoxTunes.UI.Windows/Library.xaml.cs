using FoxTunes.Interfaces;
using FoxTunes.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for Library.xaml
    /// </summary>
    public partial class Library : UserControl
    {
        private LibraryNode SelectedNode { get; set; }

        private Point DragStartPosition { get; set; }

        private bool DragInitialized { get; set; }

        public Library()
        {
            InitializeComponent();
        }

        public ICore Core
        {
            get
            {
                return this.DataContext as ICore;
            }
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            var effects = DragDropEffects.None;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                effects = DragDropEffects.Copy;
            }
            e.Effects = effects;
            base.OnDragEnter(e);
        }

        protected override void OnDrop(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var paths = e.Data.GetData(DataFormats.FileDrop) as IEnumerable<string>;
                this.Core.Managers.Library.Add(paths);
            }
            base.OnDrop(e);
        }

        private void DoDragDrop()
        {
            this.DragInitialized = true;
            DragDrop.DoDragDrop(
                this,
                this.SelectedNode.LibraryItems,
                DragDropEffects.Copy
            );
            this.DragInitialized = false;
        }

        private void TreeView_Selected(object sender, RoutedEventArgs e)
        {
            this.SelectedNode = (e.OriginalSource as TreeViewItem).DataContext as LibraryNode;
        }

        private void TreeViewItem_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }
            this.DragStartPosition = e.GetPosition(null);
        }

        private void TreeViewItem_PreviewMouseMove(object sender, MouseEventArgs e)
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
