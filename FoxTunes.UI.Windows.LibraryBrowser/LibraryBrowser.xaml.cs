using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for LibraryBrowser.xaml
    /// </summary>
    [UIComponent("FB75ECEC-A89A-4DAD-BA8D-9DB43F3DE5E3", UIComponentSlots.TOP_CENTER, "Library Browser")]
    [UIComponentDependency(MetaDataBehaviourConfiguration.SECTION, MetaDataBehaviourConfiguration.ENABLE_ELEMENT)]
    public partial class LibraryBrowser : UIComponentBase
    {
        public static readonly ArtworkGridLoader ArtworkGridLoader = ComponentRegistry.Instance.GetComponent<ArtworkGridLoader>();

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

        protected virtual void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.FixFocus();
        }

        protected virtual void OnUnloaded(object sender, RoutedEventArgs e)
        {
            this.FixFocus();
        }

        protected virtual void FixFocus()
        {
            if (!this.ItemsControl.IsKeyboardFocusWithin)
            {
                return;
            }
            Keyboard.ClearFocus();
            var container = this.ItemsControl.ItemContainerGenerator.ContainerFromIndex(this.ItemsControl.Items.Count - 1) as ContentPresenter;
            if (container == null)
            {
                return;
            }
            var listBox = container.ContentTemplate.FindName("ListBox", container) as ListBox;
            this.FixFocus(listBox);
        }

        protected virtual void FixFocus(ListBox listBox)
        {
            var index = listBox.SelectedIndex;
            if (index == -1)
            {
                index = 1;
            }
            var container = listBox.ItemContainerGenerator.ContainerFromIndex(index) as ListBoxItem;
            Keyboard.Focus(container);
        }

        protected virtual void DragSourceInitialized(object sender, ListBoxExtensions.DragSourceInitializedEventArgs e)
        {
            var viewModel = this.FindResource<global::FoxTunes.ViewModel.LibraryBrowser>("ViewModel");
            if (viewModel != null)
            {
                if (LibraryHierarchyNode.Empty.Equals(viewModel.SelectedItem))
                {
                    return;
                }
                if (viewModel.ShowCursorAdorners)
                {
                    this.MouseCursorAdorner.DataContext = viewModel.SelectedItem;
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

        protected virtual void OnIsItemVisibleChanged(object sender, ListBoxExtensions.IsItemVisibleChangedEventArgs e)
        {
            var element = e.OriginalSource as FrameworkElement;
            if (element == null)
            {
                return;
            }
            var artworkGrid = element.GetVisualChild<ArtworkGrid>();
            if (artworkGrid == null)
            {
                return;
            }
            if (e.IsItemVisible && artworkGrid.Background == null)
            {
                ArtworkGridLoader.Load(artworkGrid, ArtworkGridLoaderPriority.High);
            }
            else
            {
                ArtworkGridLoader.Cancel(artworkGrid, ArtworkGridLoaderPriority.High);
            }
        }

        protected virtual void OnItemLoaded(object sender, RoutedEventArgs e)
        {
            var element = e.OriginalSource as FrameworkElement;
            if (element == null)
            {
                return;
            }
            var artworkGrid = element.GetVisualChild<ArtworkGrid>();
            if (artworkGrid == null)
            {
                return;
            }
            if (artworkGrid.Background == null)
            {
                ArtworkGridLoader.Load(artworkGrid, ArtworkGridLoaderPriority.Low);
            }
            else
            {
                ArtworkGridLoader.Cancel(artworkGrid, ArtworkGridLoaderPriority.Low);
            }
        }
    }
}
