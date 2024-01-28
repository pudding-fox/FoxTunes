using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FoxTunes.Extensions
{
    public static partial class TreeViewExtensions
    {
        private static readonly ConcurrentDictionary<TreeView, RightButtonSelectBehaviour> RightButtonSelectBehaviours = new ConcurrentDictionary<TreeView, RightButtonSelectBehaviour>();

        public static readonly DependencyProperty RightButtonSelectProperty = DependencyProperty.RegisterAttached(
            "RightButtonSelect",
            typeof(bool),
            typeof(TreeViewExtensions),
            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnRightButtonSelectPropertyChanged))
        );

        public static bool GetRightButtonSelect(TreeView source)
        {
            return (bool)source.GetValue(RightButtonSelectProperty);
        }

        public static void SetRightButtonSelect(TreeView source, bool value)
        {
            source.SetValue(RightButtonSelectProperty, value);
        }

        private static void OnRightButtonSelectPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var treeView = sender as TreeView;
            if (treeView == null)
            {
                return;
            }
            if (GetRightButtonSelect(treeView))
            {
                RightButtonSelectBehaviours.TryAdd(treeView, new RightButtonSelectBehaviour(treeView));
            }
            else
            {
                RightButtonSelectBehaviours.TryRemove(treeView);

            }
        }

        private class RightButtonSelectBehaviour
        {
            public RightButtonSelectBehaviour(TreeView treeView)
            {
                this.TreeView = treeView;
                this.TreeView.PreviewMouseRightButtonDown += this.OnPreviewMouseRightButtonDown;
            }

            public TreeView TreeView { get; private set; }

            protected virtual void OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
            {
                var item = (e.OriginalSource as DependencyObject).FindAncestor<TreeViewItem>();
                if (item != null)
                {
                    item.Focus();
                    e.Handled = true;
                }
            }
        }
    }
}
