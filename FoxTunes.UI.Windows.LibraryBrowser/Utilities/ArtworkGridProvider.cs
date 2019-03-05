using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FoxTunes
{
    public class ArtworkGridProvider
    {
        const double DPIX = 96;

        const double DPIY = 96;

        private static readonly string PREFIX = typeof(ArtworkGridProvider).Name;

        private static readonly ThemeLoader ThemeLoader = ComponentRegistry.Instance.GetComponent<ThemeLoader>();

        public ImageSource CreateImageSource(LibraryHierarchyNode libraryHierarchyNode, int decodePixelWidth, int decodePixelHeight)
        {
            var fileName = default(string);
            if (FileMetaDataStore.Exists(PREFIX, this.GetImageId(libraryHierarchyNode), out fileName))
            {
                return ImageLoader.Load(fileName, decodePixelWidth, decodePixelHeight);
            }
            return this.CreateImageSourceCore(libraryHierarchyNode, decodePixelWidth, decodePixelHeight);
        }

        private ImageSource CreateImageSourceCore(LibraryHierarchyNode libraryHierarchyNode, int decodePixelWidth, int decodePixelHeight)
        {
            switch (libraryHierarchyNode.MetaDatas.Count)
            {
                case 0:
                    return this.CreateImageSource0(libraryHierarchyNode, decodePixelWidth, decodePixelHeight);
                case 1:
                    return this.CreateImageSource1(libraryHierarchyNode, decodePixelWidth, decodePixelHeight);
                case 2:
                    return this.CreateImageSource2(libraryHierarchyNode, decodePixelWidth, decodePixelHeight);
                case 3:
                    return this.CreateImageSource3(libraryHierarchyNode, decodePixelWidth, decodePixelHeight);
                default:
                    return this.CreateImageSource4(libraryHierarchyNode, decodePixelWidth, decodePixelHeight);
            }
        }

        private ImageSource CreateImageSource0(LibraryHierarchyNode libraryHierarchyNode, int decodePixelWidth, int decodePixelHeight)
        {
            using (var stream = ThemeLoader.Theme.ArtworkPlaceholder)
            {
                return ImageLoader.Load(stream, decodePixelWidth, decodePixelHeight);
            }
        }

        private ImageSource CreateImageSource1(LibraryHierarchyNode libraryHierarchyNode, int decodePixelWidth, int decodePixelHeight)
        {
            var fileName = libraryHierarchyNode.MetaDatas[0].Value;
            if (!File.Exists(fileName))
            {
                return this.CreateImageSource0(libraryHierarchyNode, decodePixelWidth, decodePixelHeight);
            }
            return ImageLoader.Load(fileName, decodePixelWidth, decodePixelHeight);
        }

        private ImageSource CreateImageSource2(LibraryHierarchyNode libraryHierarchyNode, int decodePixelWidth, int decodePixelHeight)
        {
            var visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
            {
                this.DrawImage(libraryHierarchyNode, context, 0, 2, decodePixelWidth, decodePixelHeight);
                this.DrawImage(libraryHierarchyNode, context, 1, 2, decodePixelWidth, decodePixelHeight);
            }
            return this.Render(libraryHierarchyNode, visual, decodePixelWidth, decodePixelHeight);
        }

        private ImageSource CreateImageSource3(LibraryHierarchyNode libraryHierarchyNode, int decodePixelWidth, int decodePixelHeight)
        {
            var visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
            {
                this.DrawImage(libraryHierarchyNode, context, 0, 3, decodePixelWidth, decodePixelHeight);
                this.DrawImage(libraryHierarchyNode, context, 1, 3, decodePixelWidth, decodePixelHeight);
                this.DrawImage(libraryHierarchyNode, context, 2, 3, decodePixelWidth, decodePixelHeight);
            }
            return this.Render(libraryHierarchyNode, visual, decodePixelWidth, decodePixelHeight);
        }

        private ImageSource CreateImageSource4(LibraryHierarchyNode libraryHierarchyNode, int decodePixelWidth, int decodePixelHeight)
        {
            var visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
            {
                this.DrawImage(libraryHierarchyNode, context, 0, 4, decodePixelWidth, decodePixelHeight);
                this.DrawImage(libraryHierarchyNode, context, 1, 4, decodePixelWidth, decodePixelHeight);
                this.DrawImage(libraryHierarchyNode, context, 2, 4, decodePixelWidth, decodePixelHeight);
                this.DrawImage(libraryHierarchyNode, context, 3, 4, decodePixelWidth, decodePixelHeight);
            }
            return this.Render(libraryHierarchyNode, visual, decodePixelWidth, decodePixelHeight);
        }

        private void DrawImage(LibraryHierarchyNode libraryHierarchyNode, DrawingContext context, int position, int count, int decodePixelWidth, int decodePixelHeight)
        {
            var fileName = libraryHierarchyNode.MetaDatas[position].Value;
            if (!File.Exists(fileName))
            {
                return;
            }
            var source = ImageLoader.Load(fileName, decodePixelWidth, decodePixelHeight);
            var region = this.GetRegion(context, position, count, decodePixelWidth, decodePixelHeight);
            if (region.Width != region.Height)
            {
                source = this.CropImage(source, region, decodePixelWidth, decodePixelHeight);
            }
            context.DrawImage(source, region);
        }

        private ImageSource CropImage(ImageSource source, Rect region, int decodePixelWidth, int decodePixelHeight)
        {
            return new CroppedBitmap((BitmapSource)source, this.GetRegion(source, region, decodePixelWidth, decodePixelHeight));
        }

        private Rect GetRegion(DrawingContext context, int region, int count, int decodePixelWidth, int decodePixelHeight)
        {
            switch (count)
            {
                case 1:
                    return new Rect(0, 0, decodePixelWidth, decodePixelHeight);
                case 2:
                    switch (region)
                    {
                        case 0:
                            return new Rect(0, 0, decodePixelWidth / 2, decodePixelHeight);
                        case 1:
                            return new Rect(decodePixelWidth / 2, 0, decodePixelWidth / 2, decodePixelHeight);
                        default:
                            throw new NotImplementedException();
                    }
                case 3:
                    switch (region)
                    {
                        case 0:
                            return new Rect(0, 0, decodePixelWidth, decodePixelHeight / 2);
                        case 1:
                            return new Rect(0, decodePixelHeight / 2, decodePixelWidth / 2, decodePixelHeight / 2);
                        case 2:
                            return new Rect(decodePixelWidth / 2, decodePixelHeight / 2, decodePixelWidth / 2, decodePixelHeight / 2);
                        default:
                            throw new NotImplementedException();
                    }
                case 4:
                    switch (region)
                    {
                        case 0:
                            return new Rect(0, 0, decodePixelWidth / 2, decodePixelHeight / 2);
                        case 1:
                            return new Rect(decodePixelWidth / 2, 0, decodePixelWidth / 2, decodePixelHeight / 2);
                        case 2:
                            return new Rect(0, decodePixelHeight / 2, decodePixelWidth / 2, decodePixelHeight / 2);
                        case 3:
                            return new Rect(decodePixelWidth / 2, decodePixelHeight / 2, decodePixelWidth / 2, decodePixelHeight / 2);
                        default:
                            throw new NotImplementedException();
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        private Int32Rect GetRegion(ImageSource source, Rect region, int decodePixelWidth, int decodePixelHeight)
        {
            var scaleX = ((BitmapSource)source).PixelWidth / (double)decodePixelWidth;
            var scaleY = ((BitmapSource)source).PixelHeight / (double)decodePixelHeight;
            return new Int32Rect(
                (int)(region.X * scaleX),
                (int)(region.Y * scaleY),
                (int)(region.Width * scaleX),
                (int)(region.Height * scaleY)
            );
        }

        private ImageSource Render(LibraryHierarchyNode libraryHierarchyNode, DrawingVisual visual, int decodePixelWidth, int decodePixelHeight)
        {
            var target = new RenderTargetBitmap(decodePixelWidth, decodePixelHeight, DPIX, DPIY, PixelFormats.Pbgra32);
            target.Render(visual);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(target));
            using (var stream = new MemoryStream())
            {
                encoder.Save(stream);
                stream.Seek(0, SeekOrigin.Begin);
                FileMetaDataStore.Write(PREFIX, this.GetImageId(libraryHierarchyNode), stream);
                stream.Seek(0, SeekOrigin.Begin);
            }
            return target;
        }

        private string GetImageId(LibraryHierarchyNode libraryHierarchyNode)
        {
            var hashCode = default(int);
            do
            {
                if (!string.IsNullOrEmpty(libraryHierarchyNode.Value))
                {
                    hashCode += libraryHierarchyNode.Value.GetHashCode();
                }
                libraryHierarchyNode = libraryHierarchyNode.Parent;
            } while (libraryHierarchyNode != null);
            return hashCode.ToString();
        }

        public void Clear()
        {
            FileMetaDataStore.Clear(PREFIX);
        }
    }
}
