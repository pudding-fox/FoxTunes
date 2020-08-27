using System;
using System.Drawing;
using System.Windows.Interop;

namespace FoxTunes
{
    public class SnappingAdapter : BaseComponent
    {
        public SnappingAdapter(IntPtr handle)
        {
            this.Handle = handle;
            this.Window = GetWindow(handle);
        }

        public IntPtr Handle { get; private set; }

        public global::System.Windows.Window Window { get; private set; }

        public Rectangle Bounds
        {
            get
            {
                var position = PointConverter.TransformToDevice(
                    this.Window,
                    this.Window.Left,
                    this.Window.Top
                );
                var size = PointConverter.TransformToDevice(
                    this.Window,
                    this.Window.ActualWidth,
                    this.Window.ActualHeight
                );
                return new Rectangle(
                    PointConverter.ToDrawingPoint(position),
                    PointConverter.ToDrawingSize(size)
                );
            }
            set
            {
                var position = PointConverter.TransformFromDevice(
                    this.Window,
                    value.X,
                    value.Y
                );
                var size = PointConverter.TransformFromDevice(
                    this.Window,
                    value.Width,
                    value.Height
                );
                this.Window.Left = position.X;
                this.Window.Top = position.Y;
                this.Window.Width = size.X;
                this.Window.Height = size.Y;
            }
        }

        public bool Capture
        {
            get
            {
                return this.Window.IsMouseCaptured;
            }
            set
            {
                if (value)
                {
                    this.Window.CaptureMouse();
                }
                else
                {
                    this.Window.ReleaseMouseCapture();
                }
            }
        }

        public void AddHook(HwndSourceHook hook)
        {
            var source = HwndSource.FromHwnd(this.Handle);
            if (source == null)
            {
                return;
            }
            source.AddHook(hook);
        }

        public void RemoveHook(HwndSourceHook hook)
        {
            var source = HwndSource.FromHwnd(this.Handle);
            if (source == null)
            {
                return;
            }
            source.RemoveHook(hook);
        }

        public Point PointToScreen(Point point)
        {
            return PointConverter.ToDrawingPoint(
                PointConverter.PointToScreen(
                    this.Window,
                    PointConverter.ToWindowsPoint(
                        point
                    )
                )
            );
        }

        public void Activate()
        {
            this.Window.Activate();
        }

        public static global::System.Windows.Window GetWindow(IntPtr handle)
        {
            var windows = System.Windows.Application.Current.Windows;
            for (var a = 0; a < windows.Count; a++)
            {
                var window = windows[a];
                if (window.GetHandle() == handle)
                {
                    return window;
                }
            }
            return null;
        }
    }
}
