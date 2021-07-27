using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FoxTunes
{
    public class EnhancedSpectrumRenderer : RendererBase
    {
        public const int DB_MIN = -90;

        public const int DB_MAX = 0;

        public const int ROLLOFF_INTERVAL = 500;

        public readonly object SyncRoot = new object();

        public SpectrumRendererData RendererData { get; private set; }

        public bool IsStarted;

        public global::System.Timers.Timer Timer;

        public IOutput Output { get; private set; }

        public SelectionConfigurationElement Bands { get; private set; }

        public BooleanConfigurationElement ShowPeaks { get; private set; }

        public BooleanConfigurationElement ShowRms { get; private set; }

        public BooleanConfigurationElement ShowCrestFactor { get; private set; }

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
            this.Bands = this.Configuration.GetElement<SelectionConfigurationElement>(
                SpectrumBehaviourConfiguration.SECTION,
                SpectrumBehaviourConfiguration.BANDS_ELEMENT
            );
            this.ShowPeaks = this.Configuration.GetElement<BooleanConfigurationElement>(
                SpectrumBehaviourConfiguration.SECTION,
                SpectrumBehaviourConfiguration.PEAKS_ELEMENT
             );
            this.ShowRms = this.Configuration.GetElement<BooleanConfigurationElement>(
                SpectrumBehaviourConfiguration.SECTION,
                SpectrumBehaviourConfiguration.RMS_ELEMENT
             );
            this.ShowCrestFactor = this.Configuration.GetElement<BooleanConfigurationElement>(
                SpectrumBehaviourConfiguration.SECTION,
                SpectrumBehaviourConfiguration.CREST_ELEMENT
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
            this.Bands.ValueChanged += this.OnValueChanged;
            this.ShowPeaks.ValueChanged += this.OnValueChanged;
            this.ShowRms.ValueChanged += this.OnValueChanged;
            this.ShowCrestFactor.ValueChanged += this.OnValueChanged;
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
            if (object.ReferenceEquals(sender, this.Bands))
            {
                //Changing bands requires full refresh.
                var task = this.CreateBitmap();
            }
            else
            {
                var task = this.RefreshBitmap();
            }
        }

        protected override void CreateViewBox()
        {
            this.RendererData = Create(
                this.Output,
                this.Bitmap.PixelWidth,
                this.Bitmap.PixelHeight,
                SpectrumBehaviourConfiguration.GetBands(this.Bands.Value),
                SpectrumBehaviourConfiguration.GetFFTSize(this.FFTSize.Value),
                this.HoldInterval.Value,
                this.UpdateInterval.Value,
                this.SmoothingFactor.Value,
                this.ShowPeaks.Value,
                this.ShowRms.Value,
                this.ShowCrestFactor.Value
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
                    SpectrumBehaviourConfiguration.GetBands(this.Bands.Value),
                    SpectrumBehaviourConfiguration.GetFFTSize(this.FFTSize.Value),
                    this.HoldInterval.Value,
                    this.UpdateInterval.Value,
                    this.SmoothingFactor.Value,
                    this.ShowPeaks.Value,
                    this.ShowRms.Value,
                    this.ShowCrestFactor.Value
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
            const byte SHADE = 30;

            var bitmap = default(WriteableBitmap);
            var success = default(bool);
            var valueRenderInfo = default(BitmapHelper.RenderInfo);
            var rmsRenderInfo = default(BitmapHelper.RenderInfo);
            var crestRenderInfo = default(BitmapHelper.RenderInfo);

            await Windows.Invoke(() =>
            {
                bitmap = this.Bitmap;
                success = bitmap.TryLock(LockTimeout);
                if (!success)
                {
                    return;
                }
                if (this.ShowRms.Value)
                {
                    var colors = this.Color.ToPair(SHADE);
                    valueRenderInfo = BitmapHelper.CreateRenderInfo(bitmap, colors[0]);
                    rmsRenderInfo = BitmapHelper.CreateRenderInfo(bitmap, colors[1]);
                    crestRenderInfo = BitmapHelper.CreateRenderInfo(bitmap, Colors.Red);
                }
                else
                {
                    valueRenderInfo = BitmapHelper.CreateRenderInfo(bitmap, this.Color);
                }
            }).ConfigureAwait(false);

            if (!success)
            {
                //Failed to establish lock.
                this.Start();
                return;
            }

            Render(valueRenderInfo, rmsRenderInfo, crestRenderInfo, this.RendererData);

            await Windows.Invoke(() =>
            {
                bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
                bitmap.Unlock();
                this.Start();
            }).ConfigureAwait(false);
        }

        protected virtual void Clear()
        {
            var data = this.RendererData;
            if (data == null)
            {
                return;
            }

            var valueElements = data.ValueElements;
            var rmsElements = data.RmsElements;
            var peakElements = data.PeakElements;
            var crestPoints = data.CrestPoints;

            var step = data.Step;
            var height = data.Height - 1;

            for (var a = 0; a < valueElements.Length; a++)
            {
                valueElements[a].X = a * step;
                valueElements[a].Y = height;
                valueElements[a].Width = step;
                valueElements[a].Height = 1;
                if (rmsElements != null)
                {
                    rmsElements[a].X = a * step;
                    rmsElements[a].Y = height;
                    rmsElements[a].Width = step;
                    rmsElements[a].Height = 1;
                }
                if (peakElements != null)
                {
                    peakElements[a].X = a * step;
                    peakElements[a].Y = height;
                    peakElements[a].Width = step;
                    peakElements[a].Height = 1;
                }
                if (crestPoints != null)
                {
                    crestPoints[a].X = (a * step) + (step / 2);
                    crestPoints[a].Y = height - 1;
                }
            }
        }

        protected virtual void OnElapsed(object sender, ElapsedEventArgs e)
        {
            var data = this.RendererData;
            if (data == null)
            {
                return;
            }
            try
            {
                if (!data.Update())
                {
                    this.Clear();
                }
                else
                {
                    UpdateValues(data);
                    if (this.Smooth.Value)
                    {
                        UpdateElementsSmooth(data);
                    }
                    else
                    {
                        UpdateElementsFast(data);
                    }
                    if (this.ShowPeaks.Value)
                    {
                        UpdatePeaks(data);
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
            return this.RendererData.Bands.Length * this.RendererData.Step;
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
            if (this.ShowRms != null)
            {
                this.ShowRms.ValueChanged -= this.OnValueChanged;
            }
            if (this.ShowCrestFactor != null)
            {
                this.ShowCrestFactor.ValueChanged -= this.OnValueChanged;
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

        private static void Render(BitmapHelper.RenderInfo valueRenderInfo, BitmapHelper.RenderInfo rmsRenderInfo, BitmapHelper.RenderInfo crestRenderInfo, SpectrumRendererData rendererData)
        {
            var valueElements = rendererData.ValueElements;
            var rmsElements = rendererData.RmsElements;
            var crestPoints = rendererData.CrestPoints;
            var peakElements = rendererData.PeakElements;

            BitmapHelper.Clear(valueRenderInfo);

            for (var a = 0; a < valueElements.Length; a++)
            {
                BitmapHelper.DrawRectangle(
                    valueRenderInfo,
                    valueElements[a].X,
                    valueElements[a].Y,
                    valueElements[a].Width,
                    valueElements[a].Height
                );
                if (rmsElements != null)
                {
                    BitmapHelper.DrawRectangle(
                        rmsRenderInfo,
                        rmsElements[a].X,
                        rmsElements[a].Y,
                        rmsElements[a].Width,
                        rmsElements[a].Height
                    );
                }
                if (peakElements != null)
                {
                    if (peakElements[a].Y >= valueElements[a].Y)
                    {
                        continue;
                    }
                    BitmapHelper.DrawRectangle(
                        valueRenderInfo,
                        peakElements[a].X,
                        peakElements[a].Y,
                        peakElements[a].Width,
                        peakElements[a].Height
                    );
                }
            }

            if (crestPoints != null)
            {
                for (var a = 0; a < crestPoints.Length - 1; a++)
                {
                    var point1 = crestPoints[a];
                    var point2 = crestPoints[a + 1];
                    BitmapHelper.DrawLine(
                        crestRenderInfo,
                        point1.X,
                        point1.Y,
                        point2.X,
                        point2.Y
                    );
                }
            }
        }

        private static void UpdateValues(SpectrumRendererData data)
        {
            var bands = data.Bands;
            var position = default(int);

            data.ValuePeak = 0;
            data.RmsPeak = 0;

            for (int a = FrequencyToIndex(data.MinBand, data.FFTSize, data.Rate), b = a; a < data.FFTSize; a++)
            {
                var frequency = IndexToFrequency(a, data.FFTSize, data.Rate);
                while (frequency > bands[position])
                {
                    if (position < (bands.Length - 1))
                    {
                        UpdateValue(data, position, b, a);
                        b = a;
                        position++;
                    }
                    else
                    {
                        UpdateValue(data, position, b, a);
                        return;
                    }
                }
            }
        }

        private static void UpdateValue(SpectrumRendererData data, int band, int start, int end)
        {
            var samples = data.Samples;
            var value = default(float);
            var rms = default(float);
            var doRms = data.Rms != null;

            for (var a = start; a < end; a++)
            {
                value = Math.Max(samples[a], value);
                if (doRms)
                {
                    rms += samples[a] * samples[a];
                }
            }

            if (value > 0)
            {
                data.ValuePeak = Math.Max(UpdateValue(data.Values, band, value), data.ValuePeak);
            }
            else
            {
                data.Values[band] = 0;
            }

            if (doRms)
            {
                var count = end - start;
                if (count > 0)
                {
                    data.RmsPeak = Math.Max(UpdateValue(data.Rms, band, Convert.ToSingle(Math.Sqrt(rms / count))), data.RmsPeak);
                }
                else
                {
                    data.Rms[band] = 0;
                }
            }
        }

        private static float UpdateValue(float[] values, int band, float value)
        {
            var dB = Math.Min(Math.Max((float)(20 * Math.Log10(value)), DB_MIN), DB_MAX);
            return values[band] = 1.0f - Math.Abs(dB) / Math.Abs(DB_MIN);
        }

        private static void UpdateElementsFast(SpectrumRendererData data)
        {
            UpdateElementsFast(data.Values, data.ValueElements, data.Step, data.Height);
            if (data.Rms != null && data.RmsElements != null)
            {
                UpdateElementsFast(data.Rms, data.RmsElements, data.Step, data.Height);
            }
            if (data.Rms != null && data.CrestPoints != null)
            {
                UpdateElementsFast(data.Values, data.Rms, data.CrestPoints, data.ValuePeak, data.RmsPeak, data.Step, data.Height);
            }
        }

        private static void UpdateElementsFast(float[] values, Int32Rect[] elements, int step, int height)
        {
            for (var a = 0; a < values.Length; a++)
            {
                var barHeight = Convert.ToInt32(values[a] * height);
                elements[a].X = a * step;
                elements[a].Width = step;
                if (barHeight > 0)
                {
                    elements[a].Height = barHeight;
                }
                else
                {
                    elements[a].Height = 1;
                }
                elements[a].Y = height - elements[a].Height;
            }
        }

        private static void UpdateElementsFast(float[] values, float[] rms, Int32Point[] elements, float valuePeak, float rmsPeak, int step, int height)
        {
            height = height - 1;
            for (var a = 0; a < values.Length; a++)
            {
                var x = (a * step) + (step / 2);
                var y = default(int);
                if (values[a] > 0)
                {
                    y = height - Convert.ToInt32(((values[a] - rms[a]) + (1.0f - rmsPeak)) * height);
                }
                else
                {
                    y = height;
                }
                elements[a].X = x;
                elements[a].Y = y;
            }
        }

        private static void UpdateElementsSmooth(SpectrumRendererData data)
        {
            UpdateElementsSmooth(data.Values, data.ValueElements, data.Step, data.Height, data.Smoothing);
            if (data.Rms != null && data.RmsElements != null)
            {
                UpdateElementsSmooth(data.Rms, data.RmsElements, data.Step, data.Height, data.Smoothing);
            }
            if (data.Rms != null && data.CrestPoints != null)
            {
                UpdateElementsSmooth(data.Values, data.Rms, data.CrestPoints, data.ValuePeak, data.RmsPeak, data.Step, data.Height, data.Smoothing);
            }
        }

        private static void UpdateElementsSmooth(float[] values, Int32Rect[] elements, int step, int height, int smoothing)
        {
            var fast = (float)height / smoothing;
            for (var a = 0; a < values.Length; a++)
            {
                var barHeight = Math.Max(Convert.ToInt32(values[a] * height), 1);
                elements[a].X = a * step;
                elements[a].Width = step;
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
                        var smoothed = Math.Min(Math.Max(fast * increment, 1), difference);
                        if (barHeight > elements[a].Height)
                        {
                            elements[a].Height = (int)Math.Min(elements[a].Height + smoothed, height);
                        }
                        else if (barHeight < elements[a].Height)
                        {
                            elements[a].Height = (int)Math.Max(elements[a].Height - smoothed, 1);
                        }
                    }
                }
                elements[a].Y = height - elements[a].Height;
            }
        }

        private static void UpdateElementsSmooth(float[] values, float[] rms, Int32Point[] elements, float valuePeak, float rmsPeak, int step, int height, int smoothing)
        {
            height = height - 1;
            var fast = (float)height / smoothing;
            for (var a = 0; a < values.Length; a++)
            {
                var x = (a * step) + (step / 2);
                var y = default(int);
                if (values[a] > 0)
                {
                    y = height - Convert.ToInt32(((values[a] - rms[a]) + (1.0f - rmsPeak)) * height);
                }
                else
                {
                    y = height;
                }
                elements[a].X = x;
                var difference = Math.Abs(elements[a].Y - y);
                if (difference > 0)
                {
                    if (difference < 2)
                    {
                        if (y > elements[a].Y)
                        {
                            elements[a].Y++;
                        }
                        else if (y < elements[a].Y)
                        {
                            elements[a].Y--;
                        }
                    }
                    else
                    {
                        var distance = (float)difference / y;
                        //TODO: We should use some kind of easing function.
                        //var increment = distance * distance * distance;
                        //var increment = 1 - Math.Pow(1 - distance, 5);
                        var increment = distance;
                        var smoothed = Math.Min(Math.Max(fast * increment, 1), difference);
                        if (y > elements[a].Y)
                        {
                            elements[a].Y = (int)Math.Min(elements[a].Y + smoothed, height);
                        }
                        else if (y < elements[a].Y)
                        {
                            elements[a].Y = (int)Math.Max(elements[a].Y - smoothed, 1);
                        }
                    }
                }
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

            var valueElements = data.ValueElements;
            var peakElements = data.PeakElements;
            var holds = data.Holds;

            var fast = data.Height / 4;
            for (int a = 0; a < valueElements.Length; a++)
            {
                if (valueElements[a].Y < peakElements[a].Y)
                {
                    peakElements[a].X = a * data.Step;
                    peakElements[a].Width = data.Step;
                    peakElements[a].Height = 1;
                    peakElements[a].Y = valueElements[a].Y;
                    holds[a] = data.HoldInterval + ROLLOFF_INTERVAL;
                }
                else if (valueElements[a].Y > peakElements[a].Y && peakElements[a].Y < data.Height - 1)
                {
                    if (holds[a] > 0)
                    {
                        if (holds[a] < data.HoldInterval)
                        {
                            var distance = 1 - ((float)holds[a] / data.HoldInterval);
                            var increment = fast * (distance * distance * distance);
                            if (peakElements[a].Y < data.Height - increment)
                            {
                                peakElements[a].Y += (int)Math.Round(increment);
                            }
                            else if (peakElements[a].Y < data.Height - 1)
                            {
                                peakElements[a].Y = data.Height - 1;
                            }
                        }
                        holds[a] -= duration;
                    }
                    else if (peakElements[a].Y < data.Height - fast)
                    {
                        peakElements[a].Y += fast;
                    }
                    else if (peakElements[a].Y < data.Height - 1)
                    {
                        peakElements[a].Y = data.Height - 1;
                    }
                }
            }

            data.LastUpdated = DateTime.UtcNow;
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

        public static SpectrumRendererData Create(IOutput output, int width, int height, int[] bands, int fftSize, int holdInterval, int updateInterval, int smoothingFactor, bool showPeaks, bool showRms, bool showCrest)
        {
            var data = new SpectrumRendererData()
            {
                Output = output,
                Width = width,
                Height = height,
                Bands = bands,
                MinBand = bands[0],
                MaxBand = bands[bands.Length - 1],
                FFTSize = fftSize,
                HoldInterval = holdInterval,
                UpdateInterval = updateInterval,
                Smoothing = smoothingFactor,
                Samples = output.GetBuffer(fftSize),
                Values = new float[bands.Length],
                ValueElements = new Int32Rect[bands.Length]
            };
            if (showRms)
            {
                data.Rms = new float[bands.Length];
                data.RmsElements = new Int32Rect[bands.Length];
            }
            if (showPeaks)
            {
                data.PeakElements = new Int32Rect[bands.Length];
                data.Holds = new int[bands.Length];
            }
            if (showCrest)
            {
                data.CrestPoints = new Int32Point[bands.Length];
            }
            data.Step = width / bands.Length;
            return data;
        }

        public class SpectrumRendererData
        {
            public IOutput Output;

            public int[] Bands;

            public int MinBand;

            public int MaxBand;

            public int Rate;

            public int Depth;

            public int FFTSize;

            public float[] Samples;

            public float[] Values;

            public float ValuePeak;

            public float[] Rms;

            public float RmsPeak;

            public int Step;

            public int Width;

            public int Height;

            public Int32Rect[] ValueElements;

            public Int32Rect[] RmsElements;

            public Int32Point[] CrestPoints;

            public Int32Rect[] PeakElements;

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
