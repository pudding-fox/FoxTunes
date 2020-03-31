using System.Windows;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for LibraryTree.xaml
    /// </summary>
    [UIComponent("86276AD4-3962-4659-A00F-95065CD92117", UIComponentSlots.TOP_LEFT, "Library Tree")]
    [UIComponentDependency(MetaDataBehaviourConfiguration.SECTION, MetaDataBehaviourConfiguration.ENABLE_ELEMENT)]
    public partial class LibraryTree : UIComponentBase
    {
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
    }
}
