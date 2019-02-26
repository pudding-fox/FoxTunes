using System;
using System.IO;
using System.Net.Cache;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FoxTunes
{
    public static class ImageLoader
    {
        public static ImageSource Load(string fileName, int decodePixelWidth, int decodePixelHeight)
        {
            using (var stream = File.OpenRead(fileName))
            {
                return Load(stream, decodePixelWidth, decodePixelHeight);
            }
        }

        public static ImageSource Load(Stream stream, int decodePixelWidth, int decodePixelHeight)
        {
            var source = new BitmapImage();
            source.BeginInit();
            source.CacheOption = BitmapCacheOption.OnLoad;
            source.StreamSource = stream;
            if (decodePixelWidth != 0)
            {
                source.DecodePixelWidth = decodePixelWidth;
            }
            else if (decodePixelHeight != 0)
            {
                source.DecodePixelHeight = decodePixelHeight;
            }
            source.EndInit();
            source.Freeze();
            return source;
        }
    }
}
