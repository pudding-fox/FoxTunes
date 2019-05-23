using FoxTunes.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class ArtworkGridProvider : StandardComponent
    {
        const double DPIX = 96;

        const double DPIY = 96;

        private static readonly string PREFIX = typeof(ArtworkGridProvider).Name;

        private static readonly KeyLock<string> KeyLock = new KeyLock<string>();

        public ThemeLoader ThemeLoader { get; private set; }

        public ImageLoader ImageLoader { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.ThemeLoader = ComponentRegistry.Instance.GetComponent<ThemeLoader>();
            this.ImageLoader = ComponentRegistry.Instance.GetComponent<ImageLoader>();
            base.InitializeComponent(core);
        }

        public async Task<ImageSource> CreateImageSource(LibraryHierarchyNode libraryHierarchyNode, int width, int height)
        {
            var id = this.GetImageId(libraryHierarchyNode, width, height);
            var fileName = default(string);
            if (FileMetaDataStore.Exists(PREFIX, id, out fileName))
            {
                return this.ImageLoader.Load(fileName);
            }
            using (KeyLock.Lock(id))
            {
                if (FileMetaDataStore.Exists(PREFIX, id, out fileName))
                {
                    return this.ImageLoader.Load(fileName);
                }
                return await this.CreateImageSourceCore(libraryHierarchyNode, width, height);
            }
        }

        private async Task<ImageSource> CreateImageSourceCore(LibraryHierarchyNode libraryHierarchyNode, int width, int height)
        {
            if (!libraryHierarchyNode.IsMetaDatasLoaded)
            {
                await libraryHierarchyNode.LoadMetaDatasAsync();
            }
            switch (libraryHierarchyNode.MetaDatas.Count)
            {
                case 0:
                    return this.CreateImageSource0(libraryHierarchyNode, width, height);
                case 1:
                    return this.CreateImageSource1(libraryHierarchyNode, width, height);
                case 2:
                    return this.CreateImageSource2(libraryHierarchyNode, width, height);
                case 3:
                    return this.CreateImageSource3(libraryHierarchyNode, width, height);
                default:
                    return this.CreateImageSource4(libraryHierarchyNode, width, height);
            }
        }

        private ImageSource CreateImageSource0(LibraryHierarchyNode libraryHierarchyNode, int width, int height)
        {
            var id = this.GetImageId(libraryHierarchyNode, width, height);
            using (var stream = ThemeLoader.Theme.ArtworkPlaceholder)
            {
                return this.ImageLoader.Load(PREFIX, id, stream, width, height);
            }
        }

        private ImageSource CreateImageSource1(LibraryHierarchyNode libraryHierarchyNode, int width, int height)
        {
            var id = this.GetImageId(libraryHierarchyNode, width, height);
            var fileName = libraryHierarchyNode.MetaDatas[0].Value;
            if (!File.Exists(fileName))
            {
                return this.CreateImageSource0(libraryHierarchyNode, width, height);
            }
            return this.ImageLoader.Load(PREFIX, id, fileName, width, height);
        }

        private ImageSource CreateImageSource2(LibraryHierarchyNode libraryHierarchyNode, int width, int height)
        {
            var visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
            {
                this.DrawImage(libraryHierarchyNode, context, 0, 2, width, height);
                this.DrawImage(libraryHierarchyNode, context, 1, 2, width, height);
            }
            return this.Render(libraryHierarchyNode, visual, width, height);
        }

        private ImageSource CreateImageSource3(LibraryHierarchyNode libraryHierarchyNode, int width, int height)
        {
            var visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
            {
                this.DrawImage(libraryHierarchyNode, context, 0, 3, width, height);
                this.DrawImage(libraryHierarchyNode, context, 1, 3, width, height);
                this.DrawImage(libraryHierarchyNode, context, 2, 3, width, height);
            }
            return this.Render(libraryHierarchyNode, visual, width, height);
        }

        private ImageSource CreateImageSource4(LibraryHierarchyNode libraryHierarchyNode, int width, int height)
        {
            var visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
            {
                this.DrawImage(libraryHierarchyNode, context, 0, 4, width, height);
                this.DrawImage(libraryHierarchyNode, context, 1, 4, width, height);
                this.DrawImage(libraryHierarchyNode, context, 2, 4, width, height);
                this.DrawImage(libraryHierarchyNode, context, 3, 4, width, height);
            }
            return this.Render(libraryHierarchyNode, visual, width, height);
        }

        private void DrawImage(LibraryHierarchyNode libraryHierarchyNode, DrawingContext context, int position, int count, int width, int height)
        {
            var id = this.GetImageId(libraryHierarchyNode, width, height) + position;
            var fileName = libraryHierarchyNode.MetaDatas[position].Value;
            if (!File.Exists(fileName))
            {
                return;
            }
            var region = this.GetRegion(context, position, count, width, height);
            var size = (int)Math.Max(region.Width, region.Height);
            var source = this.ImageLoader.Load(PREFIX, id, fileName, size, size);
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

        private ImageSource Render(LibraryHierarchyNode libraryHierarchyNode, DrawingVisual visual, int width, int height)
        {
            var target = new RenderTargetBitmap(width, height, DPIX, DPIY, PixelFormats.Pbgra32);
            target.Render(visual);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(target));
            using (var stream = new MemoryStream())
            {
                encoder.Save(stream);
                stream.Seek(0, SeekOrigin.Begin);
                FileMetaDataStore.Write(PREFIX, this.GetImageId(libraryHierarchyNode, width, height), stream);
                stream.Seek(0, SeekOrigin.Begin);
            }
            target.Freeze();
            return target;
        }

        private string GetImageId(LibraryHierarchyNode libraryHierarchyNode, int width, int height)
        {
            var hashCode = default(int);
            unchecked
            {
                do
                {
                    if (!string.IsNullOrEmpty(libraryHierarchyNode.Value))
                    {
                        hashCode += libraryHierarchyNode.Value.GetHashCode();
                    }
                    libraryHierarchyNode = libraryHierarchyNode.Parent;
                } while (libraryHierarchyNode != null);
            }
            hashCode += width.GetHashCode();
            hashCode += height.GetHashCode();
            return hashCode.ToString();
        }

        public void Clear()
        {
            try
            {
                FileMetaDataStore.Clear(PREFIX);
            }
            finally
            {
                this.OnCleared();
            }
        }

        protected virtual void OnCleared()
        {
            if (this.Cleared != null)
            {
                this.Cleared(this, EventArgs.Empty);
            }
        }

        public event EventHandler Cleared;
    }
}
