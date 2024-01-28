using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FoxTunes
{
    public class ImageLoader : StandardComponent, IConfigurableComponent
    {
        private static readonly string PREFIX = typeof(ImageLoader).Name;

        private static readonly KeyLock<string> KeyLock = new KeyLock<string>();

        public IConfiguration Configuration { get; private set; }

        public BooleanConfigurationElement HighQualityResizer { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Configuration = core.Components.Configuration;
            this.HighQualityResizer = this.Configuration.GetElement<BooleanConfigurationElement>(
                ImageLoaderConfiguration.SECTION,
                ImageLoaderConfiguration.HIGH_QUALITY_RESIZER
            );
            base.InitializeComponent(core);
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return ImageLoaderConfiguration.GetConfigurationSections();
        }

        public ImageSource Load(string id, string fileName, int width, int height)
        {
            try
            {
                var decode = false;
                if (width != 0 && height != 0 && this.HighQualityResizer.Value)
                {
                    fileName = this.Resize(id, fileName, width, height);
                }
                else
                {
                    decode = true;
                }
                var source = new BitmapImage();
                source.BeginInit();
                source.CacheOption = BitmapCacheOption.OnLoad;
                source.UriSource = new Uri(fileName);
                if (decode)
                {
                    if (width != 0)
                    {
                        source.DecodePixelWidth = width;
                    }
                    else if (height != 0)
                    {
                        source.DecodePixelHeight = height;
                    }
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

        protected virtual string Resize(string id, string fileName, int width, int height)
        {
            return this.Resize(id, () => Bitmap.FromFile(fileName), width, height);
        }

        public ImageSource Load(string id, Stream stream, int width, int height)
        {
            try
            {
                var decode = false;
                var dispose = false;
                if (width != 0 && height != 0 && this.HighQualityResizer.Value)
                {
                    stream = this.Resize(id, stream, width, height);
                    dispose = true;
                }
                else
                {
                    decode = true;
                }
                var source = new BitmapImage();
                source.BeginInit();
                source.CacheOption = BitmapCacheOption.OnLoad;
                source.StreamSource = stream;
                if (decode)
                {
                    if (width != 0)
                    {
                        source.DecodePixelWidth = width;
                    }
                    else if (height != 0)
                    {
                        source.DecodePixelHeight = height;
                    }
                }
                source.EndInit();
                source.Freeze();
                if (dispose)
                {
                    stream.Dispose();
                }
                return source;
            }
            catch (Exception e)
            {
                Logger.Write(typeof(ImageLoader), LogLevel.Warn, "Failed to load image: {0}", e.Message);
                return null;
            }
        }

        protected virtual Stream Resize(string id, Stream stream, int width, int height)
        {
            return File.OpenRead(this.Resize(id, () => Bitmap.FromStream(stream), width, height));
        }

        protected virtual string Resize(string id, Func<Image> factory, int width, int height)
        {
            var fileName = default(string);
            id = this.GetImageId(id, width, height);
            if (FileMetaDataStore.Exists(PREFIX, id, out fileName))
            {
                return fileName;
            }
            using (KeyLock.Lock(id))
            {
                if (FileMetaDataStore.Exists(PREFIX, id, out fileName))
                {
                    return fileName;
                }
                using (var image = new Bitmap(width, height))
                {
                    using (var graphics = Graphics.FromImage(image))
                    {
                        graphics.SmoothingMode = SmoothingMode.HighQuality;
                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        this.Resize(graphics, factory, width, height);
                    }
                    using (var stream = new MemoryStream())
                    {
                        image.Save(stream, ImageFormat.Png);
                        stream.Seek(0, SeekOrigin.Begin);
                        return FileMetaDataStore.Write(PREFIX, id, stream);
                    }
                }
            }
        }

        protected virtual void Resize(Graphics graphics, Func<Image> factory, int width, int height)
        {
            using (var image = factory())
            {
                graphics.DrawImage(image, new Rectangle(0, 0, width, height));
            }
        }

        protected virtual string GetImageId(string id, int width, int height)
        {
            var hashCode = default(int);
            unchecked
            {
                hashCode += id.GetHashCode();
                hashCode += width.GetHashCode();
                hashCode += height.GetHashCode();
            }
            return hashCode.ToString();
        }
    }
}
