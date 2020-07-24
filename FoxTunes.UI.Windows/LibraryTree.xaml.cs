using System.Windows;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for LibraryTree.xaml
    /// </summary>
    [UIComponent(ID, "Library Tree")]
    public partial class LibraryTree : UIComponentBase
    {
        public const string ID = "86276AD4-3962-4659-A00F-95065CD92117";

        public LibraryTree()
        {
            this.InitializeComponent();
        }

        protected virtual void DragSourceInitialized(object sender, TreeViewExtensions.DragSourceInitializedEventArgs e)
        {
            var viewModel = this.FindResource<global::FoxTunes.ViewModel.LibraryTree>("ViewModel");
            if (viewModel != null)
            {
                if (LibraryHierarchyNode.Empty.Equals(viewModel.SelectedItem))
                {
                    return;
                }
                //Only show adorners when hosted in main window.
                if (this.IsHostedIn<MainWindow>() && viewModel.ShowCursorAdorners)
                {
                    this.MouseCursorAdorner.Show();
                }
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
    }
}
