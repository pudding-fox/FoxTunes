using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class WindowStateBehaviour : StandardBehaviour
    {
        const int WM_GETMINMAXINFO = 0x0024;

        public WindowStateBehaviour()
        {
            this.Hook = new HwndSourceHook(this.WindowProc);
            Windows.MainWindowCreated += this.OnMainWindowCreated;
        }

        public HwndSourceHook Hook { get; private set; }

        public IntPtr Handle { get; private set; }

        protected virtual void OnMainWindowCreated(object sender, EventArgs e)
        {
            Windows.MainWindow.Loaded += this.OnLoaded;
            Windows.MainWindow.Closed += this.OnClosed;
        }

        protected virtual void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.Enable();
        }

        protected virtual void OnClosed(object sender, EventArgs e)
        {
            this.Disable();
        }

        public void Enable()
        {
            if (Windows.MainWindow != null)
            {
                this.Handle = Windows.MainWindow.GetHandle();
                HwndSource.FromHwnd(this.Handle).AddHook(this.Hook);
            }
        }

        public void Disable()
        {
            if (this.Handle != IntPtr.Zero)
            {
                HwndSource.FromHwnd(this.Handle).RemoveHook(this.Hook);
            }
        }

        protected virtual IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_GETMINMAXINFO:
                    this.WmGetMinMaxInfo(hwnd, lParam);
                    break;
            }
            return IntPtr.Zero;
        }

        protected virtual void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
        {
            var mousePosition = default(POINT);
            GetCursorPos(out mousePosition);

            var primaryScreen = MonitorFromPoint(new POINT(0, 0), MonitorOptions.MONITOR_DEFAULTTOPRIMARY);
            var primaryScreenInfo = new MONITORINFO();
            if (GetMonitorInfo(primaryScreen, primaryScreenInfo) == false)
            {
                return;
            }

            var currentScreen = MonitorFromPoint(mousePosition, MonitorOptions.MONITOR_DEFAULTTONEAREST);
            var minMaxInfo = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

            if (primaryScreen == currentScreen)
            {
                minMaxInfo.MaxPosition.X = primaryScreenInfo.Work.Left;
                minMaxInfo.MaxPosition.Y = primaryScreenInfo.Work.Top;
                minMaxInfo.MaxSize.X = primaryScreenInfo.Work.Right - primaryScreenInfo.Work.Left;
                minMaxInfo.MaxSize.Y = primaryScreenInfo.Work.Bottom - primaryScreenInfo.Work.Top;
            }
            else
            {
                minMaxInfo.MaxPosition.X = primaryScreenInfo.Monitor.Left;
                minMaxInfo.MaxPosition.Y = primaryScreenInfo.Monitor.Top;
                minMaxInfo.MaxSize.X = primaryScreenInfo.Monitor.Right - primaryScreenInfo.Monitor.Left;
                minMaxInfo.MaxSize.Y = primaryScreenInfo.Monitor.Bottom - primaryScreenInfo.Monitor.Top;
            }

            Marshal.StructureToPtr(minMaxInfo, lParam, true);
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr MonitorFromPoint(POINT pt, MonitorOptions dwFlags);

        [DllImport("user32.dll")]
        public static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

        public enum MonitorOptions : uint
        {
            MONITOR_DEFAULTTONULL = 0x00000000,
            MONITOR_DEFAULTTOPRIMARY = 0x00000001,
            MONITOR_DEFAULTTONEAREST = 0x00000002
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT Reserved;
            public POINT MaxSize;
            public POINT MaxPosition;
            public POINT MinTrackSize;
            public POINT MaxTrackSize;
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class MONITORINFO
        {
            public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));
            public RECT Monitor = new RECT();
            public RECT Work = new RECT();
            public int Flags = 0;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
    }
}
