using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace FoxTunes
{
    public static partial class TreeViewExtensions
    {
        private static readonly ConditionalWeakTable<TreeView, SelectedItemBehaviour> SelectedItemBehaviours = new ConditionalWeakTable<TreeView, SelectedItemBehaviour>();

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.RegisterAttached(
            "SelectedItem",
            typeof(object),
            typeof(TreeViewExtensions),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnSelectedItemPropertyChanged))
        );

        public static object GetSelectedItem(TreeView source)
        {
            return source.GetValue(SelectedItemProperty);
        }

        public static void SetSelectedItem(TreeView source, object value)
        {
            source.SetValue(SelectedItemProperty, value);
        }

        private static void OnSelectedItemPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var treeView = sender as TreeView;
            if (treeView == null)
            {
                return;
            }
            var behaviour = default(SelectedItemBehaviour);
            if (!SelectedItemBehaviours.TryGetValue(treeView, out behaviour))
            {
                behaviour = new SelectedItemBehaviour(treeView);
                SelectedItemBehaviours.Add(treeView, behaviour);
            }
            behaviour.SelectedItem = e.NewValue;
        }

        private class SelectedItemBehaviour : UIBehaviour
        {
            public SelectedItemBehaviour(TreeView treeView)
            {
                this.TreeView = treeView;
                this.TreeView.SelectedItemChanged += this.OnSelectedItemChanged;
            }

            public TreeView TreeView { get; private set; }

            public object SelectedItem
            {
                get
                {
                    return this.TreeView.SelectedItem;
                }
                set
                {
                    if (object.ReferenceEquals(this.TreeView.SelectedItem, value))
                    {
                        return;
                    }
                    if (value is IHierarchical hierarchical)
                    {
                        this.Select(hierarchical);
                    }
                }
            }

            protected virtual void Select(IHierarchical value)
            {
                //Construct the path to the value.
                var stack = new Stack<IHierarchical>();
                do
                {
                    if (value == null)
                    {
                        break;
                    }
                    stack.Push(value);
                    value = value.Parent;
                } while (true);
                if (stack.Count == 0)
                {
                    return;
                }
                //We have at least one value in the path.
                this.Select(stack);
            }

            protected virtual void Select(Stack<IHierarchical> stack)
            {
                var items = default(ItemsControl);
                var item = default(TreeViewItem);
                do
                {
                    var value = stack.Pop();
                    if (value == null)
                    {
                        break;
                    }
                    if (item != null)
                    {
                        item.IsExpanded = true;
                        items = item;
                    }
                    else
                    {
                        items = this.TreeView;
                    }
                    item = this.BringIntoView(items, value);
                    if (item == null)
                    {
                        return;
                    }
                } while (stack.Count > 0);
                if (item != null && !item.IsSelected)
                {
                    item.IsSelected = true;
                }
            }

            protected virtual TreeViewItem BringIntoView(ItemsControl items, object value)
            {
                var index = items.Items.IndexOf(value);
                if (index < 0)
                {
                    return null;
                }
                var item = items.ItemContainerGenerator.ContainerFromItem(value) as TreeViewItem;
                if (item != null)
                {
                    item.BringIntoView();
                }
                else
                {
                    var scrollViewer = items.FindChild<ScrollViewer>();
                    if (scrollViewer != null)
                    {
                        scrollViewer.ScrollToItemOffset<TreeViewItem>(index);
                        items.UpdateLayout();
                        item = items.ItemContainerGenerator.ContainerFromItem(value) as TreeViewItem;
                    }
                }
                return item;
            }

            protected virtual void OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
            {
                if (object.Equals(e.OldValue, e.NewValue))
                {
                    return;
                }
                SetSelectedItem(this.TreeView, this.TreeView.SelectedItem);
            }
        }
    }
}
