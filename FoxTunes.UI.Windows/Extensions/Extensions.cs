using System;
using System.Windows;
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
    }
}
