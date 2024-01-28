using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Interop;

namespace FoxTunes
{
    public class SnappingWindow : BaseComponent, IDisposable
    {
        public const int WM_NCLBUTTONDOWN = 0x00A1;

        public const int NC_CAPTION = 2;

        public const int NC_LEFT = 10;

        public const int NC_RIGHT = 11;

        public const int NC_TOP = 12;

        public const int NC_TOPLEFT = 13;

        public const int NC_TOPRIGHT = 14;

        public const int NC_BOTTOM = 15;

        public const int NC_BOTTOMLEFT = 16;

        public const int NC_BOTTOMRIGHT = 17;

        static SnappingWindow()
        {
            Instances = new List<WeakReference<SnappingWindow>>();
        }

        private static IList<WeakReference<SnappingWindow>> Instances { get; set; }

        public static IEnumerable<SnappingWindow> Active
        {
            get
            {
                lock (Instances)
                {
                    return Instances
                        .Where(instance => instance != null && instance.IsAlive)
                        .Select(instance => instance.Target)
                        .ToArray();
                }
            }
        }

        protected static void OnActiveChanged(SnappingWindow sender)
        {
            if (ActiveChanged == null)
            {
                return;
            }
            ActiveChanged(sender, EventArgs.Empty);
        }

        public static event EventHandler ActiveChanged;

        public static int X;

        public static int Y;

        public static bool IsMoving;

        public static bool IsResizing;

        public static ResizeDirection ResizeDirection;

        private SnappingWindow()
        {
            lock (Instances)
            {
                Instances.Add(new WeakReference<SnappingWindow>(this));
            }
            OnActiveChanged(this);
        }

        public SnappingWindow(IntPtr handle) : this()
        {
            this.Handle = handle;
            this.Window = GetWindow(handle);
            this.Callback = new HwndSourceHook(this.OnCallback);
        }

        public IntPtr Handle { get; private set; }

        public Window Window { get; private set; }

        public HwndSourceHook Callback { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.AddHook();
            base.InitializeComponent(core);
        }

        protected virtual void AddHook()
        {
            Logger.Write(this, LogLevel.Debug, "Adding Windows event handler.");
            var source = HwndSource.FromHwnd(this.Handle);
            if (source == null)
            {
                Logger.Write(this, LogLevel.Warn, "No such window for handle: {0}", handle);
                return;
            }
            source.AddHook(this.Callback);
        }

        protected virtual IntPtr OnCallback(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_NCLBUTTONDOWN)
            {
                var area = wParam.ToInt32();
                var low = (lParam.ToInt32() & 0xFFFF);
                var high = (lParam.ToInt32() & 0xFFFF) >> 16;
                X = Convert.ToInt32(low);
                Y = Convert.ToInt32(high);
                if (this.OnNonClientButtonDown(area))
                {
                    return new IntPtr(1);
                }
            }
            return IntPtr.Zero;
        }

        protected virtual bool OnNonClientButtonDown(int area)
        {
            switch (area)
            {
                case NC_CAPTION:
                    this.OnMove();
                    return true;
                case NC_TOPLEFT:
                    this.OnResize(ResizeDirection.Top | ResizeDirection.Left);
                    return true;
                case NC_TOP:
                    this.OnResize(ResizeDirection.Top);
                    return true;
                case NC_TOPRIGHT:
                    this.OnResize(ResizeDirection.Top | ResizeDirection.Right);
                    return true;
                case NC_RIGHT:
                    this.OnResize(ResizeDirection.Right);
                    return true;
                case NC_BOTTOMRIGHT:
                    this.OnResize(ResizeDirection.Bottom | ResizeDirection.Right);
                    return true;
                case NC_BOTTOM:
                    this.OnResize(ResizeDirection.Bottom);
                    return true;
                case NC_BOTTOMLEFT:
                    this.OnResize(ResizeDirection.Bottom | ResizeDirection.Left);
                    return true;
                case NC_LEFT:
                    this.OnResize(ResizeDirection.Left);
                    return true;
            }
            return false;
        }

        protected virtual void OnMove()
        {

        }

        protected virtual void OnResize(ResizeDirection direction)
        {

        }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.IsDisposed || !disposing)
            {
                return;
            }
            this.OnDisposing();
            this.IsDisposed = true;
        }

        protected virtual void OnDisposing()
        {
            lock (Instances)
            {
                for (var a = Instances.Count - 1; a >= 0; a--)
                {
                    var instance = Instances[a];
                    if (instance == null || !instance.IsAlive)
                    {
                        Instances.RemoveAt(a);
                    }
                    else if (object.ReferenceEquals(this, instance.Target))
                    {
                        Instances.RemoveAt(a);
                    }
                }
            }
            OnActiveChanged(this);
        }

        ~SnappingWindow()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            try
            {
                this.Dispose(true);
            }
            catch
            {
                //Nothing can be done, never throw on GC thread.
            }
        }

        public static Window GetWindow(IntPtr handle)
        {
            var windows = Application.Current.Windows;
            for (var a = 0; a < windows.Count; a++)
            {
                var window = windows[a];
                if (window.GetHandle() != handle)
                {
                    continue;
                }
                return window;
            }
            throw new InvalidOperationException(string.Format("Window for handle \"{0}\" could not be found.", handle));
        }
    }

    [Flags]
    public enum ResizeDirection : byte
    {
        None = 0,
        Left = 1,
        Right = 2,
        Top = 4,
        Bottom = 8
    }
}
