using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using DrawingPoint = global::System.Drawing.Point;
using DrawingSize = global::System.Drawing.Size;
using WindowsPoint = global::System.Windows.Point;
using WindowsSize = global::System.Windows.Size;

namespace FoxTunes
{
    public static class PointConverter
    {
        public static DrawingPoint ToDrawingPoint(WindowsPoint point)
        {
            return new DrawingPoint(
                Convert.ToInt32(point.X),
                Convert.ToInt32(point.Y)
            );
        }

        public static DrawingSize ToDrawingSize(WindowsPoint point)
        {
            return new DrawingSize(
                Convert.ToInt32(point.X),
                Convert.ToInt32(point.Y)
            );
        }

        public static WindowsPoint ToWindowsPoint(DrawingPoint point)
        {
            return new WindowsPoint(
                point.X,
                point.Y
            );
        }

        public static WindowsSize ToWindowsSize(DrawingPoint point)
        {
            return new WindowsSize(
                point.X,
                point.Y
            );
        }

        public static WindowsPoint PointToScreen(Visual visual, WindowsPoint point)
        {
            return visual.PointToScreen(point);
        }

        public static WindowsPoint PointFromScreen(Visual visual, WindowsPoint point)
        {
            return visual.PointFromScreen(point);
        }

        public static WindowsPoint TransformFromDevice(Visual visual, double x, double y)
        {
            var presentationSource = PresentationSource.FromVisual(visual);
            if (presentationSource != null)
            {
                var matrix = presentationSource.CompositionTarget.TransformFromDevice;
                return matrix.Transform(new WindowsPoint(x, y));
            }
            else
            {
                using (var hwndSource = new HwndSource(new HwndSourceParameters()))
                {
                    var matrix = hwndSource.CompositionTarget.TransformFromDevice;
                    return matrix.Transform(new WindowsPoint(x, y));
                }
            }
        }

        public static WindowsPoint TransformToDevice(Visual visual, double x, double y)
        {
            var presentationSource = PresentationSource.FromVisual(visual);
            if (presentationSource != null)
            {
                var matrix = presentationSource.CompositionTarget.TransformToDevice;
                return matrix.Transform(new WindowsPoint(x, y));
            }
            else
            {
                using (var hwndSource = new HwndSource(new HwndSourceParameters()))
                {
                    var matrix = hwndSource.CompositionTarget.TransformToDevice;
                    return matrix.Transform(new WindowsPoint(x, y));
                }
            }
        }
    }
}
