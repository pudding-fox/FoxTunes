using FoxTunes.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FoxTunes
{
    public class WaveFormRenderer : BaseComponent, IDisposable
    {
        public readonly object SyncRoot = new object();

        public readonly Duration LockTimeout = new Duration(TimeSpan.FromMilliseconds(1));

        public WaveFormData Data;

        public WriteableBitmap Bitmap;

        public Color Color;

        public IOutput Output;

        public IPlaybackManager PlaybackManager;

        public int Resolution;

        public bool IsStarted;

        public global::System.Timers.Timer Timer;

        public WaveFormRenderer(int resolution, int updateInterval)
        {
            this.Resolution = resolution;
            this.Timer = new global::System.Timers.Timer();
            this.Timer.Interval = updateInterval;
            this.Timer.AutoReset = false;
            this.Timer.Elapsed += this.OnElapsed;
        }

        public override void InitializeComponent(ICore core)
        {
            this.Output = core.Components.Output;
            this.PlaybackManager = core.Managers.Playback;
            this.PlaybackManager.CurrentStreamChanged += this.OnCurrentStreamChanged;
            if (this.PlaybackManager.CurrentStream != null)
            {
                this.Dispatch(() => this.Populate(this.PlaybackManager.CurrentStream));
            }
            base.InitializeComponent(core);
        }

        protected virtual void OnCurrentStreamChanged(object sender, EventArgs e)
        {
            this.Dispatch(() => this.Populate(this.PlaybackManager.CurrentStream));
        }

        protected virtual async Task Populate(IOutputStream stream)
        {
            if (this.Data.CancellationToken != null)
            {
                this.Data.CancellationToken.Cancel();
            }
            if (stream == null)
            {
                this.Stop();
                return;
            }
            else
            {
                this.Start();
            }
            stream = await this.Output.Duplicate(stream).ConfigureAwait(false);
            try
            {
                this.Data = GetData(stream, this.Resolution);
                Populate(ref this.Data);
            }
            finally
            {
                stream.Dispose();
            }
        }

        public void Start()
        {
            lock (this.SyncRoot)
            {
                if (this.Timer != null)
                {
                    this.IsStarted = true;
                    this.Timer.Start();
                }
            }
        }

        public void Stop()
        {
            lock (this.SyncRoot)
            {
                if (this.Timer != null)
                {
                    this.Timer.Stop();
                    this.IsStarted = false;
                }
            }
        }

        public void Create(int width, int height, Color color)
        {
            this.Bitmap = new WriteableBitmap(
               width,
               height,
               96,
               96,
               PixelFormats.Pbgra32,
               null
            );
            this.Color = color;
        }

        public async Task Render()
        {
            var success = default(bool);
            var info = default(BitmapHelper.RenderInfo);

            await Windows.Invoke(() =>
            {
                success = this.Bitmap.TryLock(LockTimeout);
                if (!success)
                {
                    return;
                }
                info = BitmapHelper.CreateRenderInfo(this.Bitmap, this.Color);
            });

            if (!success)
            {
                //Failed to establish lock.
                this.Start();
                return;
            }

            try
            {
                if (info.Width != this.Data.Width || info.Height != this.Data.Height)
                {
                    this.Data.Width = info.Width;
                    this.Data.Height = info.Height;
                    this.Data.ValuesPerElement = this.Data.DataCount / info.Width;
                    this.Data.ElementPosition = 0;
                    this.Data.ElementCount = 0;
                    this.Data.Elements = new Int32Rect[info.Width];
                    BitmapHelper.Clear(info);
                }
                else
                {
                    for (; this.Data.ElementPosition < this.Data.ElementCount; this.Data.ElementPosition++)
                    {
                        BitmapHelper.DrawRectangle(info, this.Data.Elements[this.Data.ElementPosition].X, this.Data.Elements[this.Data.ElementPosition].Y, this.Data.Elements[this.Data.ElementPosition].Width, this.Data.Elements[this.Data.ElementPosition].Height);
                    }
                }

                await Windows.Invoke(() =>
                {
                    this.Bitmap.AddDirtyRect(new Int32Rect(0, 0, this.Bitmap.PixelWidth, this.Bitmap.PixelHeight));
                    this.Bitmap.Unlock();
                    this.Start();
                }).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Logger.Write(this.GetType(), LogLevel.Warn, "Failed to paint wave form, disabling: {0}", e.Message);
            }
        }

        protected virtual void OnElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                Update(ref this.Data);
                if (this.Bitmap != null)
                {
                    var task = this.Render();
                }
            }
            catch (Exception exception)
            {
                Logger.Write(this.GetType(), LogLevel.Warn, "Failed to update wave form data, disabling: {0}", exception.Message);
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
            lock (this.SyncRoot)
            {
                if (this.Timer != null)
                {
                    this.Timer.Elapsed -= this.OnElapsed;
                    this.Timer.Dispose();
                    this.Timer = null;
                }
            }
        }

        ~WaveFormRenderer()
        {
            Logger.Write(typeof(SpectrumRenderer), LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            try
            {
                this.Dispose(true);
            }
            catch
            {
                //Nothing can be done, never throw on GC thread.
            }
        }

        private static void Update(ref WaveFormData data)
        {
            if (data.Elements == null)
            {
                return;
            }

            var center = data.Height / 2;

            while (data.ElementCount < data.Elements.Length)
            {
                var valuePosition = data.ElementCount * data.ValuesPerElement;
                if (valuePosition > data.DataPosition)
                {
                    break;
                }
                var x = data.ElementCount;
                var y = default(int);
                var width = 1;
                var height = default(int);

                var leftValue = default(float);
                var rightValue = default(float);
                for (var a = 0; a < data.ValuesPerElement; a++)
                {
                    leftValue += data.LeftData[valuePosition + a];
                    rightValue += data.RightData[valuePosition + a];
                }
                leftValue /= data.ValuesPerElement;
                rightValue /= data.ValuesPerElement;

                y = Convert.ToInt32(center - (leftValue * center));
                height = Convert.ToInt32((center - y) + (rightValue * center));

                data.Elements[data.ElementCount].X = x;
                data.Elements[data.ElementCount].Y = y;
                data.Elements[data.ElementCount].Width = width;
                data.Elements[data.ElementCount].Height = height;

#if DEBUG
                //Check arguments are valid.

                if (x < 0 || y < 0)
                {
                    throw new ArgumentOutOfRangeException();
                }

                if (x + width > data.Width || y + height > data.Height)
                {
                    throw new ArgumentOutOfRangeException();
                }
#endif

                data.ElementCount++;
            }
        }

        private static void Populate(ref WaveFormData data)
        {
            var duration = TimeSpan.FromMilliseconds(data.Resolution);
            var buffer = default(float[]);
            do
            {
#if DEBUG
                if (data.DataPosition >= data.DataCount)
                {
                    //TODO: Why?
                    break;
                }
#endif

                var length = data.Stream.GetData(ref buffer, duration);
                if (length <= 0)
                {
                    break;
                }
                var leftValue = default(float);
                var rightValue = default(float);
                for (var a = 0; a < length; a += 2)
                {
                    leftValue = Math.Max(leftValue, buffer[a]);
                    rightValue = Math.Max(rightValue, buffer[a + 1]);
                }
                data.LeftData[data.DataPosition] = leftValue;
                data.RightData[data.DataPosition] = rightValue;
                data.DataPosition++;
            } while (!data.CancellationToken.IsCancellationRequested);
        }

        private static WaveFormData GetData(IOutputStream stream, int resolution)
        {
            var length = Convert.ToInt32(Math.Ceiling(
                stream.GetDuration(stream.Length).TotalMilliseconds / resolution
            ));
            return new WaveFormData()
            {
                Width = 0,
                Height = 0,
                Resolution = resolution,
                Stream = stream,
                ValuesPerElement = 0,
                Elements = null,
                ElementPosition = 0,
                ElementCount = 0,
                LeftData = new float[length],
                RightData = new float[length],
                DataPosition = 0,
                DataCount = length,
                CancellationToken = new CancellationToken()
            };
        }

        public struct WaveFormData
        {
            public int Width;

            public int Height;

            public int Resolution;

            public IOutputStream Stream;

            public int ValuesPerElement;

            public Int32Rect[] Elements;

            public int ElementPosition;

            public int ElementCount;

            public float[] LeftData;

            public float[] RightData;

            public int DataPosition;

            public int DataCount;

            public CancellationToken CancellationToken;
        }
    }
}
