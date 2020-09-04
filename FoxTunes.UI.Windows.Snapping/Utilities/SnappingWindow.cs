using FoxDb;
using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Interop;

namespace FoxTunes
{
    public class SnappingWindow : BaseComponent, IDisposable
    {
        public const int WM_NCLBUTTONDOWN = 0x00A1;

        public const int WM_KEYDOWN = 0x0100;

        public const int WM_MOUSEMOVE = 0x0200;

        public const int WM_LBUTTONUP = 0x0202;

        public const int HT_CAPTION = 2;

        public const int HT_LEFT = 10;

        public const int HT_RIGHT = 11;

        public const int HT_TOP = 12;

        public const int HT_TOPLEFT = 13;

        public const int HT_TOPRIGHT = 14;

        public const int HT_BOTTOM = 15;

        public const int HT_BOTTOMLEFT = 16;

        public const int HT_BOTTOMRIGHT = 17;

        public const int VK_ESCAPE = 0x1B;

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

        private SnappingWindow()
        {
            this.StickyWindows = new Dictionary<SnappingWindow, SnapDirection>();
        }

        public SnappingWindow(IntPtr handle) : this()
        {
            this.Handle = handle;
        }

        public IntPtr Handle { get; private set; }

        public SnappingAdapter Adapter { get; private set; }

        private bool IsSticky;

        private bool IsMoving;

        private bool IsResizing;

        private ResizeDirection ResizeDirection;

        public Point MouseOrigin;

        private Rectangle PreviousBounds;

        private Dictionary<SnappingWindow, SnapDirection> StickyWindows;

        public override void InitializeComponent(ICore core)
        {
            this.Adapter = new SnappingAdapter(this.Handle);
            this.Adapter.InitializeComponent(core);
            this.SetHook(this.DefaultHook);

            core.Components.Configuration.GetElement<TextConfigurationElement>(
                WindowSnappingBehaviourConfiguration.SECTION,
                WindowSnappingBehaviourConfiguration.STICKY
            ).ConnectValue(value => this.IsSticky = WindowSnappingBehaviourConfiguration.GetIsSticky(value, this.Adapter.Id));

            lock (Instances)
            {
                Instances.Add(new WeakReference<SnappingWindow>(this));
            }
            OnActiveChanged(this);

            base.InitializeComponent(core);
        }

        protected virtual void SetHook(HwndSourceHook hook)
        {
            this.Adapter.RemoveHook(this.DefaultHook);
            this.Adapter.RemoveHook(this.MoveHook);
            this.Adapter.RemoveHook(this.ResizeHook);
            if (hook != null)
            {
                this.Adapter.AddHook(hook);
            }
        }

        protected virtual IntPtr DefaultHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_NCLBUTTONDOWN:
                    this.Adapter.Activate();
                    if (this.OnNonClientLeftButtonDown(wParam.ToInt32()))
                    {
                        handled = true;
                        if (this.IsMoving || this.IsResizing)
                        {
                            return new IntPtr(1);
                        }
                    }
                    break;
            }

            return IntPtr.Zero;
        }

        protected virtual IntPtr MoveHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (!this.Adapter.Capture)
            {
                this.Cancel();
                return IntPtr.Zero;
            }

            switch (msg)
            {
                case WM_MOUSEMOVE:
                    this.Move();
                    break;
                case WM_LBUTTONUP:
                    this.EndMove();
                    break;
                case WM_KEYDOWN:
                    if (wParam.ToInt32() == VK_ESCAPE)
                    {
                        this.Adapter.Bounds = this.PreviousBounds;
                        this.Cancel();
                    }
                    break;
            }

            return IntPtr.Zero;
        }

        protected virtual IntPtr ResizeHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (!this.Adapter.Capture)
            {
                this.Cancel();
                return IntPtr.Zero;
            }

            switch (msg)
            {
                case WM_MOUSEMOVE:
                    this.Resize();
                    break;
                case WM_LBUTTONUP:
                    this.EndResize();
                    break;
                case WM_KEYDOWN:
                    if (wParam.ToInt32() == VK_ESCAPE)
                    {
                        this.Adapter.Bounds = this.PreviousBounds;
                        this.Cancel();
                    }
                    break;
            }

            return IntPtr.Zero;
        }

        private bool OnNonClientLeftButtonDown(int area)
        {
            switch (area)
            {
                case HT_CAPTION:
                    this.StartMove();
                    return true;
                case HT_TOPLEFT:
                    this.StartResize(ResizeDirection.Top | ResizeDirection.Left);
                    return true;
                case HT_TOP:
                    this.StartResize(ResizeDirection.Top);
                    return true;
                case HT_TOPRIGHT:
                    this.StartResize(ResizeDirection.Top | ResizeDirection.Right);
                    return true;
                case HT_RIGHT:
                    this.StartResize(ResizeDirection.Right);
                    return true;
                case HT_BOTTOMRIGHT:
                    this.StartResize(ResizeDirection.Bottom | ResizeDirection.Right);
                    return true;
                case HT_BOTTOM:
                    this.StartResize(ResizeDirection.Bottom);
                    return true;
                case HT_BOTTOMLEFT:
                    this.StartResize(ResizeDirection.Bottom | ResizeDirection.Left);
                    return true;
                case HT_LEFT:
                    this.StartResize(ResizeDirection.Left);
                    return true;
            }

            return false;
        }

        private void StartMove()
        {
            if (!this.Adapter.Capture)
            {
                this.Adapter.Capture = true;
            }

            this.MouseOrigin = GetMousePosition();
            this.PreviousBounds = this.Adapter.Bounds;
            foreach (var snappingWindow in this.StickyWindows.Keys)
            {
                snappingWindow.MouseOrigin = this.MouseOrigin;
                snappingWindow.PreviousBounds = snappingWindow.Adapter.Bounds;
            }
            this.ResizeDirection = ResizeDirection.None;
            this.SetHook(this.MoveHook);
        }

        private void Move()
        {
            var mousePosition = GetMousePosition();
            var mouseOffset = new Point(
                mousePosition.X - this.MouseOrigin.X,
                mousePosition.Y - this.MouseOrigin.Y
            );
            var location = new Point(
                this.PreviousBounds.X + mouseOffset.X,
                this.PreviousBounds.Y + mouseOffset.Y
            );
            var bounds = this.Adapter.Bounds;

            bounds.Location = location;

            var direction = this.SnapToScreen(mousePosition, ref bounds, false) | this.SnapToWindows(mousePosition, ref bounds, false);

            if (this.Adapter.Bounds != bounds)
            {
                this.Adapter.Bounds = bounds;
                if (this.IsSticky)
                {
                    foreach (var pair in this.StickyWindows)
                    {
                        this.Move(pair.Key, pair.Value, mouseOffset);
                    }
                }
            }
        }

        protected virtual void Move(SnappingWindow snappingWindow, SnapDirection direction, Point mouseOffset)
        {
            var bounds = snappingWindow.Adapter.Bounds;
            var location = new Point(
                snappingWindow.PreviousBounds.X + mouseOffset.X,
                snappingWindow.PreviousBounds.Y + mouseOffset.Y
            );
            bounds.Location = location;
            if (snappingWindow.Adapter.Bounds != bounds)
            {
                snappingWindow.Adapter.Bounds = bounds;
            }
        }

        protected virtual SnapDirection SnapToScreen(Point mousePosition, ref Rectangle bounds, bool resize)
        {
            //TODO: Use only WPF frameworks.
            var screen = global::System.Windows.Forms.Screen.FromHandle(this.Handle);
            var result = SnappingHelper.Snap(ref bounds, screen.WorkingArea, true, resize);
            return result;
        }

        protected virtual SnapDirection SnapToWindows(Point mousePosition, ref Rectangle bounds, bool resize)
        {
            var direction = SnapDirection.None;
            foreach (var snappingWindow in Active)
            {
                if (object.ReferenceEquals(snappingWindow, this))
                {
                    continue;
                }
                if (this.StickyWindows.ContainsKey(snappingWindow))
                {
                    continue;
                }
                if (!snappingWindow.Adapter.IsVisible)
                {
                    continue;
                }
                direction |= SnappingHelper.Snap(ref bounds, snappingWindow.Adapter.Bounds, false, resize);
            }
            return direction;
        }

        protected virtual void EndMove()
        {
            UpdateStickyWindows();
            this.Cancel();
        }

        protected virtual void StartResize(ResizeDirection direction)
        {
            if (!this.Adapter.Capture)
            {
                this.Adapter.Capture = true;
            }

            this.MouseOrigin = GetMousePosition();
            this.PreviousBounds = this.Adapter.Bounds;
            this.ResizeDirection = direction;
            foreach (var snappingWindow in this.StickyWindows.Keys)
            {
                snappingWindow.MouseOrigin = this.MouseOrigin;
                snappingWindow.PreviousBounds = snappingWindow.Adapter.Bounds;
            }
            this.SetHook(this.ResizeHook);
        }

        protected virtual void Resize()
        {
            var mousePosition = GetMousePosition();
            var mouseOffset = new Point(
                mousePosition.X - this.MouseOrigin.X,
                mousePosition.Y - this.MouseOrigin.Y
            );
            var bounds = this.Adapter.Bounds;

            if (this.ResizeDirection.HasFlag(ResizeDirection.Left))
            {
                bounds.X = this.PreviousBounds.X + mouseOffset.X;
                bounds.Width = this.PreviousBounds.Width - mouseOffset.X;
            }
            if (this.ResizeDirection.HasFlag(ResizeDirection.Right))
            {
                bounds.Width = this.PreviousBounds.Width + mouseOffset.X;
            }
            if (this.ResizeDirection.HasFlag(ResizeDirection.Top))
            {
                bounds.Y = this.PreviousBounds.Y + mouseOffset.Y;
                bounds.Height = this.PreviousBounds.Height - mouseOffset.Y;
            }
            if (this.ResizeDirection.HasFlag(ResizeDirection.Bottom))
            {
                bounds.Height = this.PreviousBounds.Height + mouseOffset.Y;
            }

            var direction = this.SnapToScreen(mousePosition, ref bounds, true) | this.SnapToWindows(mousePosition, ref bounds, true);

            if (this.Adapter.Bounds != bounds)
            {
                this.Adapter.Bounds = bounds;
                if (this.IsSticky)
                {
                    foreach (var pair in this.StickyWindows)
                    {
                        this.Resize(pair.Key, pair.Value, mouseOffset);
                    }
                }
            }
        }

        protected virtual void Resize(SnappingWindow snappingWindow, SnapDirection direction, Point mouseOffset)
        {
            var bounds = snappingWindow.Adapter.Bounds;
            if (direction.HasFlag(SnapDirection.Left) || direction.HasFlag(SnapDirection.Right))
            {
                if (direction.HasFlag(SnapDirection.Top) && direction.HasFlag(SnapDirection.Bottom))
                {
                    if (this.ResizeDirection.HasFlag(ResizeDirection.Top))
                    {
                        bounds.Y = snappingWindow.PreviousBounds.Y + mouseOffset.Y;
                        bounds.Height = snappingWindow.PreviousBounds.Height - mouseOffset.Y;
                    }
                    if (this.ResizeDirection.HasFlag(ResizeDirection.Bottom))
                    {
                        bounds.Height = snappingWindow.PreviousBounds.Height + mouseOffset.Y;
                    }
                }
                else if (direction.HasFlag(SnapDirection.Top))
                {
                    if (this.ResizeDirection.HasFlag(ResizeDirection.Top))
                    {
                        bounds.Y = snappingWindow.PreviousBounds.Y + mouseOffset.Y;
                    }
                }
                else if (direction.HasFlag(SnapDirection.Bottom))
                {
                    if (this.ResizeDirection.HasFlag(ResizeDirection.Bottom))
                    {
                        bounds.Y = snappingWindow.PreviousBounds.Y + mouseOffset.Y;
                    }
                }
            }
            if (direction.HasFlag(SnapDirection.Top) || direction.HasFlag(SnapDirection.Bottom))
            {
                if (direction.HasFlag(SnapDirection.Left) && direction.HasFlag(SnapDirection.Right))
                {
                    if (this.ResizeDirection.HasFlag(ResizeDirection.Left))
                    {
                        bounds.X = snappingWindow.PreviousBounds.X + mouseOffset.X;
                        bounds.Width = snappingWindow.PreviousBounds.Width - mouseOffset.X;
                    }
                    if (this.ResizeDirection.HasFlag(ResizeDirection.Right))
                    {
                        bounds.Width = snappingWindow.PreviousBounds.Width + mouseOffset.X;
                    }
                }
                else if (direction.HasFlag(SnapDirection.Left))
                {
                    if (this.ResizeDirection.HasFlag(ResizeDirection.Left))
                    {
                        bounds.X = snappingWindow.PreviousBounds.X + mouseOffset.X;
                    }
                }
                else if (direction.HasFlag(SnapDirection.Right))
                {
                    if (this.ResizeDirection.HasFlag(ResizeDirection.Right))
                    {
                        bounds.X = snappingWindow.PreviousBounds.X + mouseOffset.X;
                    }
                }
            }
            if (snappingWindow.Adapter.Bounds != bounds)
            {
                snappingWindow.Adapter.Bounds = bounds;
            }
        }

        protected virtual SnapDirection ResizeToScreen(Point mousePosition, ref Rectangle bounds)
        {
            //TODO: Use only WPF frameworks.
            var screen = global::System.Windows.Forms.Screen.FromHandle(this.Handle);
            var result = SnappingHelper.Snap(ref bounds, screen.WorkingArea, true, false);
            return result;
        }

        protected virtual SnapDirection ResizeToWindows(Point mousePosition, ref Rectangle bounds)
        {
            var direction = SnapDirection.None;
            foreach (var snappingWindow in Active)
            {
                if (object.ReferenceEquals(snappingWindow, this))
                {
                    continue;
                }
                if (!snappingWindow.Adapter.IsVisible)
                {
                    continue;
                }
                direction |= SnappingHelper.Snap(ref bounds, snappingWindow.Adapter.Bounds, false, true);
            }
            return direction;
        }

        protected virtual void EndResize()
        {
            UpdateStickyWindows();
            this.Cancel();
        }

        protected virtual void Cancel()
        {
            this.Adapter.Capture = false;
            this.IsMoving = false;
            this.IsResizing = false;
            this.ResizeDirection = ResizeDirection.None;

            this.SetHook(this.DefaultHook);
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
            this.SetHook(null);
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

        public static Point GetMousePosition()
        {
            var x = default(int);
            var y = default(int);
            MouseHelper.GetPosition(out x, out y);
            return new Point(x, y);
        }

        public static void UpdateStickyWindows()
        {
            foreach (var stickyWindow in Active)
            {
                stickyWindow.StickyWindows.Clear();
                if (!stickyWindow.IsSticky || !stickyWindow.Adapter.IsVisible)
                {
                    continue;
                }
                var bounds = stickyWindow.Adapter.Bounds;
                foreach (var snappingWindow in Active)
                {
                    if (object.ReferenceEquals(stickyWindow, snappingWindow))
                    {
                        continue;
                    }
                    if (snappingWindow.IsSticky || !snappingWindow.Adapter.IsVisible)
                    {
                        //Don't stick sticky windows together or they can't be separated!
                        continue;
                    }
                    var direction = SnappingHelper.IsSnapped(bounds, snappingWindow.Adapter.Bounds);
                    if (direction == SnapDirection.None)
                    {
                        continue;
                    }
                    stickyWindow.StickyWindows[snappingWindow] = direction;
                }
            }
        }
    }
}