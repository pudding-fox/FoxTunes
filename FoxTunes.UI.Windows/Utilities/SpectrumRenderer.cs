using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FoxTunes
{
    public class SpectrumRenderer : BaseComponent, IDisposable
    {
        const int SCALE_FACTOR = 4;

        const int ROLLOFF_INTERVAL = 500;

        public readonly object SyncRoot = new object();

        public readonly Duration LockTimeout = new Duration(TimeSpan.FromMilliseconds(1));

        public DateTime LastUpdated;

        public IOutput Output;

        public SpectrumData Data;

        public WriteableBitmap Bitmap;

        public Color Color;

        public SpectrumRendererFlags Flags;

        public bool IsStarted;

        public global::System.Timers.Timer Timer;

        private SpectrumRenderer()
        {
            this.LastUpdated = DateTime.UtcNow;
        }

        public SpectrumRenderer(int width, int height, int elements, int fftSize, int updateInterval, int holdInterval, int smoothingFactor, int amplitude, Color color, SpectrumRendererFlags flags) : this()
        {
            this.Data.Width = width;
            this.Data.Height = height;
            this.Data.ElementCount = elements;
            this.Data.FFTSize = fftSize;
            this.Data.HoldInterval = holdInterval;
            this.Data.Smoothing = smoothingFactor;
            this.Data.Amplitude = (float)amplitude / 500;
            this.Timer = new global::System.Timers.Timer();
            this.Timer.Interval = updateInterval;
            this.Timer.AutoReset = false;
            this.Timer.Elapsed += this.OnElapsed;
            this.Color = color;
            this.Flags = flags;
        }

        public override void InitializeComponent(ICore core)
        {
            PlaybackStateNotifier.Notify += this.OnNotify;
            this.Output = core.Components.Output;
            this.Data.Samples = new float[this.Data.FFTSize];
            this.Data.Values = new float[this.Data.ElementCount];
            this.Data.Elements = new int[this.Data.ElementCount, 4];
            if (this.Flags.HasFlag(SpectrumRendererFlags.ShowPeaks))
            {
                this.Data.Peaks = new int[this.Data.ElementCount, 4];
                this.Data.Holds = new int[this.Data.ElementCount];
            }
            else
            {
                this.Data.Peaks = null;
                this.Data.Holds = null;
            }
            if (this.Flags.HasFlag(SpectrumRendererFlags.HighCut))
            {
                this.Data.FFTRange = this.Data.FFTSize - (this.Data.FFTSize / 4);
            }
            else
            {
                this.Data.FFTRange = this.Data.FFTSize;
            }
            this.Data.SamplesPerElement = Math.Max(this.Data.FFTRange / this.Data.ElementCount, 1);
            this.Data.Step = this.Data.Width / this.Data.ElementCount;
            this.Bitmap = new WriteableBitmap(
               this.Data.Width,
               this.Data.Height,
               96,
               96,
               PixelFormats.Pbgra32,
               null
            );
            Logger.Write(this, LogLevel.Debug, "Renderer created: {0}x{1}", this.Data.Width, this.Data.Height);
            base.InitializeComponent(core);
        }

        protected virtual void OnNotify(object sender, EventArgs e)
        {
            if (PlaybackStateNotifier.IsPlaying && !this.IsStarted)
            {
                Logger.Write(this, LogLevel.Debug, "Playback was started, starting renderer.");
                this.Start();
            }
            else if (!PlaybackStateNotifier.IsPlaying && this.IsStarted)
            {
                Logger.Write(this, LogLevel.Debug, "Playback was stopped, stopping renderer.");
                this.Stop();
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

        protected virtual async Task Render()
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
            }).ConfigureAwait(false);

            if (!success)
            {
                //Failed to establish lock.
                this.Start();
                return;
            }

            try
            {
                var elements = this.Data.Elements;
                var peaks = this.Data.Peaks;
                var elementCount = this.Data.ElementCount;
                BitmapHelper.Clear(info);
                for (var a = 0; a < elementCount; a++)
                {
                    BitmapHelper.DrawRectangle(info, elements[a, 0], elements[a, 1], elements[a, 2], elements[a, 3]);
                    if (peaks != null)
                    {
                        if (peaks[a, 1] >= elements[a, 1])
                        {
                            continue;
                        }
                        BitmapHelper.DrawRectangle(info, peaks[a, 0], peaks[a, 1], peaks[a, 2], peaks[a, 3]);
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
                Logger.Write(this.GetType(), LogLevel.Warn, "Failed to paint spectrum, disabling: {0}", e.Message);
            }
        }


        protected virtual void Clear()
        {
            for (var a = 0; a < this.Data.ElementCount; a++)
            {
                this.Data.Elements[a, 0] = a * this.Data.Step;
                this.Data.Elements[a, 1] = this.Data.Height - 1;
                this.Data.Elements[a, 2] = this.Data.Step;
                this.Data.Elements[a, 3] = 1;
                if (this.Data.Peaks != null)
                {
                    this.Data.Peaks[a, 0] = a * this.Data.Step;
                    this.Data.Peaks[a, 1] = this.Data.Height - 1;
                    this.Data.Peaks[a, 2] = this.Data.Step;
                    this.Data.Peaks[a, 3] = 1;
                }
            }
        }

        protected virtual void Update()
        {
            var now = DateTime.UtcNow;
            var duration = now - this.LastUpdated;
            lock (this.SyncRoot)
            {
                if (this.Timer == null)
                {
                    //Disposed.
                    return;
                }
                this.Data.Duration = Convert.ToInt32(Math.Min(duration.TotalMilliseconds, this.Timer.Interval * 100));
            }
            Update(this.Data);
            if (this.Flags.HasFlag(SpectrumRendererFlags.Smooth))
            {
                UpdateSmooth(this.Data);
            }
            else
            {
                UpdateFast(this.Data);
            }
            if (this.Flags.HasFlag(SpectrumRendererFlags.ShowPeaks))
            {
                UpdatePeaks(this.Data);
            }
            this.LastUpdated = DateTime.UtcNow;
        }

        protected virtual void OnElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                var length = this.Output.GetData(this.Data.Samples);
                if (length <= 0)
                {
                    this.Clear();
                }
                else
                {
                    this.Update();
                }
                var task = this.Render();
            }
            catch (Exception exception)
            {
                Logger.Write(this.GetType(), LogLevel.Warn, "Failed to update spectrum data, disabling: {0}", exception.Message);
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
            PlaybackStateNotifier.Notify -= this.OnNotify;
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

        ~SpectrumRenderer()
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

        private static void Update(SpectrumData data)
        {
            if (data.SamplesPerElement > 1)
            {
                for (int a = 0, b = 0; a < data.FFTRange; a += data.SamplesPerElement, b++)
                {
                    var value = 0.0f;
                    for (var c = 0; c < data.SamplesPerElement; c++)
                    {
                        var boost = (float)(1.0f + a * data.Amplitude);
                        value += (float)(Math.Sqrt(data.Samples[a + c] * boost) * SCALE_FACTOR);
                    }
                    data.Values[b] = Math.Min(Math.Max(value / data.SamplesPerElement, 0), 1);
                }
            }
            else
            {
                //Not enough samples to fill the values, do the best we can.
                for (int a = 0; a < data.ElementCount; a++)
                {
                    var boost = (float)(1.0f + a * data.Amplitude);
                    var value = (float)(Math.Sqrt(data.Samples[a] * boost) * SCALE_FACTOR);
                    data.Values[a] = Math.Min(Math.Max(value, 0), 1);
                }
            }
        }

        private static void UpdateFast(SpectrumData data)
        {
            for (var a = 0; a < data.ElementCount; a++)
            {
                var barHeight = Convert.ToInt32(data.Values[a] * data.Height);
                data.Elements[a, 0] = a * data.Step;
                data.Elements[a, 2] = data.Step;
                if (barHeight > 0)
                {
                    data.Elements[a, 3] = barHeight;
                }
                else
                {
                    data.Elements[a, 3] = 1;
                }
                data.Elements[a, 1] = data.Height - data.Elements[a, 3];
            }
        }

        private static void UpdateSmooth(SpectrumData data)
        {
            var fast = (float)data.Height / data.Smoothing;
            for (var a = 0; a < data.ElementCount; a++)
            {
                var barHeight = Convert.ToInt32(data.Values[a] * data.Height);
                data.Elements[a, 0] = a * data.Step;
                data.Elements[a, 2] = data.Step;
                if (barHeight > 0)
                {
                    var difference = Math.Abs(data.Elements[a, 3] - barHeight);
                    if (difference > 0)
                    {
                        if (difference < 2)
                        {
                            if (barHeight > data.Elements[a, 3])
                            {
                                data.Elements[a, 3]++;
                            }
                            else if (barHeight < data.Elements[a, 3])
                            {
                                data.Elements[a, 3]--;
                            }
                        }
                        else
                        {
                            var distance = (float)difference / barHeight;
                            //TODO: We should use some kind of easing function.
                            //var increment = distance * distance * distance;
                            //var increment = 1 - Math.Pow(1 - distance, 5);
                            var increment = distance;
                            if (barHeight > data.Elements[a, 3])
                            {
                                data.Elements[a, 3] = (int)Math.Min(data.Elements[a, 3] + Math.Min(Math.Max(fast * increment, 1), difference), data.Height);
                            }
                            else if (barHeight < data.Elements[a, 3])
                            {
                                data.Elements[a, 3] = (int)Math.Max(data.Elements[a, 3] - Math.Min(Math.Max(fast * increment, 1), difference), 1);
                            }
                        }
                    }
                }
                else
                {
                    data.Elements[a, 3] = 1;
                }
                data.Elements[a, 1] = data.Height - data.Elements[a, 3];
            }
        }

        private static void UpdatePeaks(SpectrumData data)
        {
            var fast = data.Height / 4;
            for (int a = 0; a < data.ElementCount; a++)
            {
                if (data.Elements[a, 1] < data.Peaks[a, 1])
                {
                    data.Peaks[a, 0] = a * data.Step;
                    data.Peaks[a, 2] = data.Step;
                    data.Peaks[a, 3] = 1;
                    data.Peaks[a, 1] = data.Elements[a, 1];
                    data.Holds[a] = data.HoldInterval + ROLLOFF_INTERVAL;
                }
                else if (data.Elements[a, 1] > data.Peaks[a, 1] && data.Peaks[a, 1] < data.Height - 1)
                {
                    if (data.Holds[a] > 0)
                    {
                        if (data.Holds[a] < data.HoldInterval)
                        {
                            var distance = 1 - ((float)data.Holds[a] / data.HoldInterval);
                            var increment = fast * (distance * distance * distance);
                            if (data.Peaks[a, 1] < data.Height - increment)
                            {
                                data.Peaks[a, 1] += (int)Math.Round(increment);
                            }
                            else if (data.Peaks[a, 1] < data.Height - 1)
                            {
                                data.Peaks[a, 1] = data.Height - 1;
                            }
                        }
                        data.Holds[a] -= data.Duration;
                    }
                    else if (data.Peaks[a, 1] < data.Height - fast)
                    {
                        data.Peaks[a, 1] += fast;
                    }
                    else if (data.Peaks[a, 1] < data.Height - 1)
                    {
                        data.Peaks[a, 1] = data.Height - 1;
                    }
                }
            }
        }

        public struct SpectrumData
        {
            public int FFTSize;

            public int FFTRange;

            public float Amplitude;

            public float[] Samples;

            public float[] Values;

            public int ElementCount;

            public int SamplesPerElement;

            public int Step;

            public int Width;

            public int Height;

            public int[,] Elements;

            public int[,] Peaks;

            public int[] Holds;

            public int UpdateInterval;

            public int HoldInterval;

            public int Duration;

            public int Smoothing;
        }
    }

    [Flags]
    public enum SpectrumRendererFlags : byte
    {
        None = 0,
        ShowPeaks = 1,
        HighCut = 2,
        Smooth = 4
    }
}
