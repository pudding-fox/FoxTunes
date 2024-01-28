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
    }
}
