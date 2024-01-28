using FoxTunes.Interfaces;
using FoxTunes.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FoxTunes
{
    //Setting PRIORITY_HIGH so the the cache is cleared before being re-queried.
    [Component("AA3CF53F-5358-4AD5-A3E5-0F19B1A1F8B5", ComponentSlots.None, priority: ComponentAttribute.PRIORITY_HIGH)]
    [WindowsUserInterfaceDependency]
    public class ArtworkBrushFactory : StandardFactory, IDisposable
    {
        public PixelSizeConverter PixelSizeConverter { get; private set; }

        public ImageLoader ImageLoader { get; private set; }

        public ArtworkPlaceholderBrushFactory PlaceholderBrushFactory { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public Cache Store { get; private set; }

        public TaskFactory Factory { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.PixelSizeConverter = ComponentRegistry.Instance.GetComponent<PixelSizeConverter>();
            this.ImageLoader = ComponentRegistry.Instance.GetComponent<ImageLoader>();
            this.PlaceholderBrushFactory = ComponentRegistry.Instance.GetComponent<ArtworkPlaceholderBrushFactory>();
            this.SignalEmitter = core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<IntegerConfigurationElement>(
                ImageBehaviourConfiguration.SECTION,
                ImageLoaderConfiguration.THREADS
            ).ConnectValue(value => this.CreateTaskFactory(value));
            this.Configuration.GetElement<IntegerConfigurationElement>(
                ImageBehaviourConfiguration.SECTION,
                ImageLoaderConfiguration.CACHE_SIZE
            ).ConnectValue(value => this.CreateCache(value));
            base.InitializeComponent(core);
        }

        protected virtual Task OnSignal(object sender, ISignal signal)
        {
            switch (signal.Name)
            {
                case CommonSignals.MetaDataUpdated:
                    var names = signal.State as IEnumerable<string>;
                    this.Reset(names);
                    break;
                case CommonSignals.ImagesUpdated:
                    Logger.Write(this, LogLevel.Debug, "Images were updated, resetting cache.");
                    this.Reset();
                    break;
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public AsyncResult<ImageBrush> Create(string fileName, int width, int height)
        {
            this.PixelSizeConverter.Convert(ref width, ref height);
            var placeholder = this.PlaceholderBrushFactory.Create(width, height);
            if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
            {
                return AsyncResult<ImageBrush>.FromValue(placeholder);
            }
            var brush = default(ImageBrush);
            if (this.Store.TryGetValue(fileName, width, height, out brush))
            {
                if (brush != null)
                {
                    return AsyncResult<ImageBrush>.FromValue(brush);
                }
                else
                {
                    return AsyncResult<ImageBrush>.FromValue(placeholder);
                }
            }
            return new AsyncResult<ImageBrush>(placeholder, this.Factory.StartNew(() =>
            {
                return this.Store.GetOrAdd(fileName, width, height, () => this.Create(fileName, width, height, true));
            }));
        }

        protected virtual ImageBrush Create(string fileName, int width, int height, bool cache)
        {
            Logger.Write(this, LogLevel.Debug, "Creating brush: {0}x{1}", width, height);
            var source = ImageLoader.Load(
                fileName,
                width,
                height,
                cache
            );
            if (source == null)
            {
                return null;
            }
            var brush = new ImageBrush(source)
            {
                Stretch = Stretch.Uniform
            };
            brush.Freeze();
            return brush;
        }

        protected virtual void CreateTaskFactory(int threads)
        {
            Logger.Write(this, LogLevel.Debug, "Creating task factory for {0} threads.", threads);
            this.Factory = new TaskFactory(new TaskScheduler(new ParallelOptions()
            {
                MaxDegreeOfParallelism = threads
            }));
        }

        protected virtual void CreateCache(int capacity)
        {
            Logger.Write(this, LogLevel.Debug, "Creating cache for {0} items.", capacity);
            this.Store = new Cache(capacity);
        }

        protected virtual void Reset(IEnumerable<string> names)
        {
            if (names != null && names.Any())
            {
                if (!names.Contains(CommonImageTypes.FrontCover, StringComparer.OrdinalIgnoreCase))
                {
                    return;
                }
            }
            Logger.Write(this, LogLevel.Debug, "Meta data was updated, resetting cache.");
            this.Reset();
        }

        protected virtual void Reset()
        {
            this.Store.Clear();
        }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.IsDisposed || !disposing)
            {
                return;
            }
            this.OnDisposing();
            this.IsDisposed = true;
        }

        protected virtual void OnDisposing()
        {
            if (this.SignalEmitter != null)
            {
                this.SignalEmitter.Signal -= this.OnSignal;
            }
        }

        ~ArtworkBrushFactory()
        {
            Logger.Write(this.GetType(), LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            try
            {
                this.Dispose(true);
            }
            catch
            {
                //Nothing can be done, never throw on GC thread.
            }
        }

        public class Cache
        {
            public Cache(int capacity)
            {
                this.Store = new CappedDictionary<Key, ImageBrush>(capacity);
            }

            public CappedDictionary<Key, ImageBrush> Store { get; private set; }

            public bool TryGetValue(string fileName, int width, int height, out ImageBrush brush)
            {
                var key = new Key(fileName, width, height);
                return this.Store.TryGetValue(key, out brush);
            }

            public ImageBrush GetOrAdd(string fileName, int width, int height, Func<ImageBrush> factory)
            {
                var key = new Key(fileName, width, height);
                return this.Store.GetOrAdd(key, factory);
            }

            public void Clear()
            {
                this.Store.Clear();
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
                            hashCode += this.FileName.GetHashCode();
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
