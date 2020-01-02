using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;

namespace FoxTunes
{
    public static partial class Extensions
    {
        public static T FindAncestor<T>(this DependencyObject visual) where T : DependencyObject
        {
            do
            {
                if (visual is T)
                {
                    return visual as T;
                }
                visual = VisualTreeHelper.GetParent(visual);
            } while (visual != null);
            return default(T);
        }

        public static DependencyObject FindAncestor(this DependencyObject visual, Type type)
        {
            do
            {
                if (type.IsAssignableFrom(visual.GetType()))
                {
                    return visual;
                }
                visual = VisualTreeHelper.GetParent(visual);
            } while (visual != null);
            return default(DependencyObject);
        }

        public static T FindResource<T>(this FrameworkElement element, object resourceKey) where T : class
        {
            return element.FindResource(resourceKey) as T;
        }

        public static bool GetDataPresent<T>(this IDataObject dataObject, bool inferType)
        {
            foreach (var format in dataObject.GetFormats())
            {
                try
                {
                    var data = dataObject.GetData(format);
                    if (data is T)
                    {
                        return true;
                    }
                }
                catch
                {
                    //TODO: Warn.
                    continue;
                }
            }
            return false;
        }

        public static T GetData<T>(this IDataObject dataObject, bool inferType)
        {
            foreach (var format in dataObject.GetFormats())
            {
                var data = dataObject.GetData(format);
                if (data is T)
                {
                    return (T)data;
                }
            }
            return default(T);
        }

        public static bool IsEmpty(this Rect rect)
        {
            return
                (rect.Left == 0 || double.IsInfinity(rect.Left)) &&
                (rect.Top == 0 || double.IsInfinity(rect.Top)) &&
                (rect.Width == 0 || double.IsInfinity(rect.Width)) &&
                (rect.Height == 0 || double.IsInfinity(rect.Height));
        }

        public static void BringToFront(this Window window)
        {
            window.Activate();
            if (!window.Topmost)
            {
                window.Topmost = true;
                window.Topmost = false;
            }
            window.Focus();
        }

        public static IntPtr GetHandle(this Window window)
        {
            var source = new WindowInteropHelper(window);
            return source.EnsureHandle();
        }

        public static Size GetElementPixelSize(this FrameworkElement element, double width, double height)
        {
            var matrix = default(Matrix);
            var presentationSource = PresentationSource.FromVisual(element);
            if (presentationSource != null)
            {
                matrix = presentationSource.CompositionTarget.TransformToDevice;
            }
            else
            {
                using (var hwndSource = new HwndSource(new HwndSourceParameters()))
                {
                    matrix = hwndSource.CompositionTarget.TransformToDevice;
                }
            }
            return (Size)matrix.Transform(new Vector(width, height));
        }

        public static void Disconnect(this FrameworkElement element)
        {
            var parent = element.Parent;
            if (parent == null)
            {
                return;
            }
            else if (parent is ContentControl)
            {
                (parent as ContentControl).Content = null;
            }
            else if (parent is Panel)
            {
                (parent as Panel).Children.Remove(element);
            }
        }

        public static T GetVisualChild<T>(this FrameworkElement parent) where T : FrameworkElement
        {
            var stack = new Stack<DependencyObject>();
            stack.Push(parent);
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (current is T)
                {
                    return current as T;
                }
                for (int a = 0, b = VisualTreeHelper.GetChildrenCount(current); a < b; a++)
                {
                    stack.Push(VisualTreeHelper.GetChild(current, a));
                }
            }
            return default(T);
        }

        public static bool IsMouseOver(this FrameworkElement element)
        {
            var x = default(int);
            var y = default(int);
            MouseHelper.GetPosition(out x, out y);
            DpiHelper.TransformPosition(ref x, ref y);
            var window = element.FindAncestor<Window>();
            var result = VisualTreeHelper.HitTest(window, window.PointFromScreen(new Point(x, y)));
            if (result == null || result.VisualHit == null)
            {
                return false;
            }
            return element.IsAncestorOf(result.VisualHit);
        }
    }
}
