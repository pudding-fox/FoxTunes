using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace FoxTunes
{
    public static class WindowsImaging
    {
        public const int ILC_COLOR32 = 0x00000020;

        public static Bitmap Resize(Bitmap sourceImage, int width, int height, bool scale)
        {
            var rectangle = default(Rectangle);
            if (scale)
            {
                Scale(sourceImage.Width, sourceImage.Height, width, height, out rectangle);
            }
            else
            {
                rectangle = new Rectangle(0, 0, width, height);
            }
            var resultImage = new Bitmap(width, height);
            using (var graphics = Graphics.FromImage(resultImage))
            {
                graphics.InterpolationMode = InterpolationMode.High;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.DrawImage(sourceImage, rectangle);
            }
            return resultImage;
        }

        public static void Scale(int sourceWidth, int sourceHeight, int targetWidth, int targetHeight, out Rectangle rectangle)
        {
            var ratioX = Convert.ToSingle(targetWidth) / sourceWidth;
            var ratioY = Convert.ToSingle(targetHeight) / sourceHeight;
            var ratio = Math.Min(ratioX, ratioY);
            var actualWidth = Convert.ToInt32(sourceWidth * ratio);
            var actualHeight = Convert.ToInt32(sourceHeight * ratio);
            var x = (targetWidth / 2) - (actualWidth / 2);
            var y = (targetHeight / 2) - (actualHeight / 2);
            rectangle = new Rectangle(x, y, actualWidth, actualHeight);
        }

        public static bool CreateDIBSection(Bitmap bitmap, int width, int height, out IntPtr bitmapSection)
        {
            var bitmapBits = default(IntPtr);
            var bitmapInfo = new BITMAPINFO()
            {
                biSize = 40,
                biBitCount = 32,
                biPlanes = 1,
                biWidth = width,
                biHeight = height
            };
            bitmapSection = CreateDIBSection(
                IntPtr.Zero,
                bitmapInfo,
                0,
                out bitmapBits,
                IntPtr.Zero,
                0
            );
            if (IntPtr.Zero.Equals(bitmapSection))
            {
                return false;
            }
            var bitmapData = bitmap.LockBits(
                new Rectangle(
                    0,
                    0,
                    bitmap.Width,
                    bitmap.Height
                ),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb
            );
            var result = RtlMoveMemory(
                bitmapBits,
                bitmapData.Scan0,
                bitmap.Height * bitmapData.Stride
            );
            if (!result)
            {
                return false;
            }
            bitmap.UnlockBits(bitmapData);
            return true;
        }

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateDIBSection(IntPtr hdc, [In, MarshalAs(UnmanagedType.LPStruct)] BITMAPINFO pbmi, uint iUsage, out IntPtr ppvBits, IntPtr hSection, uint dwOffset);

        [DllImport("kernel32.dll")]
        public static extern bool RtlMoveMemory(IntPtr dest, IntPtr source, int dwcount);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject([In] IntPtr hObject);

        [StructLayout(LayoutKind.Sequential)]
        public class BITMAPINFO
        {
            public Int32 biSize;
            public Int32 biWidth;
            public Int32 biHeight;
            public Int16 biPlanes;
            public Int16 biBitCount;
            public Int32 biCompression;
            public Int32 biSizeImage;
            public Int32 biXPelsPerMeter;
            public Int32 biYPelsPerMeter;
            public Int32 biClrUsed;
            public Int32 biClrImportant;
            public Int32 colors;
        };
    }
}
