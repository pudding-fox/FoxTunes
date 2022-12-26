using System;
using System.Linq;
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

        public const int COLOR_FROM_X = 1;
        public const int COLOR_FROM_Y = 2;

        [SuppressUnmanagedCodeSecurity]
        [DllImport("bitmap_utilities.dll", EntryPoint = "create_palette")]
        public static extern IntPtr CreatePalette([In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] Int32Color[] colors, int count, int flags);

        [SuppressUnmanagedCodeSecurity]
        [DllImport("bitmap_utilities.dll", EntryPoint = "destroy_palette")]
        public static extern bool DestroyPalette(ref IntPtr palette);

        [SuppressUnmanagedCodeSecurity]
        [DllImport("bitmap_utilities.dll", EntryPoint = "draw_rectangles")]
        public static extern bool DrawRectangles([In] ref RenderInfo info, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] Int32Rect[] rectangles, int count);

        [SuppressUnmanagedCodeSecurity]
        [DllImport("bitmap_utilities.dll", EntryPoint = "draw_rectangle")]
        public static extern bool DrawRectangle([In] ref RenderInfo info, int x, int y, int width, int height);

        [SuppressUnmanagedCodeSecurity]
        [DllImport("bitmap_utilities.dll", EntryPoint = "draw_lines")]
        public static extern bool DrawLines([In] ref RenderInfo info, [In, MarshalAs(UnmanagedType.LPArray)] Int32Point[,] points, int dimentions, int count);

        [SuppressUnmanagedCodeSecurity]
        [DllImport("bitmap_utilities.dll", EntryPoint = "draw_line")]
        public static extern bool DrawLine([In] ref RenderInfo info, int x1, int y1, int x2, int y2);

        [SuppressUnmanagedCodeSecurity]
        [DllImport("bitmap_utilities.dll", EntryPoint = "draw_pixels")]
        public static extern bool DrawDots([In] ref RenderInfo info, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] Int32Pixel[] pixels, int count);

        [SuppressUnmanagedCodeSecurity]
        [DllImport("bitmap_utilities.dll", EntryPoint = "draw_pixel")]
        public static extern bool DrawDot([In] ref RenderInfo info, int color, int x, int y);

        [SuppressUnmanagedCodeSecurity]
        [DllImport("bitmap_utilities.dll", EntryPoint = "shift_left")]
        public static extern bool ShiftLeft([In] ref RenderInfo info, int count);

        [SuppressUnmanagedCodeSecurity]
        [DllImport("bitmap_utilities.dll", EntryPoint = "clear")]
        public static extern bool Clear([In] ref RenderInfo info);

        public static RenderInfo CreateRenderInfo(WriteableBitmap bitmap, params Color[] colors)
        {
            return CreateRenderInfo(bitmap, 0, colors);
        }

        public static RenderInfo CreateRenderInfo(WriteableBitmap bitmap, int flags, params Color[] colors)
        {
            if (bitmap.Format != PixelFormats.Pbgra32)
            {
                throw new NotImplementedException();
            }

            return new RenderInfo
            {
                BytesPerPixel = bitmap.Format.BitsPerPixel / 8,
                Width = bitmap.PixelWidth,
                Height = bitmap.PixelHeight,
                Stride = bitmap.PixelWidth * (bitmap.Format.BitsPerPixel / 8),
                Buffer = bitmap.BackBuffer,
                Palette = CreatePalette(flags, colors.Select(
                    color => new Int32Color(color.B, color.G, color.R, color.A)
                ).ToArray())
            };
        }

        public static IntPtr CreatePalette(int flags, params Int32Color[] colors)
        {
            return CreatePalette(colors, colors.Length, flags);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RenderInfo
        {
            public int BytesPerPixel;

            public int Width;

            public int Height;

            public int Stride;

            public IntPtr Buffer;

            public IntPtr Palette;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Int32Color
    {
        public Int32Color(int blue, int green, int red, int alpha)
        {
            this.Blue = blue;
            this.Green = green;
            this.Red = red;
            this.Alpha = alpha;
        }

        public int Blue;

        public int Green;

        public int Red;

        public int Alpha;
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
    public struct Int32Pixel
    {
        public Int32Pixel(int x, int y, int color)
        {
            this.X = x;
            this.Y = y;
            this.Color = color;
        }

        public int X;

        public int Y;

        public int Color;
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
