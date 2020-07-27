using FoxTunes.Interfaces;
using System;
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

        public WaveFormRendererMode Mode;

        public bool IsStarted;

        public global::System.Timers.Timer Timer;

        private WaveFormRenderer()
        {
            this.Data = new WaveFormData();
        }

        public WaveFormRenderer(int resolution, int updateInterval) : this()
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
            if (stream == null)
            {
                this.Stop();
                await this.Clear().ConfigureAwait(false);
                return;
            }
            else
            {
                this.Start();
            }
            stream = await this.Output.Duplicate(stream).ConfigureAwait(false);
            try
            {
                this.Data = GetData(stream, this.Resolution, this.Mode);
                Populate(this.Data);
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

        public void Create(int width, int height, Color color, WaveFormRendererMode mode)
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
            this.Mode = mode;
            //TODO: We actually need to restart and only if there's something to do.
            this.Start();
        }

        public async Task Render()
        {
            var success = default(bool);
            var info = default(BitmapHelper.RenderInfo);
            var bitmap = this.Bitmap;

            await Windows.Invoke(() =>
            {
                success = this.Bitmap.TryLock(LockTimeout);
                if (!success)
                {
                    return;
                }
                info = BitmapHelper.CreateRenderInfo(bitmap, this.Color);
            }).ConfigureAwait(false);

            if (!success)
            {
                //Failed to establish lock.
                this.Start();
                return;
            }

            try
            {
                Render(info, this.Data, this.Mode);

                await Windows.Invoke(() =>
                {
                    bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
                    bitmap.Unlock();
                    if (!this.Data.Complete)
                    {
                        this.Start();
                    }
                }).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Logger.Write(this.GetType(), LogLevel.Warn, "Failed to paint wave form, disabling: {0}", e.Message);
            }
        }

        public async Task Clear()
        {
            var success = default(bool);
            var info = default(BitmapHelper.RenderInfo);
            var bitmap = this.Bitmap;

            await Windows.Invoke(() =>
            {
                success = this.Bitmap.TryLock(LockTimeout);
                if (!success)
                {
                    return;
                }
                info = BitmapHelper.CreateRenderInfo(bitmap, this.Color);
            }).ConfigureAwait(false);

            if (!success)
            {
                //Failed to establish lock.
                return;
            }

            try
            {
                BitmapHelper.Clear(info);

                await Windows.Invoke(() =>
                {
                    bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
                    bitmap.Unlock();
                }).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Logger.Write(this.GetType(), LogLevel.Warn, "Failed to clear wave form: {0}", e.Message);
            }
        }

        protected virtual void OnElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                Update(this.Data, this.Mode);
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

        private static void Render(BitmapHelper.RenderInfo info, WaveFormData data, WaveFormRendererMode mode)
        {
            if (data.DataCount == 0)
            {
                return;
            }
            else if (info.Width != data.Width || info.Height != data.Height || data.Mode != mode)
            {
                data.Width = info.Width;
                data.Height = info.Height;
                data.ValuesPerElement = Math.Max(data.DataCount / info.Width, 1);
                data.ElementPosition = 0;
                data.ElementCount = 0;
                data.Elements = new Int32Rect[info.Width * (mode == WaveFormRendererMode.Seperate ? data.ChannelCount : 1)];
                data.Mode = mode;
                data.Complete = false;
                BitmapHelper.Clear(info);
            }
            else
            {
                for (; data.ElementPosition < data.ElementCount; data.ElementPosition++)
                {
                    BitmapHelper.DrawRectangle(info, data.Elements[data.ElementPosition].X, data.Elements[data.ElementPosition].Y, data.Elements[data.ElementPosition].Width, data.Elements[data.ElementPosition].Height);
                }

                if (data.ElementPosition == data.Width)
                {
                    data.Complete = true;
                }
            }
        }

        private static void Update(WaveFormData data, WaveFormRendererMode mode)
        {
            if (data.Elements == null)
            {
                return;
            }

            if (data.ElementPeak == 0)
            {
                if (data.DataPeak == 0)
                {
                    return;
                }
                data.ElementPeak = data.DataPeak;
            }

            switch (mode)
            {
                case WaveFormRendererMode.Mono:
                    UpdateMono(data);
                    break;
                case WaveFormRendererMode.Stereo:
                    UpdateStereo(data);
                    break;
                case WaveFormRendererMode.Seperate:
                    UpdateSeperate(data);
                    break;
                default:
                    throw new NotImplementedException();
            }

            if (data.ElementPeak != data.DataPeak)
            {
                data.ElementPosition = 0;
                data.ElementCount = 0;
                data.ElementPeak = 0;
            }
        }

        private static void UpdateMono(WaveFormData data)
        {
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

                var value = default(float);
                for (var a = 0; a < data.ValuesPerElement; a++)
                {
                    value += data.Data[valuePosition + a, 0];
                }
                value /= data.ValuesPerElement;
                value /= data.DataPeak;

                y = Convert.ToInt32(center - (value * center));
                height = Convert.ToInt32((center - y) + (value * center));

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

        private static void UpdateStereo(WaveFormData data)
        {
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
                    leftValue += data.Data[valuePosition + a, 0];
                    rightValue += data.Data[valuePosition + a, 1];
                }
                leftValue /= data.ValuesPerElement;
                leftValue /= data.DataPeak;
                rightValue /= data.ValuesPerElement;
                rightValue /= data.DataPeak;

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

        private static void UpdateSeperate(WaveFormData data)
        {
            while (data.ElementCount < data.Elements.Length)
            {
                var valuePosition = (data.ElementCount / data.ChannelCount) * data.ValuesPerElement;
                if (valuePosition > data.DataPosition)
                {
                    break;
                }

                var x = data.ElementCount / data.ChannelCount;

                var waveHeight = data.Height / data.ChannelCount;

                for (var channel = 0; channel < data.ChannelCount; channel++)
                {
                    var waveCenter = (waveHeight * channel) + (waveHeight / 2);

                    var y = default(int);
                    var width = 1;
                    var height = default(int);

                    var value = default(float);
                    for (var a = 0; a < data.ValuesPerElement; a++)
                    {
                        value += data.Data[valuePosition + a, channel];
                    }
                    value /= data.ValuesPerElement;
                    value /= data.DataPeak;

                    y = Convert.ToInt32(waveCenter - (value * (waveHeight / 2)));
                    height = Convert.ToInt32((waveCenter - y) + (value * (waveHeight / 2)));

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
        }

        private static void Populate(WaveFormData data)
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

                var peak = default(float);

                for (var a = 0; a < length; a += data.ChannelCount)
                {
                    for (var b = 0; b < data.ChannelCount; b++)
                    {
                        data.Data[data.DataPosition, b] = Math.Max(
                            data.Data[data.DataPosition, b],
                            buffer[a + b]
                        );
                        peak = Math.Max(peak, buffer[a + b]);
                    }
                }

                if (peak > data.DataPeak)
                {
                    data.DataPeak = peak;
                }

                data.DataPosition++;
            } while (!data.CancellationToken.IsCancellationRequested);
        }

        private static WaveFormData GetData(IOutputStream stream, int resolution, WaveFormRendererMode mode)
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
                ElementPeak = 0,
                Data = new float[length, stream.Channels],
                DataPosition = 0,
                DataCount = length,
                DataPeak = 0,
                ChannelCount = stream.Channels,
                CancellationToken = new CancellationToken(),
                Mode = mode
            };
        }

        public class WaveFormData
        {
            public int Width;

            public int Height;

            public int Resolution;

            public IOutputStream Stream;

            public int ValuesPerElement;

            public Int32Rect[] Elements;

            public int ElementPosition;

            public int ElementCount;

            public float ElementPeak;

            public float[,] Data;

            public int DataPosition;

            public int DataCount;

            public float DataPeak;

            public int ChannelCount;

            public bool Complete;

            public CancellationToken CancellationToken;

            public WaveFormRendererMode Mode;
        }
    }

    public enum WaveFormRendererMode : byte
    {
        None,
        Mono,
        Stereo,
        Seperate
    }
}
