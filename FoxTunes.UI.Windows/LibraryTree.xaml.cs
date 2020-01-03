using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for LibraryTree.xaml
    /// </summary>
    [UIComponent("86276AD4-3962-4659-A00F-95065CD92117", UIComponentSlots.TOP_LEFT, "Library Tree")]
    [UIComponentDependency(MetaDataBehaviourConfiguration.SECTION, MetaDataBehaviourConfiguration.ENABLE_ELEMENT)]
    public partial class LibraryTree : UIComponentBase
    {
        const int EXPAND_ALL_LIMIT = 5;

        public LibraryTree()
        {
            this.InitializeComponent();
        }

        protected virtual void DragSourceInitialized(object sender, TreeViewExtensions.DragSourceInitializedEventArgs e)
        {
            var viewModel = this.FindResource<global::FoxTunes.ViewModel.LibraryTree>("ViewModel");
            if (viewModel != null && viewModel.ShowCursorAdorners)
            {
                this.MouseCursorAdorner.DataContext = viewModel.SelectedItem;
                this.MouseCursorAdorner.Show();
            }
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
                if (this.MouseCursorAdorner.IsVisible)
                {
                    this.MouseCursorAdorner.Hide();
                }
            }
        }

        protected virtual void OnSearchCompleted(object sender, System.EventArgs e)
        {
            var view = CollectionViewSource.GetDefaultView(this.TreeView.ItemsSource) as CollectionView;
            if (view == null || view.Count > EXPAND_ALL_LIMIT)
            {
                return;
            }
            var task = this.ExpandAll(view.OfType<LibraryHierarchyNode>());
        }

        public async Task ExpandAll(IEnumerable<LibraryHierarchyNode> sequence)
        {
            var stack = new Stack<LibraryHierarchyNode>(sequence);
            while (stack.Count > 0)
            {
                var node = stack.Pop();
                if (!node.IsExpanded)
                {
                    await Windows.Invoke(() => node.IsExpanded = true).ConfigureAwait(false);
                }
                foreach (var child in node.Children)
                {
                    if (child.IsLeaf)
                    {
                        continue;
                    }
                    stack.Push(child);
                }
            }
        }
    }
}
