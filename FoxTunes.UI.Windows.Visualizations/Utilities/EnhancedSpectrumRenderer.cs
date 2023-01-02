using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FoxTunes
{
    public class EnhancedSpectrumRenderer : VisualizationBase
    {
        const int MARGIN = 1;

        public SpectrumRendererData RendererData { get; private set; }

        public SelectionConfigurationElement Bands { get; private set; }

        public BooleanConfigurationElement ShowPeaks { get; private set; }

        public BooleanConfigurationElement ShowRms { get; private set; }

        public BooleanConfigurationElement ShowCrestFactor { get; private set; }

        public IntegerConfigurationElement HoldInterval { get; private set; }

        public TextConfigurationElement ColorPalette { get; private set; }

        public SelectionConfigurationElement FFTSize { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            base.InitializeComponent(core);
            this.Bands = this.Configuration.GetElement<SelectionConfigurationElement>(
                EnhancedSpectrumBehaviourConfiguration.SECTION,
                EnhancedSpectrumBehaviourConfiguration.BANDS_ELEMENT
            );
            this.ShowPeaks = this.Configuration.GetElement<BooleanConfigurationElement>(
                EnhancedSpectrumBehaviourConfiguration.SECTION,
                EnhancedSpectrumBehaviourConfiguration.PEAKS_ELEMENT
             );
            this.HoldInterval = this.Configuration.GetElement<IntegerConfigurationElement>(
               EnhancedSpectrumBehaviourConfiguration.SECTION,
               EnhancedSpectrumBehaviourConfiguration.HOLD_ELEMENT
            );
            this.ShowRms = this.Configuration.GetElement<BooleanConfigurationElement>(
                EnhancedSpectrumBehaviourConfiguration.SECTION,
                EnhancedSpectrumBehaviourConfiguration.RMS_ELEMENT
             );
            this.ShowCrestFactor = this.Configuration.GetElement<BooleanConfigurationElement>(
                EnhancedSpectrumBehaviourConfiguration.SECTION,
                EnhancedSpectrumBehaviourConfiguration.CREST_ELEMENT
            );
            this.ColorPalette = this.Configuration.GetElement<TextConfigurationElement>(
                EnhancedSpectrumBehaviourConfiguration.SECTION,
                EnhancedSpectrumBehaviourConfiguration.COLOR_PALETTE_ELEMENT
            );
            this.FFTSize = this.Configuration.GetElement<SelectionConfigurationElement>(
               VisualizationBehaviourConfiguration.SECTION,
               VisualizationBehaviourConfiguration.FFT_SIZE_ELEMENT
            );
            this.Bands.ValueChanged += this.OnValueChanged;
            this.ShowPeaks.ValueChanged += this.OnValueChanged;
            this.ShowRms.ValueChanged += this.OnValueChanged;
            this.ShowCrestFactor.ValueChanged += this.OnValueChanged;
            this.ColorPalette.ValueChanged += this.OnValueChanged;
            this.FFTSize.ValueChanged += this.OnValueChanged;
            var task = this.CreateBitmap();
        }

        protected virtual void OnValueChanged(object sender, EventArgs e)
        {
            if (object.ReferenceEquals(sender, this.Bands))
            {
                //Changing bands requires full refresh.
                var task = this.CreateBitmap();
            }
            else
            {
                var task = this.CreateData();
            }
        }

        protected override bool CreateData(int width, int height)
        {
            this.RendererData = Create(
                this,
                width,
                height,
                EnhancedSpectrumBehaviourConfiguration.GetBands(this.Bands.Value),
                EnhancedSpectrumBehaviourConfiguration.GetFFTSize(this.FFTSize.Value, this.Bands.Value),
                this.ShowPeaks.Value,
                this.ShowRms.Value,
                this.ShowCrestFactor.Value,
                EnhancedSpectrumBehaviourConfiguration.GetColorPalette(this.ColorPalette.Value, this.Color)
            );
            return true;
        }

        protected virtual async Task Render(SpectrumRendererData data)
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
                if (bitmap == null)
                {
                    return;
                }

                success = bitmap.TryLock(LockTimeout);
                if (!success)
                {
                    return;
                }
                if (data.RmsElements != null)
                {
                    var colors = this.Color.ToPair(SHADE);
                    valueRenderInfo = BitmapHelper.CreateRenderInfo(bitmap, BitmapHelper.GetOrCreatePalette(0, colors[0]));
                    rmsRenderInfo = BitmapHelper.CreateRenderInfo(bitmap, BitmapHelper.GetOrCreatePalette(0, colors[1]));
                    if (data.CrestPoints != null)
                    {
                        crestRenderInfo = BitmapHelper.CreateRenderInfo(bitmap, BitmapHelper.GetOrCreatePalette(0, Colors.Red));
                    }
                }
                else
                {
                    valueRenderInfo = BitmapHelper.CreateRenderInfo(bitmap, BitmapHelper.GetOrCreatePalette(BitmapHelper.COLOR_FROM_Y, data.Colors));
                }
            }, DISPATCHER_PRIORITY).ConfigureAwait(false);

            if (!success)
            {
                //Failed to establish lock.
                this.Restart();
                return;
            }

            try
            {
                Render(valueRenderInfo, rmsRenderInfo, crestRenderInfo, data);
            }
            catch (Exception e)
            {
#if DEBUG
                Logger.Write(this.GetType(), LogLevel.Warn, "Failed to render spectrum: {0}", e.Message);
#else
                Logger.Write(this.GetType(), LogLevel.Warn, "Failed to render spectrum, disabling: {0}", e.Message);
                success = false;
#endif
            }

            await Windows.Invoke(() =>
            {
                bitmap.AddDirtyRect(new global::System.Windows.Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
                bitmap.Unlock();
            }, DISPATCHER_PRIORITY).ConfigureAwait(false);

            if (!success)
            {
                return;
            }
            this.Restart();
        }

        protected override void OnElapsed(object sender, ElapsedEventArgs e)
        {
            var data = this.RendererData;
            if (data == null)
            {
                this.Restart();
                return;
            }
            try
            {
                if (!data.Update())
                {
                    this.Restart();
                    return;
                }
                else
                {
                    UpdateValues(data);
                    if (!data.LastUpdated.Equals(default(DateTime)))
                    {
                        UpdateElementsSmooth(data);
                    }
                    else
                    {
                        UpdateElementsFast(data);
                    }
                    if (data.PeakElements != null)
                    {
                        UpdatePeaks(data, this.UpdateInterval, this.HoldInterval.Value);
                    }
                    data.LastUpdated = DateTime.UtcNow;
                }

                var task = this.Render(data);
            }
            catch (Exception exception)
            {
#if DEBUG
                Logger.Write(this.GetType(), LogLevel.Warn, "Failed to update spectrum data: {0}", exception.Message);
                this.Restart();
#else
                Logger.Write(this.GetType(), LogLevel.Warn, "Failed to update spectrum data, disabling: {0}", exception.Message);
#endif
            }
        }

        protected override int GetPixelWidth(double width)
        {
            var min = EnhancedSpectrumBehaviourConfiguration.GetWidth(this.Bands.Value);
            var bands = EnhancedSpectrumBehaviourConfiguration.GetBands(this.Bands.Value);
            return base.GetPixelWidth(Math.Max(bands.Length * (Convert.ToInt32(width) / bands.Length), min));
        }

        protected override void OnDisposing()
        {
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
            if (this.ColorPalette != null)
            {
                this.ColorPalette.ValueChanged -= this.OnValueChanged;
            }
            if (this.FFTSize != null)
            {
                this.FFTSize.ValueChanged -= this.OnValueChanged;
            }
            base.OnDisposing();
        }

        private static void Render(BitmapHelper.RenderInfo valueRenderInfo, BitmapHelper.RenderInfo rmsRenderInfo, BitmapHelper.RenderInfo crestRenderInfo, SpectrumRendererData data)
        {
            var valueElements = data.ValueElements;
            var rmsElements = data.RmsElements;
            var crestPoints = data.CrestPoints;
            var peakElements = data.PeakElements;

            BitmapHelper.Clear(ref valueRenderInfo);

            if (data.SampleCount == 0)
            {
                //No data.
                return;
            }

            if (valueRenderInfo.Width != data.Width || valueRenderInfo.Height != data.Height)
            {
                //Bitmap does not match data.
                return;
            }

            BitmapHelper.DrawRectangles(ref valueRenderInfo, valueElements, valueElements.Length);
            if (rmsElements != null)
            {
                BitmapHelper.DrawRectangles(ref rmsRenderInfo, rmsElements, rmsElements.Length);
            }

            if (peakElements != null)
            {
                for (var a = 0; a < valueElements.Length; a++)
                {
                    var max = valueElements[a].Y;
                    if (rmsElements != null)
                    {
                        max = Math.Max(max, rmsElements[a].Y);
                    }
                    if (peakElements[a].Y >= max)
                    {
                        continue;
                    }
                    BitmapHelper.DrawRectangle(
                        ref valueRenderInfo,
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
                        ref crestRenderInfo,
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
            var count = end - start;

            if (count > 0)
            {
                for (var a = start; a < end; a++)
                {
                    value = Math.Max(samples[a], value);
                    if (doRms)
                    {
                        rms += samples[a] * samples[a];
                    }
                }
            }
            else
            {
                //If we don't have data then average the closest available bins.
                if (start > 0)
                {
                    start--;
                }
                if (end < data.FFTSize)
                {
                    end++;
                }
                count = end - start;
                for (var a = start; a < end; a++)
                {
                    value += samples[a];
                    if (doRms)
                    {
                        rms += samples[a] * samples[a];
                    }
                }
                value /= count;
            }

            if (value > 0)
            {
                data.ValuePeak = Math.Max(data.Values[band] = ToDecibelFixed(value), data.ValuePeak);
            }
            else
            {
                data.Values[band] = 0;
            }

            if (doRms)
            {
                if (count > 0)
                {
                    data.RmsPeak = Math.Max(data.Rms[band] = ToDecibelFixed(Convert.ToSingle(Math.Sqrt(rms / count))), data.RmsPeak);
                }
                else
                {
                    data.Rms[band] = 0;
                }
            }
        }

        private static void UpdateElementsFast(SpectrumRendererData data)
        {
            UpdateElementsFast(data.Values, data.ValueElements, data.Width, data.Height, MARGIN, Orientation.Vertical);
            if (data.Rms != null && data.RmsElements != null)
            {
                UpdateElementsFast(data.Rms, data.RmsElements, data.Width, data.Height, MARGIN, Orientation.Vertical);
            }
            if (data.Rms != null && data.CrestPoints != null)
            {
                UpdateCrestPointsFast(data.Values, data.Rms, data.CrestPoints, data.Width, data.Height);
            }
        }

        private static void UpdatePeaks(SpectrumRendererData data, int updateInterval, int holdInterval)
        {
            if (data.RmsElements != null)
            {
                for (var a = 0; a < data.Peaks.Length; a++)
                {
                    data.Peaks[a] = Math.Max(data.ValueElements[a].Y, data.RmsElements[a].Y);
                }
            }
            else
            {
                for (var a = 0; a < data.Peaks.Length; a++)
                {
                    data.Peaks[a] = data.ValueElements[a].Y;
                }
            }
            var duration = Convert.ToInt32(
                Math.Min(
                    (DateTime.UtcNow - data.LastUpdated).TotalMilliseconds,
                    updateInterval * 100
                )
            );
            UpdateElementsSmooth(data.Peaks, data.PeakElements, data.Holds, data.Width, data.Height, MARGIN, holdInterval, duration, Orientation.Vertical);
        }

        private static void UpdateCrestPointsFast(float[] values, float[] rms, Int32Point[] elements, int width, int height)
        {
            if (values.Length == 0)
            {
                return;
            }
            height = height - 1;
            var step = width / values.Length;
            var offset = default(float);
            for (var a = 0; a < values.Length; a++)
            {
                offset = Math.Max(offset, values[a] - rms[a]);
            }
            for (var a = 0; a < values.Length; a++)
            {
                var x = (a * step) + (step / 2);
                var y = default(int);
                if (values[a] > 0)
                {
                    y = height - Convert.ToInt32(ToCrestFactor(values[a], rms[a], offset) * height);
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
            UpdateElementsSmooth(data.Values, data.ValueElements, data.Width, data.Height, MARGIN, Orientation.Vertical);
            if (data.Rms != null && data.RmsElements != null)
            {
                UpdateElementsSmooth(data.Rms, data.RmsElements, data.Width, data.Height, MARGIN, Orientation.Vertical);
            }
            if (data.Rms != null && data.CrestPoints != null)
            {
                UpdateCrestPointsSmooth(data.Values, data.Rms, data.CrestPoints, data.Width, data.Height);
            }
        }

        private static void UpdateCrestPointsSmooth(float[] values, float[] rms, Int32Point[] elements, int width, int height)
        {
            if (values.Length == 0)
            {
                return;
            }
            height = height - 1;
            var step = width / values.Length;
            var offset = default(float);
            var minChange = 1;
            var maxChange = Convert.ToInt32(height * 0.05f);
            for (var a = 0; a < values.Length; a++)
            {
                offset = Math.Max(offset, values[a] - rms[a]);
            }
            for (var a = 0; a < values.Length; a++)
            {
                var x = (a * step) + (step / 2);
                var y = default(int);
                if (values[a] > 0)
                {
                    y = height - Convert.ToInt32(ToCrestFactor(values[a], rms[a], offset) * height);
                }
                else
                {
                    y = height;
                }
                elements[a].X = x;
                Animate(ref elements[a].Y, y, 1, height, minChange, maxChange);
            }
        }

        public static SpectrumRendererData Create(EnhancedSpectrumRenderer renderer, int width, int height, int[] bands, int fftSize, bool showPeaks, bool showRms, bool showCrest, Color[] colors)
        {
            var data = new SpectrumRendererData()
            {
                Renderer = renderer,
                Width = width,
                Height = height,
                Bands = bands,
                MinBand = bands[0],
                MaxBand = bands[bands.Length - 1],
                FFTSize = fftSize,
                Samples = renderer.Output.GetBuffer(fftSize),
                Values = new float[bands.Length],
                Colors = colors,
                ValueElements = new Int32Rect[bands.Length]
            };
            if (showRms)
            {
                data.Rms = new float[bands.Length];
                data.RmsElements = new Int32Rect[bands.Length];
                if (showCrest)
                {
                    data.CrestPoints = new Int32Point[bands.Length];
                }
            }
            if (showPeaks)
            {
                data.Peaks = new int[bands.Length];
                data.Holds = new int[bands.Length];
                data.PeakElements = CreatePeaks(bands.Length);
            }
            return data;
        }

        private static Int32Rect[] CreatePeaks(int count)
        {
            var peaks = new Int32Rect[count];
            for (var a = 0; a < count; a++)
            {
                peaks[a].Y = int.MaxValue;
            }
            return peaks;
        }

        public class SpectrumRendererData
        {
            public EnhancedSpectrumRenderer Renderer;

            public int[] Bands;

            public int MinBand;

            public int MaxBand;

            public int Rate;

            public int Channels;

            public OutputStreamFormat Format;

            public int FFTSize;

            public float[] Samples;

            public int SampleCount;

            public float[] Values;

            public float ValuePeak;

            public float[] Rms;

            public float RmsPeak;

            public int Width;

            public int Height;

            public Color[] Colors;

            public Int32Rect[] ValueElements;

            public Int32Rect[] RmsElements;

            public Int32Point[] CrestPoints;

            public Int32Rect[] PeakElements;

            public int[] Peaks;

            public int[] Holds;

            public DateTime LastUpdated;

            public bool Update()
            {
                if (!this.Renderer.Output.GetDataFormat(out this.Rate, out this.Channels, out this.Format))
                {
                    return false;
                }
                this.SampleCount = this.Renderer.Output.GetData(this.Samples, this.FFTSize);
                return this.SampleCount > 0;
            }
        }
    }
}
