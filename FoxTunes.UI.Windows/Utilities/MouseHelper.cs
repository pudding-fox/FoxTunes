using System.Runtime.InteropServices;

namespace FoxTunes
{
    public static class MouseHelper
    {
        public static void GetPosition(out int x, out int y)
        {
            var point = new Point();
#if VISTA
            GetPhysicalCursorPos(ref point);
#else
            GetCursorPos(ref point);
#endif
            x = point.X;
            y = point.Y;
        }

#if VISTA
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetPhysicalCursorPos(ref Point lpPoint);
#else
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetCursorPos(ref Point lpPoint);
#endif

        [StructLayout(LayoutKind.Sequential)]
        public struct Point
        {
            public int X;
            public int Y;
        }
    }
}
