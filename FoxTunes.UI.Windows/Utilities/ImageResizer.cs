using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class ImageResizer : StandardComponent
    {
        const int TIMEOUT = 1000;

        private static readonly string PREFIX = typeof(ImageResizer).Name;

        private static readonly KeyLock<string> KeyLock = new KeyLock<string>();

        public string Resize(string id, string fileName, int width, int height)
        {
            return this.Resize(id, () => Bitmap.FromFile(fileName), width, height);
        }

        public Stream Resize(string id, Stream stream, int width, int height)
        {
            return File.OpenRead(this.Resize(id, () => Bitmap.FromStream(stream), width, height));
        }

        protected virtual string Resize(string id, Func<Image> factory, int width, int height)
        {
            var fileName = default(string);
            if (FileMetaDataStore.Exists(PREFIX, id, out fileName))
            {
                return fileName;
            }
            //TODO: Setting throwOnTimeout = false so we ignore synchronization timeout.
            //TODO: I think there exists a deadlock bug in KeyLock but I haven't been able to prove it.
            using (KeyLock.Lock(id, TIMEOUT, false))
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
    }
}
