using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Reflection;
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
            private static readonly PropertyInfo ItemsHost = typeof(ItemsControl).GetProperty(
                "ItemsHost",
                BindingFlags.Instance | BindingFlags.NonPublic
             );

            private static readonly MethodInfo EnsureGenerator = typeof(Panel).GetMethod(
                "EnsureGenerator",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            private static readonly MethodInfo BringIndexIntoView = typeof(VirtualizingPanel).GetMethod(
                "BringIndexIntoView",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

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
                    this.SelectItem(value as IHierarchical);
                }
            }

            protected virtual void SelectItem(IHierarchical value)
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
                this.SelectItem(stack);
            }

            protected virtual void SelectItem(Stack<IHierarchical> stack)
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
                    //Ensure the children are loaded.
                    value.LoadChildren();
                    if (item != null)
                    {
                        //If we had a previous item then expand it.
                        item.IsExpanded = true;
                        items = item;
                    }
                    else
                    {
                        //Else it's the first iteration.
                        items = this.TreeView;
                    }
                    //Try the easy method.
                    item = items.ItemContainerGenerator.ContainerFromItem(value) as TreeViewItem;
                    if (item == null)
                    {
                        //Looks like the item hasn't been generated.
                        //Apply any templates we might need.
                        if (items.Template != null)
                        {
                            items.ApplyTemplate();
                            var presenter = items.Template.FindName("ItemsHost", items) as ItemsPresenter;
                            if (presenter != null)
                            {
                                presenter.ApplyTemplate();
                            }
                        }
                        //Update the layout and get the panel.
                        items.UpdateLayout();
                        var panel = ItemsHost.GetValue(items, null) as VirtualizingPanel;
                        if (panel != null)
                        {
                            //Enssure the ItemContainerGenerator is constructed.
                            EnsureGenerator.Invoke(panel, null);
                            //Get the index of the value.
                            var index = items.Items.IndexOf(value);
                            if (index < 0)
                            {
                                //There is no item corresponding to the current value.
                                //Nothing can be done.
                                break;
                            }
                            //Tell the panel to being the index into view.
                            //This will create the item we're looking for.
                            BringIndexIntoView.Invoke(panel, new object[] { index });
                        }
                        //Try the easy method (again).
                        item = items.ItemContainerGenerator.ContainerFromItem(value) as TreeViewItem;
                        if (item == null)
                        {
                            //Looks like the item hasn't been generated.
                            //Nothing can be done.
                            break;
                        }
                    }
                } while (stack.Count > 0);

                if (item != null)
                {
                    //Found the item, ensure it's visible and select it.
                    item.BringIntoView();
                    item.IsSelected = true;
                }
            }

            protected virtual void OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
            {
                SetSelectedItem(this.TreeView, this.TreeView.SelectedItem);
                if (GetSelectedItem(this.TreeView) != this.TreeView.SelectedItem)
                {
                    //TODO: Sometimes the value doesn't stick. Don't know why. Second attempt probably works.
                    SetSelectedItem(this.TreeView, this.TreeView.SelectedItem);
                }
            }

            protected override void OnDisposing()
            {
                this.TreeView.SelectedItemChanged -= this.OnSelectedItemChanged;
                base.OnDisposing();
            }
        }
    }
}
