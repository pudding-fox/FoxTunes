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
        private static readonly ConditionalWeakTable<TreeView, EnsureSelectedItemVisibleBehaviour> EnsureSelectedItemVisibleBehaviours = new ConditionalWeakTable<TreeView, EnsureSelectedItemVisibleBehaviour>();

        public static readonly DependencyProperty EnsureSelectedItemVisibleProperty = DependencyProperty.RegisterAttached(
            "EnsureSelectedItemVisible",
            typeof(bool),
            typeof(TreeViewExtensions),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnEnsureSelectedItemVisiblePropertyChanged))
        );

        public static bool GetEnsureSelectedItemVisible(TreeView source)
        {
            return (bool)source.GetValue(EnsureSelectedItemVisibleProperty);
        }

        public static void SetEnsureSelectedItemVisible(TreeView source, bool value)
        {
            source.SetValue(EnsureSelectedItemVisibleProperty, value);
        }

        private static void OnEnsureSelectedItemVisiblePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var treeView = sender as TreeView;
            if (treeView == null)
            {
                return;
            }
            if (GetEnsureSelectedItemVisible(treeView))
            {
                var behaviour = default(EnsureSelectedItemVisibleBehaviour);
                if (!EnsureSelectedItemVisibleBehaviours.TryGetValue(treeView, out behaviour))
                {
                    EnsureSelectedItemVisibleBehaviours.Add(treeView, new EnsureSelectedItemVisibleBehaviour(treeView));
                }
            }
            else
            {
                var behaviour = default(EnsureSelectedItemVisibleBehaviour);
                if (EnsureSelectedItemVisibleBehaviours.TryGetValue(treeView, out behaviour))
                {
                    EnsureSelectedItemVisibleBehaviours.Remove(treeView);
                    behaviour.Dispose();
                }
            }
        }

        private class EnsureSelectedItemVisibleBehaviour : UIBehaviour
        {
            private static readonly PropertyInfo ItemsHost = typeof(ItemsControl).GetProperty(
                "ItemsHost",
                BindingFlags.Instance | BindingFlags.NonPublic
             );

            private static readonly MethodInfo EnsureGenerator = typeof(Panel).GetMethod(
                "EnsureGenerator",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

#if NET40

            private static readonly MethodInfo BringIndexIntoView = typeof(VirtualizingPanel).GetMethod(
                "BringIndexIntoView",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

#endif

            public EnsureSelectedItemVisibleBehaviour(TreeView treeView)
            {
                this.TreeView = treeView;
                this.TreeView.SelectedItemChanged += this.OnSelectedItemChanged;
            }

            public TreeView TreeView { get; private set; }

            protected virtual void EnsureVisible(IHierarchical value)
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
                this.EnsureVisible(stack);
            }

            protected virtual void EnsureVisible(Stack<IHierarchical> stack)
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
#if NET40
                            BringIndexIntoView.Invoke(panel, new object[] { index });
#else
                            panel.BringIndexIntoViewPublic(index);
#endif
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
                    //Found the item, ensure it's visible.
                    item.BringIntoView();
                }
                else
                {

                }
            }

            protected virtual void OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
            {
                if (object.Equals(e.OldValue, e.NewValue))
                {
                    return;
                }
                this.EnsureVisible(this.TreeView.SelectedItem as IHierarchical);
            }
        }
    }
}
