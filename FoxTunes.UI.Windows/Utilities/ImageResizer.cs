using FoxTunes.Interfaces;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class ImageResizer : StandardComponent, IDisposable
    {
        private static readonly string PREFIX = typeof(ImageResizer).Name;

        private static readonly KeyLock<string> KeyLock = new KeyLock<string>();

        public ISignalEmitter SignalEmitter { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.SignalEmitter = core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
            base.InitializeComponent(core);
        }

        protected virtual Task OnSignal(object sender, ISignal signal)
        {
            switch (signal.Name)
            {
                case CommonSignals.PluginInvocation:
                    switch (signal.State as string)
                    {
                        case ImageBehaviour.REFRESH_IMAGES:
                            this.Clear();
                            break;
                    }
                    break;
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public string Resize(string fileName, int width, int height)
        {
            var id = this.GetImageId(fileName, width, height);
            return this.Resize(id, () => Bitmap.FromFile(fileName), width, height);
        }

        protected virtual string Resize(string id, Func<Image> factory, int width, int height)
        {
            var fileName = default(string);
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

        private string GetImageId(string fileName, int width, int height)
        {
            var hashCode = default(int);
            unchecked
            {
                if (!string.IsNullOrEmpty(fileName))
                {
                    hashCode += fileName.GetHashCode();
                }
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

        ~ImageResizer()
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
