using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FoxTunes
{
    public abstract class LibraryBrowserBase : UIComponentBase
    {
        protected abstract ItemsControl GetItemsControl();

        protected abstract MouseCursorAdorner GetMouseCursorAdorner();

        public ListBox GetActiveListBox()
        {
            var itemsControl = this.GetItemsControl();
            if (itemsControl == null)
            {
                return null;
            }
            var container = itemsControl.ItemContainerGenerator.ContainerFromIndex(itemsControl.Items.Count - 1) as ContentPresenter;
            if (container == null)
            {
                return null;
            }
            container.ApplyTemplate();
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
            this.FixFocus();
        }

        protected virtual void OnUnloaded(object sender, RoutedEventArgs e)
        {
            this.FixFocus();
        }

        protected virtual void FixFocus()
        {
            var itemsControl = this.GetItemsControl();
            if (itemsControl == null)
            {
                return;
            }
            if (!itemsControl.IsKeyboardFocusWithin)
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
            var mouseCursorAdorner = this.GetMouseCursorAdorner();
            if (mouseCursorAdorner == null)
            {
                return;
            }
            var viewModel = this.FindResource<global::FoxTunes.ViewModel.LibraryBrowser>("ViewModel");
            if (viewModel == null)
            {
                return;
            }
            if (LibraryHierarchyNode.Empty.Equals(viewModel.SelectedItem))
            {
                return;
            }
            //Only show adorners when hosted in main window.
            if (this.IsHostedIn<MainWindow>() && viewModel.ShowCursorAdorners)
            {
                mouseCursorAdorner.Show();
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
                if (mouseCursorAdorner.IsVisible)
                {
                    mouseCursorAdorner.Hide();
                }
            }
        }
    }
}
