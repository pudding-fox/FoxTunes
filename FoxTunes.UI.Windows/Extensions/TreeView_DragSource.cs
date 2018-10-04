using System;
using System.Collections.Concurrent;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FoxTunes
{
    public static partial class TreeViewExtensions
    {
        private static readonly ConcurrentDictionary<TreeView, DragSourceBehaviour> DragSourceBehaviours = new ConcurrentDictionary<TreeView, DragSourceBehaviour>();

        public static readonly DependencyProperty DragSourceProperty = DependencyProperty.RegisterAttached(
            "DragSource",
            typeof(bool),
            typeof(TreeViewExtensions),
            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnDragSourcePropertyChanged))
        );

        public static bool GetDragSource(TreeView source)
        {
            return (bool)source.GetValue(DragSourceProperty);
        }

        public static void SetDragSource(TreeView source, bool value)
        {
            source.SetValue(DragSourceProperty, value);
        }

        private static void OnDragSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var treeView = sender as TreeView;
            if (treeView == null)
            {
                return;
            }
            if (GetDragSource(treeView))
            {
                DragSourceBehaviours.TryAdd(treeView, new DragSourceBehaviour(treeView));
            }
            else
            {
                DragSourceBehaviours.TryRemove(treeView);

            }
        }

        public static readonly RoutedEvent DragSourceInitializedEvent = EventManager.RegisterRoutedEvent(
            "DragSourceInitialized",
            RoutingStrategy.Bubble,
            typeof(DragSourceInitializedEventHandler),
            typeof(TreeViewExtensions)
        );

        public static void AddDragSourceInitializedHandler(DependencyObject source, DragSourceInitializedEventHandler handler)
        {
            var element = source as UIElement;
            if (element != null)
            {
                element.AddHandler(DragSourceInitializedEvent, handler);
            }
        }

        public static void RemoveDragSourceInitializedHandler(DependencyObject source, DragSourceInitializedEventHandler handler)
        {
            var element = source as UIElement;
            if (element != null)
            {
                element.RemoveHandler(DragSourceInitializedEvent, handler);
            }
        }

        public delegate void DragSourceInitializedEventHandler(object sender, DragSourceInitializedEventArgs e);

        public class DragSourceInitializedEventArgs : RoutedEventArgs
        {
            public DragSourceInitializedEventArgs(object data)
            {
                this.Data = data;
            }

            public DragSourceInitializedEventArgs(RoutedEvent routedEvent, object data)
                : base(routedEvent)
            {
                this.Data = data;
            }

            public DragSourceInitializedEventArgs(RoutedEvent routedEvent, object source, object data)
                : base(routedEvent, source)
            {
                this.Data = data;
            }

            public object Data { get; private set; }
        }

        private class DragSourceBehaviour
        {
            public DragSourceBehaviour(TreeView treeView)
            {
                this.TreeView = treeView;
                this.TreeView.MouseDown += this.OnMouseDown;
                this.TreeView.MouseMove += this.OnMouseMove;
            }

            public Point DragStartPosition { get; private set; }

            public bool DragInitialized { get; private set; }

            public TreeView TreeView { get; private set; }

            protected virtual void OnMouseDown(object sender, MouseButtonEventArgs e)
            {
                if (e.LeftButton != MouseButtonState.Pressed)
                {
                    return;
                }
                this.DragStartPosition = e.GetPosition(null);
            }

            protected virtual void OnMouseMove(object sender, MouseEventArgs e)
            {
                var selectedItem = GetSelectedItem(this.TreeView);
                if (e.LeftButton != MouseButtonState.Pressed || this.DragInitialized || selectedItem == null)
                {
                    return;
                }
                var position = e.GetPosition(null);
                if (Math.Abs(position.X - this.DragStartPosition.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(position.Y - this.DragStartPosition.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    this.TreeView.RaiseEvent(new DragSourceInitializedEventArgs(DragSourceInitializedEvent, selectedItem));
                }
            }
        }
    }
}
