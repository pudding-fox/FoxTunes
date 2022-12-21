using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FoxTunes
{
    public static class BitmapHelper
    {
        static BitmapHelper()
        {
            Loader.Load("bitmap_utilities.dll");
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport("bitmap_utilities.dll", EntryPoint = "draw_rectangles", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern bool DrawRectangles([In] ref RenderInfo info, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] Int32Rect[] rectangles, int count);

        [SuppressUnmanagedCodeSecurity]
        [DllImport("bitmap_utilities.dll", EntryPoint = "draw_rectangle", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern bool DrawRectangle([In] ref RenderInfo info, int x, int y, int width, int height);

        [SuppressUnmanagedCodeSecurity]
        [DllImport("bitmap_utilities.dll", EntryPoint = "draw_line", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern bool DrawLine([In] ref RenderInfo info, int x1, int y1, int x2, int y2);

        [SuppressUnmanagedCodeSecurity]
        [DllImport("bitmap_utilities.dll", EntryPoint = "draw_dot", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern bool DrawDot([In] ref RenderInfo info, int x, int y);

        [SuppressUnmanagedCodeSecurity]
        [DllImport("bitmap_utilities.dll", EntryPoint = "shift_left", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern bool ShiftLeft([In] ref RenderInfo info, int count);

        [SuppressUnmanagedCodeSecurity]
        [DllImport("bitmap_utilities.dll", EntryPoint = "clear", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern bool Clear([In] ref RenderInfo info);

        public static RenderInfo CreateRenderInfo(WriteableBitmap bitmap, Color color)
        {
            if (bitmap.Format != PixelFormats.Pbgra32)
            {
                throw new NotImplementedException();
            }

            return new RenderInfo()
            {
                BytesPerPixel = bitmap.Format.BitsPerPixel / 8,
                Width = bitmap.PixelWidth,
                Height = bitmap.PixelHeight,
                Stride = bitmap.PixelWidth * (bitmap.Format.BitsPerPixel / 8),
                Buffer = bitmap.BackBuffer,
                Blue = color.B,
                Green = color.G,
                Red = color.R,
                Alpha = color.A
            };
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RenderInfo
        {
            public int BytesPerPixel;

            public int Width;

            public int Height;

            public int Stride;

            public IntPtr Buffer;

            public int Blue;

            public int Green;

            public int Red;

            public int Alpha;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Int32Point
    {
        public Int32Point(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public int X;

        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Int32Rect
    {
        public Int32Rect(int x, int y, int width, int height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }

        public int X;

        public int Y;

        public int Width;

        public int Height;
    }
}
