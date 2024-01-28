using FoxTunes.Interfaces;
using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FoxTunes
{
    public static class ImageLoader
    {
        public static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        public static ImageSource Load(string fileName, int decodePixelWidth, int decodePixelHeight)
        {
            try
            {
                var source = new BitmapImage();
                source.BeginInit();
                source.CacheOption = BitmapCacheOption.OnLoad;
                source.UriSource = new Uri(fileName);
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
            catch (Exception e)
            {
                Logger.Write(typeof(ImageLoader), LogLevel.Warn, "Failed to load image: {0}", e.Message);
                return null;
            }
        }

        public static ImageSource Load(Stream stream, int decodePixelWidth, int decodePixelHeight)
        {
            try
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
            catch (Exception e)
            {
                Logger.Write(typeof(ImageLoader), LogLevel.Warn, "Failed to load image: {0}", e.Message);
                return null;
            }
        }
    }
}
