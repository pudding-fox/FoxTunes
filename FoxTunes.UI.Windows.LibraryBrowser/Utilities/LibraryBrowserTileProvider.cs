using FoxTunes.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class LibraryBrowserTileProvider : StandardComponent, IDisposable
    {
        const double DPIX = 96;

        const double DPIY = 96;

        public static readonly string FRONT_COVER = Enum.GetName(typeof(ArtworkType), ArtworkType.FrontCover);

        private static readonly string PREFIX = typeof(LibraryBrowserTileProvider).Name;

        private static readonly KeyLock<string> KeyLock = new KeyLock<string>(StringComparer.OrdinalIgnoreCase);

        public ImageLoader ImageLoader { get; private set; }

        public ILibraryHierarchyBrowser LibraryHierarchyBrowser { get; private set; }

        public IOnDemandMetaDataProvider OnDemandMetaDataProvider { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public LibraryBrowserImageMode ImageMode { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.ImageLoader = ComponentRegistry.Instance.GetComponent<ImageLoader>();
            this.LibraryHierarchyBrowser = core.Components.LibraryHierarchyBrowser;
            this.OnDemandMetaDataProvider = core.Components.OnDemandMetaDataProvider;
            this.SignalEmitter = core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<SelectionConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                LibraryBrowserBehaviourConfiguration.LIBRARY_BROWSER_TILE_IMAGE
            ).ConnectValue(option => this.ImageMode = LibraryBrowserBehaviourConfiguration.GetLibraryImage(option));
            base.InitializeComponent(core);
        }

        protected virtual Task OnSignal(object sender, ISignal signal)
        {
            switch (signal.Name)
            {
                case CommonSignals.ImagesUpdated:
                    this.Clear();
                    break;
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public ImageSource CreateImageSource(LibraryHierarchyNode libraryHierarchyNode, int width, int height, bool cache)
        {
            try
            {
                var id = this.GetImageId(libraryHierarchyNode, width, height);
                if (cache)
                {
                    var fileName = default(string);
                    if (FileMetaDataStore.Exists(PREFIX, id, out fileName))
                    {
                        return this.ImageLoader.Load(fileName, 0, 0, true);
                    }
                }
                using (KeyLock.Lock(id))
                {
                    if (cache)
                    {
                        var fileName = default(string);
                        if (FileMetaDataStore.Exists(PREFIX, id, out fileName))
                        {
                            return this.ImageLoader.Load(fileName, 0, 0, true);
                        }
                    }
                    return this.CreateImageSourceCore(libraryHierarchyNode, width, height, cache);
                }
            }
            catch (Exception e)
            {
                Logger.Write(this, LogLevel.Error, "Error creating image source: {0}", e.Message);
                return null;
            }
        }

        private ImageSource CreateImageSourceCore(LibraryHierarchyNode libraryHierarchyNode, int width, int height, bool cache)
        {
            var fileNames = libraryHierarchyNode.MetaDatas.Where(
                metaDataItem => string.Equals(metaDataItem.Name, FRONT_COVER, StringComparison.OrdinalIgnoreCase) && metaDataItem.Type == MetaDataItemType.Image && File.Exists(metaDataItem.Value)
            ).Select(
                metaDataItem => metaDataItem.Value
            ).ToArray();
            if (!fileNames.Any())
            {
                if (this.OnDemandMetaDataProvider.IsSourceEnabled(FRONT_COVER, MetaDataItemType.Image))
                {
                    //TODO: Warning: Buffering a potentially large sequence. It might be better to run the query multiple times.
                    var libraryItems = this.LibraryHierarchyBrowser.GetItems(
                        libraryHierarchyNode,
                        true
                    ).ToArray();
                    if (libraryItems.Any())
                    {
                        //TODO: Bad .Result
                        fileNames = this.OnDemandMetaDataProvider.GetMetaData(
                            libraryItems, 
                            new OnDemandMetaDataRequest(
                                FRONT_COVER, 
                                MetaDataItemType.Image, 
                                false
                            )
                        ).Result.ToArray();
                    }
                }
            }
            switch (this.ImageMode)
            {
                case LibraryBrowserImageMode.First:
                    switch (fileNames.Length)
                    {
                        case 0:
                            return null;
                        default:
                            return this.CreateImageSource1(libraryHierarchyNode, fileNames, width, height);
                    }
                default:
                case LibraryBrowserImageMode.Compound:
                    switch (fileNames.Length)
                    {
                        case 0:
                            return null;
                        case 1:
                            return this.CreateImageSource1(libraryHierarchyNode, fileNames, width, height);
                        case 2:
                            return this.CreateImageSource2(libraryHierarchyNode, fileNames, width, height, cache);
                        case 3:
                            return this.CreateImageSource3(libraryHierarchyNode, fileNames, width, height, cache);
                        default:
                            return this.CreateImageSource4(libraryHierarchyNode, fileNames, width, height, cache);
                    }
            }
        }

        private ImageSource CreateImageSource1(LibraryHierarchyNode libraryHierarchyNode, string[] fileNames, int width, int height)
        {
            return this.ImageLoader.Load(fileNames[0], width, height, true);
        }

        private ImageSource CreateImageSource2(LibraryHierarchyNode libraryHierarchyNode, string[] fileNames, int width, int height, bool cache)
        {
            var visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
            {
                this.DrawImage(fileNames[0], context, 0, 2, width, height);
                this.DrawImage(fileNames[1], context, 1, 2, width, height);
            }
            return this.Render(libraryHierarchyNode, visual, width, height, cache);
        }

        private ImageSource CreateImageSource3(LibraryHierarchyNode libraryHierarchyNode, string[] fileNames, int width, int height, bool cache)
        {
            var visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
            {
                this.DrawImage(fileNames[0], context, 0, 3, width, height);
                this.DrawImage(fileNames[1], context, 1, 3, width, height);
                this.DrawImage(fileNames[2], context, 2, 3, width, height);
            }
            return this.Render(libraryHierarchyNode, visual, width, height, cache);
        }

        private ImageSource CreateImageSource4(LibraryHierarchyNode libraryHierarchyNode, string[] fileNames, int width, int height, bool cache)
        {
            var visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
            {
                this.DrawImage(fileNames[0], context, 0, 4, width, height);
                this.DrawImage(fileNames[1], context, 1, 4, width, height);
                this.DrawImage(fileNames[2], context, 2, 4, width, height);
                this.DrawImage(fileNames[3], context, 3, 4, width, height);
            }
            return this.Render(libraryHierarchyNode, visual, width, height, cache);
        }

        private void DrawImage(string fileName, DrawingContext context, int position, int count, int width, int height)
        {
            var region = this.GetRegion(context, position, count, width, height);
            var size = (int)Math.Max(region.Width, region.Height);
            var source = this.ImageLoader.Load(fileName, size, size, false);
            if (source == null)
            {
                //Image failed to load, nothing can be done.
                return;
            }
            if (region.Width != region.Height)
            {
                source = this.CropImage(source, region, width, height);
                source.Freeze();
            }
            context.DrawImage(source, region);
        }

        private ImageSource CropImage(ImageSource source, Rect region, int width, int height)
        {
            return new CroppedBitmap((BitmapSource)source, this.GetRegion(source, region, width, height));
        }

        private Rect GetRegion(DrawingContext context, int region, int count, int width, int height)
        {
            switch (count)
            {
                case 1:
                    return new Rect(0, 0, width, height);
                case 2:
                    switch (region)
                    {
                        case 0:
                            return new Rect(0, 0, width / 2, height);
                        case 1:
                            return new Rect(width / 2, 0, width / 2, height);
                        default:
                            throw new NotImplementedException();
                    }
                case 3:
                    switch (region)
                    {
                        case 0:
                            return new Rect(0, 0, width, height / 2);
                        case 1:
                            return new Rect(0, height / 2, width / 2, height / 2);
                        case 2:
                            return new Rect(width / 2, height / 2, width / 2, height / 2);
                        default:
                            throw new NotImplementedException();
                    }
                case 4:
                    switch (region)
                    {
                        case 0:
                            return new Rect(0, 0, width / 2, height / 2);
                        case 1:
                            return new Rect(width / 2, 0, width / 2, height / 2);
                        case 2:
                            return new Rect(0, height / 2, width / 2, height / 2);
                        case 3:
                            return new Rect(width / 2, height / 2, width / 2, height / 2);
                        default:
                            throw new NotImplementedException();
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        private Int32Rect GetRegion(ImageSource source, Rect region, int width, int height)
        {
            var scaleX = ((BitmapSource)source).PixelWidth / (double)width;
            var scaleY = ((BitmapSource)source).PixelHeight / (double)height;
            return new Int32Rect(
                (int)(region.X * scaleX),
                (int)(region.Y * scaleY),
                (int)(region.Width * scaleX),
                (int)(region.Height * scaleY)
            );
        }

        private ImageSource Render(LibraryHierarchyNode libraryHierarchyNode, DrawingVisual visual, int width, int height, bool cache)
        {
            var target = new RenderTargetBitmap(width, height, DPIX, DPIY, PixelFormats.Pbgra32);
            target.Render(visual);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(target));
            if (cache)
            {
                using (var stream = new MemoryStream())
                {
                    encoder.Save(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    FileMetaDataStore.Write(PREFIX, this.GetImageId(libraryHierarchyNode, width, height), stream);
                    stream.Seek(0, SeekOrigin.Begin);
                }
            }
            target.Freeze();
            return target;
        }

        private string GetImageId(LibraryHierarchyNode libraryHierarchyNode, int width, int height)
        {
            var hashCode = default(int);
            unchecked
            {
                hashCode = (hashCode * 29) + libraryHierarchyNode.Id.GetHashCode();
                hashCode = (hashCode * 29) + width.GetHashCode();
                hashCode = (hashCode * 29) + height.GetHashCode();
            }
            return Math.Abs(hashCode).ToString();
        }

        public void Clear()
        {
            try
            {
                FileMetaDataStore.Clear(PREFIX);
            }
            catch (Exception e)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to clear storage \"{0}\": {1}", PREFIX, e.Message);
            }
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

        ~LibraryBrowserTileProvider()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            try
            {
                this.Dispose(true);
            }
            catch
            {
                //Nothing can be done, never throw on GC thread.
            }
        }
    }
}
