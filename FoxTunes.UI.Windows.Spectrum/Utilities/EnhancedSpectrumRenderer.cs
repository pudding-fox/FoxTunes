using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Media.Imaging;

namespace FoxTunes
{
    public class EnhancedSpectrumRenderer : RendererBase
    {
        public static readonly int[] BANDS = new[]
       {
            20,
            50,
            100,
            200,
            500,
            1000,
            2000,
            5000,
            10000,
            20000
        };

        public static readonly int MIN_FREQ = BANDS[0];

        public static readonly int MAX_FREQ = BANDS[BANDS.Length - 1];

        public const float DB_MIN = -90;

        public const float DB_MAX = 0;

        public const int ROLLOFF_INTERVAL = 500;

        public readonly object SyncRoot = new object();

        public SpectrumRendererData RendererData { get; private set; }

        public bool IsStarted;

        public global::System.Timers.Timer Timer;

        public IOutput Output { get; private set; }

        public BooleanConfigurationElement ShowPeaks { get; private set; }

        public BooleanConfigurationElement Smooth { get; private set; }

        public IntegerConfigurationElement SmoothingFactor { get; private set; }

        public IntegerConfigurationElement HoldInterval { get; private set; }

        public IntegerConfigurationElement UpdateInterval { get; private set; }

        public SelectionConfigurationElement FFTSize { get; private set; }

        public EnhancedSpectrumRenderer()
        {
            this.Timer = new global::System.Timers.Timer();
            this.Timer.AutoReset = false;
            this.Timer.Elapsed += this.OnElapsed;
        }

        public override void InitializeComponent(ICore core)
        {
            base.InitializeComponent(core);
            PlaybackStateNotifier.Notify += this.OnNotify;
            this.Output = core.Components.Output;
            this.ShowPeaks = this.Configuration.GetElement<BooleanConfigurationElement>(
                SpectrumBehaviourConfiguration.SECTION,
                SpectrumBehaviourConfiguration.PEAKS_ELEMENT
             );
            this.Smooth = this.Configuration.GetElement<BooleanConfigurationElement>(
               SpectrumBehaviourConfiguration.SECTION,
               SpectrumBehaviourConfiguration.SMOOTH_ELEMENT
            );
            this.SmoothingFactor = this.Configuration.GetElement<IntegerConfigurationElement>(
               SpectrumBehaviourConfiguration.SECTION,
               SpectrumBehaviourConfiguration.SMOOTH_FACTOR_ELEMENT
            );
            this.HoldInterval = this.Configuration.GetElement<IntegerConfigurationElement>(
               SpectrumBehaviourConfiguration.SECTION,
               SpectrumBehaviourConfiguration.HOLD_ELEMENT
            );
            this.UpdateInterval = this.Configuration.GetElement<IntegerConfigurationElement>(
               SpectrumBehaviourConfiguration.SECTION,
               SpectrumBehaviourConfiguration.INTERVAL_ELEMENT
            );
            this.FFTSize = this.Configuration.GetElement<SelectionConfigurationElement>(
               SpectrumBehaviourConfiguration.SECTION,
               SpectrumBehaviourConfiguration.FFT_SIZE_ELEMENT
            );
            this.ScalingFactor.ValueChanged += this.OnValueChanged;
            this.ShowPeaks.ValueChanged += this.OnValueChanged;
            this.Smooth.ValueChanged += this.OnValueChanged;
            this.SmoothingFactor.ValueChanged += this.OnValueChanged;
            this.HoldInterval.ValueChanged += this.OnValueChanged;
            this.UpdateInterval.ValueChanged += this.OnValueChanged;
            this.FFTSize.ValueChanged += this.OnValueChanged;
#if NET40
            var task = TaskEx.Run(async () =>
#else
            var task = Task.Run(async () =>
#endif
            {
                await this.CreateBitmap().ConfigureAwait(false);
                if (PlaybackStateNotifier.IsPlaying)
                {
                    this.Start();
                }
            });
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

        protected virtual void OnValueChanged(object sender, EventArgs e)
        {
            lock (this.SyncRoot)
            {
                if (this.Timer != null)
                {
                    this.Timer.Interval = UpdateInterval.Value;
                }
            }
            var task = this.RefreshBitmap();
        }

        protected override void CreateViewBox()
        {
            this.RendererData = Create(
                this.Output,
                this.Bitmap.PixelWidth,
                this.Bitmap.PixelHeight,
                SpectrumBehaviourConfiguration.GetFFTSize(this.FFTSize.Value),
                this.HoldInterval.Value,
                this.UpdateInterval.Value,
                this.SmoothingFactor.Value,
                this.ShowPeaks.Value
            );
            this.Viewbox = new Rect(0, 0, this.GetPixelWidth(), this.Bitmap.PixelHeight);
        }

        protected virtual Task RefreshBitmap()
        {
            return Windows.Invoke(() =>
            {
                this.RendererData = Create(
                    this.Output,
                    this.Bitmap.PixelWidth,
                    this.Bitmap.PixelHeight,
                    SpectrumBehaviourConfiguration.GetFFTSize(this.FFTSize.Value),
                    this.HoldInterval.Value,
                    this.UpdateInterval.Value,
                    this.SmoothingFactor.Value,
                    this.ShowPeaks.Value
                );
            });
        }

        public void Start()
        {
            lock (this.SyncRoot)
            {
                if (this.Timer != null)
                {
                    this.IsStarted = true;
                    this.Timer.Interval = UpdateInterval.Value;
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
            var bitmap = default(WriteableBitmap);
            var success = default(bool);
            var info = default(BitmapHelper.RenderInfo);

            await Windows.Invoke(() =>
            {
                bitmap = this.Bitmap;
                success = bitmap.TryLock(LockTimeout);
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

            Render(info, this.RendererData);

            await Windows.Invoke(() =>
            {
                bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
                bitmap.Unlock();
                this.Start();
            }).ConfigureAwait(false);
        }

        protected virtual void Clear()
        {
            var elements = this.RendererData.Elements;
            var peaks = this.RendererData.Peaks;

            for (var a = 0; a < elements.Length; a++)
            {
                elements[a].X = a * this.RendererData.Step;
                elements[a].Y = this.RendererData.Height - 1;
                elements[a].Width = this.RendererData.Step;
                elements[a].Height = 1;
                if (this.RendererData.Peaks != null)
                {
                    peaks[a].X = a * this.RendererData.Step;
                    peaks[a].Y = this.RendererData.Height - 1;
                    peaks[a].Width = this.RendererData.Step;
                    peaks[a].Height = 1;
                }
            }
        }

        protected virtual void OnElapsed(object sender, ElapsedEventArgs e)
        {

            try
            {
                if (!this.RendererData.Update())
                {
                    this.Clear();
                }
                else
                {
                    UpdateValues(this.RendererData);
                    if (this.Smooth.Value)
                    {
                        UpdateElementsSmooth(this.RendererData);
                    }
                    else
                    {
                        UpdateElementsFast(this.RendererData);
                    }
                    if (this.ShowPeaks.Value)
                    {
                        UpdatePeaks(this.RendererData);
                    }
                }
                var task = this.Render();
            }
            catch (Exception exception)
            {
                Logger.Write(this.GetType(), LogLevel.Warn, "Failed to update spectrum data, disabling: {0}", exception.Message);
            }
        }

        protected virtual double GetPixelWidth()
        {
            if (this.RendererData == null)
            {
                return 1;
            }
            return BANDS.Length * this.RendererData.Step;
        }

        protected override Freezable CreateInstanceCore()
        {
            return new SpectrumRenderer();
        }

        protected override void OnDisposing()
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
            if (this.ScalingFactor != null)
            {
                this.ScalingFactor.ValueChanged -= this.OnValueChanged;
            }
            if (this.ShowPeaks != null)
            {
                this.ShowPeaks.ValueChanged -= this.OnValueChanged;
            }
            if (this.Smooth != null)
            {
                this.Smooth.ValueChanged -= this.OnValueChanged;
            }
            if (this.SmoothingFactor != null)
            {
                this.SmoothingFactor.ValueChanged -= this.OnValueChanged;
            }
            if (this.HoldInterval != null)
            {
                this.HoldInterval.ValueChanged -= this.OnValueChanged;
            }
            if (this.UpdateInterval != null)
            {
                this.UpdateInterval.ValueChanged -= this.OnValueChanged;
            }
            if (this.FFTSize != null)
            {
                this.FFTSize.ValueChanged -= this.OnValueChanged;
            }
        }

        private static void Render(BitmapHelper.RenderInfo info, SpectrumRendererData rendererData)
        {
            var elements = rendererData.Elements;
            var peaks = rendererData.Peaks;

            BitmapHelper.Clear(info);

            for (var a = 0; a < elements.Length; a++)
            {
                BitmapHelper.DrawRectangle(
                    info,
                    elements[a].X,
                    elements[a].Y,
                    elements[a].Width,
                    elements[a].Height
                );
                if (peaks != null)
                {
                    if (peaks[a].Y >= elements[a].Y)
                    {
                        continue;
                    }
                    BitmapHelper.DrawRectangle(
                        info,
                        peaks[a].X,
                        peaks[a].Y,
                        peaks[a].Width,
                        peaks[a].Height
                    );
                }
            }
        }

        private static void UpdateValues(SpectrumRendererData data)
        {
            var samples = data.Samples;
            var values = data.Values;
            var position = default(int);
            var value = default(float);

            for (var a = FrequencyToIndex(BANDS[0], data.FFTSize, data.Rate); a < data.FFTSize; a++)
            {
                var frequency = IndexToFrequency(a, data.FFTSize, data.Rate);
                while (frequency > BANDS[position])
                {
                    if (position < (BANDS.Length - 1))
                    {
                        values[position] = value;
                        position++;
                        value = 0.0f;
                    }
                    else
                    {
                        values[position] = value;
                        return;
                    }
                }
                var dB = Math.Min(Math.Max((float)(20 * Math.Log10(samples[a])), DB_MIN), DB_MAX);
                value = 1.0f - Math.Abs(dB) / Math.Abs(DB_MIN);
            }
        }

        private static void UpdateElementsFast(SpectrumRendererData data)
        {
            var values = data.Values;
            var elements = data.Elements;

            for (var a = 0; a < elements.Length; a++)
            {
                var barHeight = Convert.ToInt32(values[a] * data.Height);
                elements[a].X = a * data.Step;
                elements[a].Width = data.Step;
                if (barHeight > 0)
                {
                    elements[a].Height = barHeight;
                }
                else
                {
                    elements[a].Height = 1;
                }
                elements[a].Y = data.Height - elements[a].Height;
            }
        }

        private static void UpdateElementsSmooth(SpectrumRendererData data)
        {
            var values = data.Values;
            var elements = data.Elements;

            var fast = (float)data.Height / data.Smoothing;
            for (var a = 0; a < elements.Length; a++)
            {
                var barHeight = Convert.ToInt32(values[a] * data.Height);
                elements[a].X = a * data.Step;
                elements[a].Width = data.Step;
                if (barHeight > 0)
                {
                    var difference = Math.Abs(elements[a].Height - barHeight);
                    if (difference > 0)
                    {
                        if (difference < 2)
                        {
                            if (barHeight > elements[a].Height)
                            {
                                elements[a].Height++;
                            }
                            else if (barHeight < elements[a].Height)
                            {
                                elements[a].Height--;
                            }
                        }
                        else
                        {
                            var distance = (float)difference / barHeight;
                            //TODO: We should use some kind of easing function.
                            //var increment = distance * distance * distance;
                            //var increment = 1 - Math.Pow(1 - distance, 5);
                            var increment = distance;
                            if (barHeight > elements[a].Height)
                            {
                                elements[a].Height = (int)Math.Min(elements[a].Height + Math.Min(Math.Max(fast * increment, 1), difference), data.Height);
                            }
                            else if (barHeight < elements[a].Height)
                            {
                                elements[a].Height = (int)Math.Max(elements[a].Height - Math.Min(Math.Max(fast * increment, 1), difference), 1);
                            }
                        }
                    }
                }
                else
                {
                    elements[a].Height = 1;
                }
                elements[a].Y = data.Height - elements[a].Height;
            }
        }

        private static void UpdatePeaks(SpectrumRendererData data)
        {
            var duration = Convert.ToInt32(
                Math.Min(
                    (DateTime.UtcNow - data.LastUpdated).TotalMilliseconds,
                    data.UpdateInterval * 100
                )
            );

            var peaks = data.Peaks;
            var holds = data.Holds;
            var elements = data.Elements;

            var fast = data.Height / 4;
            for (int a = 0; a < elements.Length; a++)
            {
                if (elements[a].Y < peaks[a].Y)
                {
                    peaks[a].X = a * data.Step;
                    peaks[a].Width = data.Step;
                    peaks[a].Height = 1;
                    peaks[a].Y = elements[a].Y;
                    holds[a] = data.HoldInterval + ROLLOFF_INTERVAL;
                }
                else if (elements[a].Y > peaks[a].Y && peaks[a].Y < data.Height - 1)
                {
                    if (holds[a] > 0)
                    {
                        if (holds[a] < data.HoldInterval)
                        {
                            var distance = 1 - ((float)holds[a] / data.HoldInterval);
                            var increment = fast * (distance * distance * distance);
                            if (peaks[a].Y < data.Height - increment)
                            {
                                peaks[a].Y += (int)Math.Round(increment);
                            }
                            else if (peaks[a].Y < data.Height - 1)
                            {
                                peaks[a].Y = data.Height - 1;
                            }
                        }
                        holds[a] -= duration;
                    }
                    else if (peaks[a].Y < data.Height - fast)
                    {
                        peaks[a].Y += fast;
                    }
                    else if (peaks[a].Y < data.Height - 1)
                    {
                        peaks[a].Y = data.Height - 1;
                    }
                }
            }

            data.LastUpdated = DateTime.UtcNow;
        }

        private static int Nyquist(int rate)
        {
            return rate / 2;
        }

        public static int IndexToFrequency(int index, int fftSize, int rate)
        {
            return (int)Math.Floor((double)index * (double)rate / (double)fftSize);
        }

        public static int FrequencyToIndex(int frequency, int fftSize, int rate)
        {
            var index = (int)Math.Floor((double)fftSize * (double)frequency / (double)rate);
            if (index > fftSize / 2 - 1)
            {
                index = fftSize / 2 - 1;
            }
            return index;
        }

        public static SpectrumRendererData Create(IOutput output, int width, int height, int fftSize, int holdInterval, int updateInterval, int smoothingFactor, bool showPeaks)
        {
            var data = new SpectrumRendererData()
            {
                Output = output,
                Width = width,
                Height = height,
                FFTSize = fftSize,
                HoldInterval = holdInterval,
                UpdateInterval = updateInterval,
                Smoothing = smoothingFactor,
                Samples = output.GetBuffer(fftSize),
                Values = new float[BANDS.Length],
                Elements = new Int32Rect[BANDS.Length]
            };
            if (showPeaks)
            {
                data.Peaks = new Int32Rect[BANDS.Length];
                data.Holds = new int[BANDS.Length];
            }
            data.Step = width / BANDS.Length;
            return data;
        }

        public class SpectrumRendererData
        {
            public IOutput Output;

            public int Rate;

            public int Depth;

            public int FFTSize;

            public float[] Samples;

            public float[] Values;

            public int Step;

            public int Width;

            public int Height;

            public Int32Rect[] Elements;

            public Int32Rect[] Peaks;

            public int[] Holds;

            public int UpdateInterval;

            public int HoldInterval;

            public int Smoothing;

            public DateTime LastUpdated;

            public bool Update()
            {
                this.Rate = this.Output.GetRate();
                this.Depth = this.Output.GetDepth();
                return this.Output.GetData(this.Samples, this.FFTSize) > 0;
            }
        }
    }
}
