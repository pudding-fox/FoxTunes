using System;
using System.Collections.Concurrent;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FoxTunes
{
    public static partial class ListBoxExtensions
    {
        private static readonly ConcurrentDictionary<ListBox, DragSourceBehaviour> DragSourceBehaviours = new ConcurrentDictionary<ListBox, DragSourceBehaviour>();

        public static readonly DependencyProperty DragSourceProperty = DependencyProperty.RegisterAttached(
            "DragSource",
            typeof(bool),
            typeof(ListBoxExtensions),
            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnDragSourcePropertyChanged))
        );

        public static bool GetDragSource(ListBox source)
        {
            return (bool)source.GetValue(DragSourceProperty);
        }

        public static void SetDragSource(ListBox source, bool value)
        {
            source.SetValue(DragSourceProperty, value);
        }

        private static void OnDragSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox == null)
            {
                return;
            }
            if (GetDragSource(listBox))
            {
                DragSourceBehaviours.TryAdd(listBox, new DragSourceBehaviour(listBox));
            }
            else
            {
                DragSourceBehaviours.TryRemove(listBox);
            }
        }

        public static readonly RoutedEvent DragSourceInitializedEvent = EventManager.RegisterRoutedEvent(
            "DragSourceInitialized",
            RoutingStrategy.Bubble,
            typeof(DragSourceInitializedEventHandler),
            typeof(ListBoxExtensions)
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
            public DragSourceBehaviour(ListBox listBox)
            {
                this.ListBox = listBox;
                this.ListBox.PreviewMouseDown += this.OnMouseDown;
                this.ListBox.MouseMove += this.OnMouseMove;
            }

            public Point DragStartPosition { get; private set; }

            public ListBox ListBox { get; private set; }

            protected virtual bool ShouldInitializeDrag(object source, Point position)
            {
                if (this.DragStartPosition.Equals(default(Point)))
                {
                    return false;
                }
                var dependencyObject = source as DependencyObject;
                if (dependencyObject == null || dependencyObject.FindAncestor<ListBoxItem>() == null)
                {
                    return false;
                }
                if (Math.Abs(position.X - this.DragStartPosition.X) > SystemParameters.MinimumHorizontalDragDistance)
                {
                    return true;
                }
                if (Math.Abs(position.Y - this.DragStartPosition.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    return true;
                }
                return false;
            }

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
                if (e.LeftButton != MouseButtonState.Pressed)
                {
                    return;
                }
                var selectedItem = this.ListBox.SelectedItem;
                if (selectedItem == null)
                {
                    return;
                }
                var position = e.GetPosition(null);
                if (this.ShouldInitializeDrag(e.OriginalSource, position))
                {
                    this.DragStartPosition = default(Point);
                    this.ListBox.RaiseEvent(new DragSourceInitializedEventArgs(DragSourceInitializedEvent, selectedItem));
                }
            }
        }
    }
}
