using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace FoxTunes
{
    public static partial class ListBoxExtensions
    {
        private static readonly ConditionalWeakTable<ListBox, TrackItemVisibilityBehaviour> TrackItemVisibilityBehaviours = new ConditionalWeakTable<ListBox, TrackItemVisibilityBehaviour>();

        public static readonly DependencyProperty TrackItemVisibilityProperty = DependencyProperty.RegisterAttached(
            "TrackItemVisibility",
            typeof(bool),
            typeof(ListBoxExtensions),
            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnTrackItemVisibilityPropertyChanged))
        );

        public static bool GetTrackItemVisibility(ListBox source)
        {
            return (bool)source.GetValue(TrackItemVisibilityProperty);
        }

        public static void SetTrackItemVisibility(ListBox source, bool value)
        {
            source.SetValue(TrackItemVisibilityProperty, value);
        }

        private static void OnTrackItemVisibilityPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox == null)
            {
                return;
            }
            if (GetTrackItemVisibility(listBox))
            {
                var behaviour = default(TrackItemVisibilityBehaviour);
                if (!TrackItemVisibilityBehaviours.TryGetValue(listBox, out behaviour))
                {
                    TrackItemVisibilityBehaviours.Add(listBox, new TrackItemVisibilityBehaviour(listBox));
                }
            }
            else
            {
                TrackItemVisibilityBehaviours.Remove(listBox);
            }
        }

        public static readonly RoutedEvent IsItemVisibleChangedEvent = EventManager.RegisterRoutedEvent(
            "IsItemVisibleChanged",
            RoutingStrategy.Bubble,
            typeof(IsItemVisibleChangedEventHandler),
            typeof(ListBoxExtensions)
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

        private class TrackItemVisibilityBehaviour
        {
            private TrackItemVisibilityBehaviour()
            {
                this.Visibility = new Dictionary<FrameworkElement, bool>();
            }

            public TrackItemVisibilityBehaviour(ListBox listBox) : this()
            {
                this.ListBox = listBox;
                this.ListBox.AddHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(this.OnScrollChanged));
            }

            public ListBox ListBox { get; private set; }

            public IDictionary<FrameworkElement, bool> Visibility { get; private set; }

            protected virtual void OnScrollChanged(object sender, ScrollChangedEventArgs e)
            {
                var elements = new List<FrameworkElement>();
                var containerBounds = new Rect(0.0, 0.0, this.ListBox.ActualWidth, this.ListBox.ActualHeight);
                foreach (var item in this.ListBox.Items)
                {
                    var element = this.ListBox.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
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
                    .TransformToAncestor(this.ListBox)
                    .TransformBounds(new Rect(0.0, 0.0, element.ActualWidth, element.ActualHeight));
                return containerBounds.Contains(elementBounds.TopLeft) || containerBounds.Contains(elementBounds.BottomRight);
            }
        }
    }
}
