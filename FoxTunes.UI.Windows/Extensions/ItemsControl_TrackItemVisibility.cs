using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace FoxTunes
{
    public static partial class ItemsControlExtensions
    {
        private static readonly ConditionalWeakTable<ItemsControl, TrackItemVisibilityBehaviour> TrackItemVisibilityBehaviours = new ConditionalWeakTable<ItemsControl, TrackItemVisibilityBehaviour>();

        public static readonly DependencyProperty TrackItemVisibilityProperty = DependencyProperty.RegisterAttached(
            "TrackItemVisibility",
            typeof(bool),
            typeof(ItemsControlExtensions),
            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnTrackItemVisibilityPropertyChanged))
        );

        public static bool GetTrackItemVisibility(ItemsControl source)
        {
            return (bool)source.GetValue(TrackItemVisibilityProperty);
        }

        public static void SetTrackItemVisibility(ItemsControl source, bool value)
        {
            source.SetValue(TrackItemVisibilityProperty, value);
        }

        private static void OnTrackItemVisibilityPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var itemsControl = sender as ItemsControl;
            if (itemsControl == null)
            {
                return;
            }
            if (GetTrackItemVisibility(itemsControl))
            {
                var behaviour = default(TrackItemVisibilityBehaviour);
                if (!TrackItemVisibilityBehaviours.TryGetValue(itemsControl, out behaviour))
                {
                    TrackItemVisibilityBehaviours.Add(itemsControl, new TrackItemVisibilityBehaviour(itemsControl));
                }
            }
            else
            {
                var behaviour = default(TrackItemVisibilityBehaviour);
                if (TrackItemVisibilityBehaviours.TryGetValue(itemsControl, out behaviour))
                {
                    TrackItemVisibilityBehaviours.Remove(itemsControl);
                    behaviour.Dispose();
                }
            }
        }

        public static readonly RoutedEvent IsItemVisibleChangedEvent = EventManager.RegisterRoutedEvent(
            "IsItemVisibleChanged",
            RoutingStrategy.Bubble,
            typeof(IsItemVisibleChangedEventHandler),
            typeof(ItemsControlExtensions)
        );

        public static void AddIsItemVisibleChangedHandler(DependencyObject source, IsItemVisibleChangedEventHandler handler)
        {
            var element = source as UIElement;
            if (element != null)
            {
                element.AddHandler(IsItemVisibleChangedEvent, handler);
            }
        }

        public static void RemoveIsItemVisibleChangedHandler(DependencyObject source, IsItemVisibleChangedEventHandler handler)
        {
            var element = source as UIElement;
            if (element != null)
            {
                element.RemoveHandler(IsItemVisibleChangedEvent, handler);
            }
        }

        public delegate void IsItemVisibleChangedEventHandler(object sender, IsItemVisibleChangedEventArgs e);

        public class IsItemVisibleChangedEventArgs : RoutedEventArgs
        {
            public IsItemVisibleChangedEventArgs(bool isItemVisible)
            {
                this.IsItemVisible = isItemVisible;
            }

            public IsItemVisibleChangedEventArgs(RoutedEvent routedEvent, bool isItemVisible)
                : base(routedEvent)
            {
                this.IsItemVisible = isItemVisible;
            }

            public IsItemVisibleChangedEventArgs(RoutedEvent routedEvent, object source, bool isItemVisible)
                : base(routedEvent, source)
            {
                this.IsItemVisible = isItemVisible;
            }

            public bool IsItemVisible { get; private set; }
        }

        private class TrackItemVisibilityBehaviour : UIBehaviour
        {
            private TrackItemVisibilityBehaviour()
            {
                this.Visibility = new Dictionary<FrameworkElement, bool>();
            }

            public TrackItemVisibilityBehaviour(ItemsControl itemsControl) : this()
            {
                this.ItemsControl = itemsControl;
                this.ItemsControl.AddHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(this.OnScrollChanged));
            }

            public ItemsControl ItemsControl { get; private set; }

            public IDictionary<FrameworkElement, bool> Visibility { get; private set; }

            protected virtual void OnScrollChanged(object sender, ScrollChangedEventArgs e)
            {
                var elements = new List<FrameworkElement>();
                var containerBounds = new Rect(0.0, 0.0, this.ItemsControl.ActualWidth, this.ItemsControl.ActualHeight);
                foreach (var item in this.ItemsControl.Items)
                {
                    var element = this.ItemsControl.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
                    if (element != null)
                    {
                        elements.Add(element);
                    }
                }
                foreach (var element in elements)
                {
                    var isVisible = this.GetIsVisible(element, containerBounds);
                    if (this.Visibility.GetValueOrDefault(element) != isVisible)
                    {
                        this.Visibility[element] = isVisible;
                        element.RaiseEvent(new IsItemVisibleChangedEventArgs(IsItemVisibleChangedEvent, isVisible));
                    }
                }
                foreach (var key in this.Visibility.Keys.ToArray())
                {
                    if (elements.Contains(key))
                    {
                        continue;
                    }
                    this.Visibility.Remove(key);
                }
            }

            protected virtual bool GetIsVisible(FrameworkElement element, Rect containerBounds)
            {
                if (!element.IsVisible)
                {
                    return false;
                }
                var elementBounds = element
                    .TransformToAncestor(this.ItemsControl)
                    .TransformBounds(new Rect(0.0, 0.0, element.ActualWidth, element.ActualHeight));
                return containerBounds.Contains(elementBounds.TopLeft) || containerBounds.Contains(elementBounds.BottomRight);
            }
        }
    }
}
