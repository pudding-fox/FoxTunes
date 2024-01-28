using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace FoxTunes
{
    public class PeakRenderer : VisualizationBase
    {
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
            "Orientation",
            typeof(Orientation),
            typeof(PeakRenderer),
            new FrameworkPropertyMetadata(Orientation.Horizontal, new PropertyChangedCallback(OnOrientationChanged))
        );

        public static Orientation GetOrientation(PeakRenderer source)
        {
            return (Orientation)source.GetValue(OrientationProperty);
        }

        public static void SetOrientation(PeakRenderer source, Orientation value)
        {
            source.SetValue(OrientationProperty, value);
        }

        public static void OnOrientationChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var renderer = sender as PeakRenderer;
            if (renderer == null)
            {
                return;
            }
            renderer.OnOrientationChanged();
        }

        public PeakRendererData RendererData { get; private set; }

        public BooleanConfigurationElement ShowPeaks { get; private set; }

        public IntegerConfigurationElement HoldInterval { get; private set; }

        public BooleanConfigurationElement ShowRms { get; private set; }

        public Orientation Orientation
        {
            get
            {
                return (Orientation)this.GetValue(OrientationProperty);
            }
            set
            {
                this.SetValue(OrientationProperty, value);
            }
        }

        protected virtual void OnOrientationChanged()
        {
            var data = this.RendererData;
            if (data != null)
            {
                data.Orientation = this.Orientation;
            }
            if (this.OrientationChanged != null)
            {
                this.OrientationChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Orientation");
        }

        public event EventHandler OrientationChanged;

        public override void InitializeComponent(ICore core)
        {
            base.InitializeComponent(core);
            this.ShowPeaks = this.Configuration.GetElement<BooleanConfigurationElement>(
                PeakMeterBehaviourConfiguration.SECTION,
                PeakMeterBehaviourConfiguration.PEAKS
             );
            this.HoldInterval = this.Configuration.GetElement<IntegerConfigurationElement>(
               PeakMeterBehaviourConfiguration.SECTION,
               PeakMeterBehaviourConfiguration.HOLD
            );
            this.ShowRms = this.Configuration.GetElement<BooleanConfigurationElement>(
                PeakMeterBehaviourConfiguration.SECTION,
                PeakMeterBehaviourConfiguration.RMS
            );
            this.ShowPeaks.ValueChanged += this.OnValueChanged;
            this.ShowRms.ValueChanged += this.OnValueChanged;
            var task = this.CreateBitmap();
        }

        protected virtual void OnValueChanged(object sender, EventArgs e)
        {
            var task = this.CreateData();
        }

        protected override bool CreateData(int width, int height)
        {
            this.RendererData = Create(
                this,
                width,
                height,
                this.Orientation
            );
            return true;
        }

        protected virtual async Task Render(PeakRendererData data)
        {
            const byte SHADE = 30;

            var bitmap = default(WriteableBitmap);
            var success = default(bool);
            var valueRenderInfo = default(BitmapHelper.RenderInfo);
            var rmsRenderInfo = default(BitmapHelper.RenderInfo);

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
                }
                else
                {
                    valueRenderInfo = BitmapHelper.CreateRenderInfo(bitmap, BitmapHelper.GetOrCreatePalette(0, this.Color));
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
                Render(valueRenderInfo, rmsRenderInfo, data);
            }
            catch (Exception e)
            {
#if DEBUG
                Logger.Write(this.GetType(), LogLevel.Warn, "Failed to render peaks: {0}", e.Message);
#else
                Logger.Write(this.GetType(), LogLevel.Warn, "Failed to render peaks, disabling: {0}", e.Message);
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
                    var duration = Convert.ToInt32(
                        Math.Min(
                            (DateTime.UtcNow - data.LastUpdated).TotalMilliseconds,
                            this.UpdateInterval * 100
                        )
                    );
                    if (data.RmsElements != null)
                    {
                        var elements = new[]
                        {
                            data.ValueElements,
                            data.RmsElements
                        };
                        UpdateElementsSmooth(elements, data.PeakElements, data.Holds, data.Width, data.Height, this.HoldInterval.Value, duration, data.Orientation);
                    }
                    else
                    {
                        UpdateElementsSmooth(data.ValueElements, data.PeakElements, data.Holds, data.Width, data.Height, this.HoldInterval.Value, duration, data.Orientation);
                    }
                }
                data.LastUpdated = DateTime.UtcNow;

                var task = this.Render(data);
            }
            catch (Exception exception)
            {
#if DEBUG
                Logger.Write(this.GetType(), LogLevel.Warn, "Failed to update peak data: {0}", exception.Message);
                this.Restart();
#else
                Logger.Write(this.GetType(), LogLevel.Warn, "Failed to update peak data, disabling: {0}", exception.Message);
#endif
            }
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
            base.OnDisposing();
        }

        private static void Render(BitmapHelper.RenderInfo valueRenderInfo, BitmapHelper.RenderInfo rmsRenderInfo, PeakRendererData data)
        {
            var valueElements = data.ValueElements;
            var rmsElements = data.RmsElements;
            var peakElements = data.PeakElements;
            var orientation = data.Orientation;

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
                    if (orientation == Orientation.Horizontal)
                    {
                        var min = valueElements[a].Width;
                        if (rmsElements != null)
                        {
                            min = Math.Min(min, rmsElements[a].Width);
                        }
                        if (peakElements[a].X > min)
                        {
                            BitmapHelper.DrawRectangle(
                                ref valueRenderInfo,
                                peakElements[a].X,
                                peakElements[a].Y,
                                peakElements[a].Width,
                                peakElements[a].Height
                            );
                        }
                    }
                    else if (orientation == Orientation.Vertical)
                    {
                        var max = valueElements[a].Y;
                        if (rmsElements != null)
                        {
                            max = Math.Max(max, rmsElements[a].Y);
                        }
                        if (peakElements[a].Y < max)
                        {
                            BitmapHelper.DrawRectangle(
                                ref valueRenderInfo,
                                peakElements[a].X,
                                peakElements[a].Y,
                                peakElements[a].Width,
                                peakElements[a].Height
                            );
                        }
                    }
                }
            }
        }

        private static void UpdateValues(PeakRendererData data)
        {
            UpdateValues(data.Samples, data.Values, data.Rms, data.Channels, data.SampleCount);
        }

        private static void UpdateValues(float[,] samples, float[] values, float[] rms, int channels, int count)
        {
            var doRms = rms != null;
            for (var channel = 0; channel < channels; channel++)
            {
                values[channel] = 0;
                if (doRms)
                {
                    rms[channel] = 0;
                }
            }

            if (count == 0)
            {
                return;
            }

            for (var position = 0; position < count; position += channels)
            {
                for (var channel = 0; channel < channels; channel++)
                {
                    var value = samples[channel, position];
                    values[channel] = Math.Max(value, values[channel]);
                    if (doRms)
                    {
                        rms[channel] += value * value;
                    }
                }
            }

            for (var channel = 0; channel < channels; channel++)
            {
                values[channel] = ToDecibelFixed(values[channel]);
            }

            if (doRms)
            {
                for (var channel = 0; channel < channels; channel++)
                {
                    rms[channel] = ToDecibelFixed(Convert.ToSingle(Math.Sqrt(rms[channel] / (count / channels))));
                }
            }
        }

        private static void UpdateElementsFast(PeakRendererData data)
        {
            UpdateElementsFast(data.Values, data.ValueElements, data.Width, data.Height, data.Orientation);
            if (data.Rms != null && data.RmsElements != null)
            {
                UpdateElementsFast(data.Rms, data.RmsElements, data.Width, data.Height, data.Orientation);
            }
        }

        private static void UpdateElementsSmooth(PeakRendererData data)
        {
            UpdateElementsSmooth(data.Values, data.ValueElements, data.Width, data.Height, data.Orientation);
            if (data.Rms != null && data.RmsElements != null)
            {
                UpdateElementsSmooth(data.Rms, data.RmsElements, data.Width, data.Height, data.Orientation);
            }
        }

        public static PeakRendererData Create(PeakRenderer renderer, int width, int height, Orientation orientation)
        {
            var data = new PeakRendererData()
            {
                Renderer = renderer,
                Width = width,
                Height = height,
                Orientation = orientation
            };
            return data;
        }

        public class PeakRendererData
        {
            public PeakRenderer Renderer;

            public int Width;

            public int Height;

            public Orientation Orientation;

            public int Rate;

            public int Channels;

            public OutputStreamFormat Format;

            public short[] Samples16;

            public float[] Samples32;

            public float[,] Samples;

            public int SampleCount;

            public float[] Values;

            public float[] Rms;

            public Int32Rect[] ValueElements;

            public Int32Rect[] RmsElements;

            public Int32Rect[] PeakElements;

            public int[] Holds;

            public DateTime LastUpdated;

            public bool Update()
            {
                var rate = default(int);
                var channels = default(int);
                var format = default(OutputStreamFormat);
                if (!this.Renderer.Output.GetDataFormat(out rate, out channels, out format))
                {
                    return false;
                }
                this.Update(rate, channels, format);
                switch (this.Format)
                {
                    case OutputStreamFormat.Short:
                        this.SampleCount = this.Renderer.Output.GetData(this.Samples16);
                        for (var a = 0; a < this.SampleCount; a++)
                        {
                            this.Samples32[a] = (float)this.Samples16[a] / short.MaxValue;
                        }
                        break;
                    case OutputStreamFormat.Float:
                        this.SampleCount = this.Renderer.Output.GetData(this.Samples32);
                        break;
                    default:
                        throw new NotImplementedException();
                }
                if (this.Rate > 0 && this.Channels > 0 && this.SampleCount > 0)
                {
                    this.SampleCount = Deinterlace(this.Samples, this.Samples32, this.Channels, this.SampleCount);
                    return true;
                }
                return false;
            }

            private void Update(int rate, int channels, OutputStreamFormat format)
            {
                if (this.Rate == rate && this.Channels == channels && this.Format == format)
                {
                    return;
                }

                this.Rate = rate;
                this.Channels = channels;
                this.Format = format;

                switch (format)
                {
                    case OutputStreamFormat.Short:
                        this.Samples16 = this.Renderer.Output.GetBuffer<short>(TimeSpan.FromMilliseconds(this.Renderer.UpdateInterval));
                        this.Samples32 = new float[this.Samples16.Length];
                        break;
                    case OutputStreamFormat.Float:
                        this.Samples32 = this.Renderer.Output.GetBuffer<float>(TimeSpan.FromMilliseconds(this.Renderer.UpdateInterval));
                        break;
                    default:
                        throw new NotImplementedException();
                }

                this.Samples = new float[this.Channels, this.Samples32.Length];

                this.Values = new float[channels];
                this.ValueElements = new Int32Rect[channels];

                if (this.Renderer.ShowRms.Value)
                {
                    this.Rms = new float[channels];
                    this.RmsElements = new Int32Rect[channels];
                }
                if (this.Renderer.ShowPeaks.Value)
                {
                    this.PeakElements = CreatePeaks(channels);
                    this.Holds = new int[channels];
                }
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
        }
    }
}
