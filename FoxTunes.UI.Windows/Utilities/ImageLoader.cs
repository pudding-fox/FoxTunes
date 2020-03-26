using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class ImageLoader : StandardComponent, IConfigurableComponent
    {
        public ImageResizer ImageResizer { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public bool HighQualityResizer { get; private set; }

        public Cache Store { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.ImageResizer = ComponentRegistry.Instance.GetComponent<ImageResizer>();
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<BooleanConfigurationElement>(
                ImageLoaderConfiguration.SECTION,
                ImageLoaderConfiguration.HIGH_QUALITY_RESIZER
            ).ConnectValue(value => this.HighQualityResizer = value);
            this.Configuration.GetElement<IntegerConfigurationElement>(
                ImageLoaderConfiguration.SECTION,
                ImageLoaderConfiguration.CACHE_SIZE
            ).ConnectValue(value => this.Store = new Cache(value));
            base.InitializeComponent(core);
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return ImageLoaderConfiguration.GetConfigurationSections();
        }

        public ImageSource Load(string fileName, int width, int height, bool cache)
        {
            if (cache)
            {
                var imageSource = default(Lazy<ImageSource>);
                if (Store.TryGetValue(fileName, width, height, out imageSource))
                {
                    return imageSource.Value;
                }
                Store.Add(fileName, width, height, new Lazy<ImageSource>(() => this.LoadCore(fileName, width, height)));
                //Second iteration will always hit cache.
                return this.Load(fileName, width, height, cache);
            }
            return this.LoadCore(fileName, width, height);
        }

        private ImageSource LoadCore(string fileName, int width, int height)
        {
            try
            {
                var decode = false;
                if (width != 0 && height != 0 && this.HighQualityResizer)
                {
                    fileName = ImageResizer.Resize(fileName, width, height);
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

        public ImageSource Load(string id, Func<Stream> factory, bool cache)
        {
            if (cache)
            {
                var imageSource = default(Lazy<ImageSource>);
                if (Store.TryGetValue(id, 0, 0, out imageSource))
                {
                    return imageSource.Value;
                }
                Store.Add(id, 0, 0, new Lazy<ImageSource>(() => this.LoadCore(id, factory)));
                //Second iteration will always hit cache.
                return this.Load(id, factory, cache);
            }
            return this.LoadCore(id, factory);
        }

        public ImageSource LoadCore(string id, Func<Stream> factory)
        {
            try
            {
                var source = new BitmapImage();
                source.BeginInit();
                source.CacheOption = BitmapCacheOption.OnLoad;
                using (var stream = factory())
                {
                    source.StreamSource = stream;
                    source.EndInit();
                    source.Freeze();
                }
                return source;
            }
            catch (Exception e)
            {
                Logger.Write(typeof(ImageLoader), LogLevel.Warn, "Failed to load image: {0}", e.Message);
                return null;
            }
        }

        public class Cache
        {
            public Cache(int capacity)
            {
                this.Store = new CappedDictionary<Key, Lazy<ImageSource>>(capacity);
            }

            public CappedDictionary<Key, Lazy<ImageSource>> Store { get; private set; }

            public void Add(string fileName, int width, int height, Lazy<ImageSource> imageSource)
            {
                var key = new Key(fileName, width, height);
                this.Store.Add(key, imageSource);
            }

            public bool TryGetValue(string fileName, int width, int height, out Lazy<ImageSource> imageSource)
            {
                var key = new Key(fileName, width, height);
                return this.Store.TryGetValue(key, out imageSource);
            }

            public class Key : IEquatable<Key>
            {
                public Key(string fileName, int width, int height)
                {
                    this.FileName = fileName;
                    this.Width = width;
                    this.Height = height;
                }

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
