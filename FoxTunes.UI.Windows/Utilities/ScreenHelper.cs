using System;
using System.Drawing;
using System.Windows;

namespace FoxTunes
{
    public static class ScreenHelper
    {
        public static bool WindowBoundsVisible(Rect bounds)
        {
            var rect = new Rectangle(
                Convert.ToInt32(bounds.X),
                Convert.ToInt32(bounds.Y),
                Convert.ToInt32(bounds.Width),
                Convert.ToInt32(bounds.Height)
            );
            //TODO: Use only WPF frameworks.
            foreach (var screen in global::System.Windows.Forms.Screen.AllScreens)
            {
                if (screen.WorkingArea.IntersectsWith(rect))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
