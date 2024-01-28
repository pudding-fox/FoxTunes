using System.Windows;
using System.Windows.Media;

namespace FoxTunes.Extensions
{
    public static partial class DependencyObjectExtensions
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

        public static T GetData<T>(this IDataObject dataObject) where T : class
        {
            return dataObject.GetData(typeof(T)) as T;
        }

        public static bool TryGetData<T>(this IDataObject dataObject, out T data) where T : class
        {
            if (!dataObject.GetDataPresent(typeof(T)))
            {
                data = default(T);
                return false;
            }
            data = dataObject.GetData<T>();
            return true;
        }
    }
}
