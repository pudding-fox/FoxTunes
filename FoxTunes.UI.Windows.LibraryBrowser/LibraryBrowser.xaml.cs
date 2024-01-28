using System;
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
        public static readonly LibraryBrowserTileLoader LibraryBrowserTileLoader = ComponentRegistry.Instance.GetComponent<LibraryBrowserTileLoader>();

        public LibraryBrowser()
        {
            this.InitializeComponent();
        }

        public LibraryBrowserViewMode ViewMode
        {
            get
            {
                var viewModel = this.FindResource("ViewModel") as global::FoxTunes.ViewModel.LibraryBrowser;
                if (viewModel == null)
                {
                    return LibraryBrowserViewMode.Grid;
                }
                return viewModel.ViewMode;
            }
        }

        public ListBox GetActiveListBox()
        {
            var container = this.ItemsControl.ItemContainerGenerator.ContainerFromIndex(this.ItemsControl.Items.Count - 1) as ContentPresenter;
            if (container == null)
            {
                return null;
            }
            var listBox = container.ContentTemplate.FindName("ListBox", container) as ListBox;
            return listBox;
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
            this.EnableVisibilityTracking();
            this.FixFocus();
        }

        protected virtual void OnUnloaded(object sender, RoutedEventArgs e)
        {
            this.DisableVisibilityTracking();
            this.FixFocus();
        }

        protected virtual void EnableVisibilityTracking()
        {
            var listBox = this.GetActiveListBox();
            if (listBox == null)
            {
                return;
            }
            switch (this.ViewMode)
            {
                default:
                case LibraryBrowserViewMode.Grid:
                    ItemsControlExtensions.SetTrackItemVisibility(listBox, true);
                    ItemsControlExtensions.AddIsItemVisibleChangedHandler(listBox, this.OnIsItemVisibleChanged);
                    break;
                case LibraryBrowserViewMode.List:
                    ItemsControlExtensions.SetTrackItemVisibility(listBox, false);
                    ItemsControlExtensions.RemoveIsItemVisibleChangedHandler(listBox, this.OnIsItemVisibleChanged);
                    break;
            }
        }

        protected virtual void DisableVisibilityTracking()
        {
            var listBox = this.GetActiveListBox();
            if (listBox == null)
            {
                return;
            }
            ItemsControlExtensions.SetTrackItemVisibility(listBox, false);
            ItemsControlExtensions.RemoveIsItemVisibleChangedHandler(listBox, this.OnIsItemVisibleChanged);
        }

        protected virtual void FixFocus()
        {
            if (!this.ItemsControl.IsKeyboardFocusWithin)
            {
                return;
            }
            Keyboard.ClearFocus();
            var listBox = this.GetActiveListBox();
            if (listBox == null)
            {
                return;
            }
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

        protected virtual void OnIsItemVisibleChanged(object sender, ItemsControlExtensions.IsItemVisibleChangedEventArgs e)
        {
            var element = e.OriginalSource as FrameworkElement;
            if (element == null)
            {
                return;
            }
            var libraryBrowserTile = element.GetVisualChild<LibraryBrowserTile>();
            if (libraryBrowserTile == null)
            {
                return;
            }
            if (e.IsItemVisible && libraryBrowserTile.Background == null)
            {
                LibraryBrowserTileLoader.Load(libraryBrowserTile, LibraryBrowserTileLoaderPriority.High);
            }
            else
            {
                LibraryBrowserTileLoader.Cancel(libraryBrowserTile, LibraryBrowserTileLoaderPriority.High);
            }
        }

        protected virtual void OnItemLoaded_Low(object sender, RoutedEventArgs e)
        {
            var libraryBrowserTile = e.OriginalSource as LibraryBrowserTile;
            if (libraryBrowserTile == null)
            {
                return;
            }
            if (libraryBrowserTile.Background == null)
            {
                LibraryBrowserTileLoader.Load(libraryBrowserTile, LibraryBrowserTileLoaderPriority.Low);
            }
            else
            {
                LibraryBrowserTileLoader.Cancel(libraryBrowserTile, LibraryBrowserTileLoaderPriority.Low);
            }
        }

        protected virtual void OnItemLoaded_High(object sender, RoutedEventArgs e)
        {
            var libraryBrowserTile = e.OriginalSource as LibraryBrowserTile;
            if (libraryBrowserTile == null)
            {
                return;
            }
            if (libraryBrowserTile.Background == null)
            {
                LibraryBrowserTileLoader.Load(libraryBrowserTile, LibraryBrowserTileLoaderPriority.High);
            }
            else
            {
                LibraryBrowserTileLoader.Cancel(libraryBrowserTile, LibraryBrowserTileLoaderPriority.High);
            }
        }
    }
}
