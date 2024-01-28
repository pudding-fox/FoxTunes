using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace FoxTunes.Extensions
{
    public static partial class VisualExtensions
    {
        private static readonly Dictionary<Visual, ImageStackBehaviour> ImageStackBehaviours = new Dictionary<Visual, ImageStackBehaviour>();

        public static readonly DependencyProperty ImageStackProperty = DependencyProperty.RegisterAttached(
            "ImageStack",
            typeof(bool),
            typeof(VisualExtensions),
            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnImageStackPropertyChanged))
        );

        public static bool GetImageStack(Visual source)
        {
            return (bool)source.GetValue(ImageStackProperty);
        }

        public static void SetImageStack(Visual source, bool value)
        {
            source.SetValue(ImageStackProperty, value);
        }

        private static void OnImageStackPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var visual = sender as Visual;
            if (visual == null)
            {
                return;
            }
            var value = GetImageStack(visual);
            if (value && !ImageStackBehaviours.ContainsKey(visual))
            {
                ImageStackBehaviours.Add(visual, new ImageStackBehaviour(visual));
            }
            else if (!value && ImageStackBehaviours.ContainsKey(visual))
            {
                ImageStackBehaviours.Remove(visual);
            }
        }

        public static readonly DependencyProperty ImageStackItemsProperty = DependencyProperty.RegisterAttached(
            "ImageStackItems",
            typeof(IList),
            typeof(VisualExtensions),
            new FrameworkPropertyMetadata(null)
        );

        public static IList GetImageStackItems(Visual source)
        {
            return (IList)source.GetValue(ImageStackItemsProperty);
        }

        public static void SetImageStackItems(Visual source, IList value)
        {
            source.SetValue(ImageStackItemsProperty, value);
        }

        public static readonly DependencyProperty ImageStackVisibleProperty = DependencyProperty.RegisterAttached(
            "ImageStackVisible",
            typeof(bool),
            typeof(VisualExtensions),
            new FrameworkPropertyMetadata(false)
        );

        public static bool GetImageStackVisible(Visual source)
        {
            return (bool)source.GetValue(ImageStackVisibleProperty);
        }

        public static void SetImageStackVisible(Visual source, bool value)
        {
            source.SetValue(ImageStackVisibleProperty, value);
        }

        private class ImageStackBehaviour
        {
            public ImageStackBehaviour(Visual visual)
            {
                this.Visual = visual;
            }

            public Visual Visual { get; private set; }
        }
    }
}
