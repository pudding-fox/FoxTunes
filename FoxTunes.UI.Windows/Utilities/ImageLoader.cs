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
            var source = new BitmapImage();
            source.BeginInit();
            source.CacheOption = BitmapCacheOption.None;
            source.UriCachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
            source.UriSource = new Uri(fileName);
            source.DecodePixelWidth = decodePixelWidth;
            source.DecodePixelHeight = decodePixelHeight;
            source.EndInit();
            source.Freeze();
            return source;
        }

        public static ImageSource Load(Stream stream, int decodePixelWidth, int decodePixelHeight)
        {
            var source = new BitmapImage();
            source.BeginInit();
            source.CacheOption = BitmapCacheOption.None;
            source.StreamSource = stream;
            source.DecodePixelWidth = decodePixelWidth;
            source.DecodePixelHeight = decodePixelHeight;
            source.EndInit();
            source.Freeze();
            return source;
        }
    }
}
