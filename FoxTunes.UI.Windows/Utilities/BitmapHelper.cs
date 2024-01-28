using System;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FoxTunes
{
    public static class BitmapHelper
    {
        public static void Clear(RenderInfo info)
        {
            memset(info.Buffer, 0, new UIntPtr((uint)(info.Height * info.Stride)));
        }

        public static void DrawRectangle(RenderInfo info, int x, int y, int width, int height)
        {
            //Check arguments are valid.
            if (x < 0 || y < 0 || width <= 0 || height <= 0)
            {
#if DEBUG
                throw new ArgumentOutOfRangeException();
#else
                return;
#endif
            }

            if (x + width > info.Width || y + height > info.Height)
            {
#if DEBUG
                throw new ArgumentOutOfRangeException();
#else
                return;
#endif
            }

            var topLeft = IntPtr.Add(info.Buffer, (x * info.BytesPerPixel) + (y * info.Stride));

            //Set initial pixel.
            memset(IntPtr.Add(topLeft, 0), info.Blue, new UIntPtr(1));
            memset(IntPtr.Add(topLeft, 1), info.Green, new UIntPtr(1));
            memset(IntPtr.Add(topLeft, 2), info.Red, new UIntPtr(1));
            memset(IntPtr.Add(topLeft, 3), info.Alpha, new UIntPtr(1));

            //Fill first line by copying initial pixel.
            var position = 1;
            var linePosition = IntPtr.Add(topLeft, info.BytesPerPixel);
            for (position = 1; position <= width;)
            {
                //Double the number of pixels we copy until there isn't enough room.
                if (position * 2 <= width)
                {
                    memcpy(linePosition, topLeft, new UIntPtr((uint)(position * info.BytesPerPixel)));
                    linePosition = IntPtr.Add(linePosition, position * info.BytesPerPixel);
                    position *= 2;
                }
                //Fill the remainder.
                else
                {
                    var remaining = width - position;
                    memcpy(linePosition, topLeft, new UIntPtr((uint)(remaining * info.BytesPerPixel)));
                    break;
                }
            }
            if (height > 1)
            {
                //Fill each other line by copying the first line.
                var lineStart = IntPtr.Add(topLeft, info.Stride);
                for (position = 1; position < height; position++)
                {
                    memcpy(lineStart, topLeft, new UIntPtr((uint)(width * info.BytesPerPixel)));
                    lineStart = IntPtr.Add(lineStart, info.Stride);
                }
            }
        }

        public static void DrawLine(RenderInfo info, int x1, int y1, int x2, int y2)
        {
            var dx = Math.Abs(x2 - x1);
            var dy = Math.Abs(y2 - y1);

            //Check arguments are valid.
            if (x1 < 0 || y1 < 0 || x2 < 0 || y2 < 0)
            {
#if DEBUG
                throw new ArgumentOutOfRangeException();
#else
                return;
#endif
            }

            if (Math.Max(x1, x2) >= info.Width || Math.Max(y1, y2) >= info.Height)
            {
#if DEBUG
                throw new ArgumentOutOfRangeException();
#else
                return;
#endif
            }

            var source = IntPtr.Add(info.Buffer, (x1 * info.BytesPerPixel) + (y1 * info.Stride));

            //Set initial pixel.
            memset(IntPtr.Add(source, 0), info.Blue, new UIntPtr(1));
            memset(IntPtr.Add(source, 1), info.Green, new UIntPtr(1));
            memset(IntPtr.Add(source, 2), info.Red, new UIntPtr(1));
            memset(IntPtr.Add(source, 3), info.Alpha, new UIntPtr(1));

            //This code influenced by https://rosettacode.org/wiki/Bitmap/Bresenham's_line_algorithm
            var sx = default(int);
            if (x1 == x2)
            {
                sx = 0;
            }
            else if (x1 < x2)
            {
                sx = 1;
            }
            else
            {
                sx = -1;
            }

            var sy = default(int);
            if (y1 == y2)
            {
                sy = 0;
            }
            else if (y1 < y2)
            {
                sy = 1;
            }
            else
            {
                sy = -1;
            }

            var err = (dx > dy ? dx : -dy) / 2;

            while (x1 != x2 || y1 != y2)
            {
                var e2 = err;
                if (e2 > -dx)
                {
                    err -= dy;
                    x1 += sx;
                }
                if (e2 < dy)
                {
                    err += dx;
                    y1 += sy;
                }

                var destination = IntPtr.Add(info.Buffer, ((int)x1 * info.BytesPerPixel) + ((int)y1 * info.Stride));
                memcpy(destination, source, new UIntPtr((uint)info.BytesPerPixel));
            }
        }

        [DllImport("msvcrt.dll", EntryPoint = "memset", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern IntPtr memset(IntPtr destination, int value, UIntPtr count);

        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        private static extern IntPtr memcpy(IntPtr destination, IntPtr source, UIntPtr count);

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
