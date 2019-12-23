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
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class ImageLoader : StandardComponent, IConfigurableComponent
    {
        const int CACHE_SIZE = 128;

        const int TIMEOUT = 1000;

        private static readonly KeyLock<string> KeyLock = new KeyLock<string>();

        private static readonly Cache Store = new Cache(CACHE_SIZE);

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

        public ImageSource Load(string fileName)
        {
            return this.Load(null, null, fileName, 0, 0);
        }

        public ImageSource Load(string prefix, string id, string fileName, int width, int height)
        {
            var imageSource = default(Lazy<ImageSource>);
            if (Store.TryGetValue(prefix, fileName, width, height, out imageSource))
            {
                return imageSource.Value;
            }
            Store.Add(prefix, fileName, width, height, new Lazy<ImageSource>(() => this.LoadCore(prefix, id, fileName, width, height)));
            //Second iteration will always hit cache.
            return this.Load(prefix, id, fileName, width, height);
        }

        private ImageSource LoadCore(string prefix, string id, string fileName, int width, int height)
        {
            try
            {
                var decode = false;
                if (width != 0 && height != 0 && this.HighQualityResizer.Value)
                {
                    fileName = this.Resize(prefix, id, fileName, width, height);
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

        public string Resize(string prefix, string id, string fileName, int width, int height)
        {
            return this.Resize(prefix, id, () => Bitmap.FromFile(fileName), width, height);
        }

        public ImageSource Load(Stream stream)
        {
            return this.Load(null, null, stream, 0, 0);
        }

        public ImageSource Load(string prefix, string id, Stream stream, int width, int height)
        {
            try
            {
                var decode = false;
                var dispose = false;
                if (width != 0 && height != 0 && this.HighQualityResizer.Value)
                {
                    stream = this.Resize(prefix, id, stream, width, height);
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

        public Stream Resize(string prefix, string id, Stream stream, int width, int height)
        {
            return File.OpenRead(this.Resize(prefix, id, () => Bitmap.FromStream(stream), width, height));
        }

        protected virtual string Resize(string prefix, string id, Func<Image> factory, int width, int height)
        {
            var fileName = default(string);
            if (FileMetaDataStore.Exists(prefix, id, out fileName))
            {
                return fileName;
            }
            //TODO: Setting throwOnTimeout = false so we ignore synchronization timeout.
            //TODO: I think there exists a deadlock bug in KeyLock but I haven't been able to prove it.
            using (KeyLock.Lock(id, TIMEOUT, false))
            {
                if (FileMetaDataStore.Exists(prefix, id, out fileName))
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
                        return FileMetaDataStore.Write(prefix, id, stream);
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

        public class Cache
        {
            public Cache(int capacity)
            {
                this.Store = new CappedDictionary<Key, Lazy<ImageSource>>(capacity);
            }

            public CappedDictionary<Key, Lazy<ImageSource>> Store { get; private set; }

            public void Add(string prefix, string fileName, int width, int height, Lazy<ImageSource> imageSource)
            {
                var key = new Key(prefix, fileName, width, height);
                this.Store.Add(key, imageSource);
            }

            public bool TryGetValue(string prefix, string fileName, int width, int height, out Lazy<ImageSource> imageSource)
            {
                var key = new Key(prefix, fileName, width, height);
                return this.Store.TryGetValue(key, out imageSource);
            }

            public class Key : IEquatable<Key>
            {
                public Key(string prefix, string fileName, int width, int height)
                {
                    this.Prefix = prefix;
                    this.FileName = fileName;
                    this.Width = width;
                    this.Height = height;
                }

                public string Prefix { get; private set; }

                public string FileName { get; private set; }

                public int Width { get; private set; }

                public int Height { get; private set; }

                public virtual bool Equals(Key other)
                {
                    if (other == null)
                    {
                        return false;
                    }
                    if (object.ReferenceEquals(this, other))
                    {
                        return true;
                    }
                    if (!string.Equals(this.Prefix, other.Prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                    if (!string.Equals(this.FileName, other.FileName, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                    if (this.Width != other.Width)
                    {
                        return false;
                    }
                    if (this.Height != other.Height)
                    {
                        return false;
                    }
                    return true;
                }

                public override bool Equals(object obj)
                {
                    return this.Equals(obj as Key);
                }

                public override int GetHashCode()
                {
                    var hashCode = default(int);
                    unchecked
                    {
                        if (!string.IsNullOrEmpty(this.Prefix))
                        {
                            hashCode += this.Prefix.ToLower().GetHashCode();
                        }
                        if (!string.IsNullOrEmpty(this.FileName))
                        {
                            hashCode += this.FileName.ToLower().GetHashCode();
                        }
                        hashCode += this.Width.GetHashCode();
                        hashCode += this.Height.GetHashCode();
                    }
                    return hashCode;
                }

                public static bool operator ==(Key a, Key b)
                {
                    if ((object)a == null && (object)b == null)
                    {
                        return true;
                    }
                    if ((object)a == null || (object)b == null)
                    {
                        return false;
                    }
                    if (object.ReferenceEquals((object)a, (object)b))
                    {
                        return true;
                    }
                    return a.Equals(b);
                }

                public static bool operator !=(Key a, Key b)
                {
                    return !(a == b);
                }
            }
        }
    }
}
