using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace FoxTunes
{
    public static class ResourceDisposer
    {
        public static void Dispose(FrameworkElement element)
        {
            var stack = new Stack<FrameworkElement>();
            stack.Push(element);
            while (stack.Count > 0)
            {
                element = stack.Pop();
                Dispose(element.Resources);
                for (int a = 0, b = VisualTreeHelper.GetChildrenCount(element); a < b; a++)
                {
                    var child = VisualTreeHelper.GetChild(element, a) as FrameworkElement;
                    if (child != null)
                    {
                        stack.Push(child);
                    }
                }
            }
        }

        private static void Dispose(ResourceDictionary resources)
        {
            foreach (var disposable in resources.Values.OfType<IDisposable>())
            {
                try
                {
                    disposable.Dispose();
                }
                catch
                {
                    //Nothing can be done.
                }
            }
        }
    }
}
