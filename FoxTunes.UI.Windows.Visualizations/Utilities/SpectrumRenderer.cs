using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace FoxTunes
{
    public class SpectrumRenderer : VisualizationBase
    {
        public SpectrumRendererData RendererData { get; private set; }

        public SelectionConfigurationElement Bars { get; private set; }

        public BooleanConfigurationElement ShowPeaks { get; private set; }

        public BooleanConfigurationElement HighCut { get; private set; }

        public BooleanConfigurationElement Smooth { get; private set; }

        public IntegerConfigurationElement HoldInterval { get; private set; }

        public SelectionConfigurationElement FFTSize { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            base.InitializeComponent(core);
            this.Bars = this.Configuration.GetElement<SelectionConfigurationElement>(
               SpectrumBehaviourConfiguration.SECTION,
               SpectrumBehaviourConfiguration.BARS_ELEMENT
            );
            this.ShowPeaks = this.Configuration.GetElement<BooleanConfigurationElement>(
                SpectrumBehaviourConfiguration.SECTION,
                SpectrumBehaviourConfiguration.PEAKS_ELEMENT
             );
            this.HighCut = this.Configuration.GetElement<BooleanConfigurationElement>(
                SpectrumBehaviourConfiguration.SECTION,
                SpectrumBehaviourConfiguration.HIGH_CUT_ELEMENT
            );
            this.Smooth = this.Configuration.GetElement<BooleanConfigurationElement>(
               VisualizationBehaviourConfiguration.SECTION,
               VisualizationBehaviourConfiguration.SMOOTH_ELEMENT
            );
            this.HoldInterval = this.Configuration.GetElement<IntegerConfigurationElement>(
               SpectrumBehaviourConfiguration.SECTION,
               SpectrumBehaviourConfiguration.HOLD_ELEMENT
            );
            this.Configuration.GetElement<IntegerConfigurationElement>(
               VisualizationBehaviourConfiguration.SECTION,
               VisualizationBehaviourConfiguration.INTERVAL_ELEMENT
            ).ConnectValue(value => this.UpdateInterval = value);
            this.FFTSize = this.Configuration.GetElement<SelectionConfigurationElement>(
               VisualizationBehaviourConfiguration.SECTION,
               VisualizationBehaviourConfiguration.FFT_SIZE_ELEMENT
            );
            this.Bars.ValueChanged += this.OnValueChanged;
            this.ShowPeaks.ValueChanged += this.OnValueChanged;
            this.HighCut.ValueChanged += this.OnValueChanged;
            this.Smooth.ValueChanged += this.OnValueChanged;
            this.HoldInterval.ValueChanged += this.OnValueChanged;
            this.FFTSize.ValueChanged += this.OnValueChanged;
            var task = this.CreateBitmap();
        }

        protected virtual void OnValueChanged(object sender, EventArgs e)
        {
            if (object.ReferenceEquals(sender, this.Bars))
            {
                //Changing bars requires full refresh.
                this.Debouncer.Exec(this.CreateBitmap);
            }
            else
            {
                this.Debouncer.Exec(this.CreateData);
            }
        }

        protected override bool CreateData(int width, int height)
        {
            this.RendererData = Create(
                this.Output,
                width,
                height,
                SpectrumBehaviourConfiguration.GetBars(this.Bars.Value),
                VisualizationBehaviourConfiguration.GetFFTSize(this.FFTSize.Value),
                this.ShowPeaks.Value,
                this.HighCut.Value
            );
            return true;
        }

        protected virtual async Task Render(SpectrumRendererData data)
        {
            var bitmap = default(WriteableBitmap);
            var success = default(bool);
            var info = default(BitmapHelper.RenderInfo);

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
                info = BitmapHelper.CreateRenderInfo(bitmap, this.Color);
            }, DISPATCHER_PRIORITY).ConfigureAwait(false);

            if (!success)
            {
                //Failed to establish lock.
                this.Restart();
                return;
            }

            try
            {
                Render(info, data);
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
                UpdateValues(data);
                if (this.Smooth.Value && !data.LastUpdated.Equals(default(DateTime)))
                {
                    UpdateElementsSmooth(data.Values, data.Elements, data.Width, data.Height, Orientation.Vertical);
                }
                else
                {
                    UpdateElementsFast(data.Values, data.Elements, data.Width, data.Height, Orientation.Vertical);
                }
                if (data.Peaks != null)
                {
                    var duration = Convert.ToInt32(
                        Math.Min(
                            (DateTime.UtcNow - data.LastUpdated).TotalMilliseconds,
                            this.UpdateInterval * 100
                        )
                    );
                    UpdateElementsSmooth(data.Elements, data.Peaks, data.Holds, data.Width, data.Height, this.HoldInterval.Value, duration, Orientation.Vertical);
                }
                data.LastUpdated = DateTime.UtcNow;

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
            var bars = SpectrumBehaviourConfiguration.GetBars(this.Bars.Value);
            return base.GetPixelWidth(bars * (Convert.ToInt32(width) / bars));
        }

        protected override void OnDisposing()
        {
            if (this.Bars != null)
            {
                this.Bars.ValueChanged -= this.OnValueChanged;
            }
            if (this.ShowPeaks != null)
            {
                this.ShowPeaks.ValueChanged -= this.OnValueChanged;
            }
            if (this.HighCut != null)
            {
                this.HighCut.ValueChanged -= this.OnValueChanged;
            }
            if (this.Smooth != null)
            {
                this.Smooth.ValueChanged -= this.OnValueChanged;
            }
            if (this.HoldInterval != null)
            {
                this.HoldInterval.ValueChanged -= this.OnValueChanged;
            }
            if (this.FFTSize != null)
            {
                this.FFTSize.ValueChanged -= this.OnValueChanged;
            }
            base.OnDisposing();
        }

        private static void Render(BitmapHelper.RenderInfo info, SpectrumRendererData data)
        {
            var elements = data.Elements;
            var peaks = data.Peaks;

            BitmapHelper.Clear(ref info);

            if (data.SampleCount == 0)
            {
                //No data.
                return;
            }

            if (info.Width != data.Width || info.Height != data.Height)
            {
                //Bitmap does not match data.
                return;
            }

            BitmapHelper.DrawRectangles(ref info, elements, elements.Length);

            for (var a = 0; a < elements.Length; a++)
            {
                if (peaks != null)
                {
                    if (peaks[a].Y >= elements[a].Y)
                    {
                        continue;
                    }
                    BitmapHelper.DrawRectangle(
                        ref info,
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

            var samplesPerValue = data.FFTRange / data.Count;

            if (samplesPerValue == 1)
            {
                for (int a = 0; a < data.FFTRange && a < values.Length; a++)
                {
                    var value = samples[a];
                    values[a] = ToDecibelFixed(value);
                }
            }
            else if (samplesPerValue > 1)
            {
                for (int a = 0, b = 0; a < data.FFTRange && b < values.Length; a += samplesPerValue, b++)
                {
                    var value = default(float);
                    for (var c = 0; c < samplesPerValue; c++)
                    {
                        value += samples[a + c];
                    }
                    values[b] = ToDecibelFixed(value / samplesPerValue);
                }
            }
            else
            {
                var valuesPerSample = (float)data.Count / data.FFTRange;
                for (var a = 0; a < data.Count; a++)
                {
                    var value = samples[Convert.ToInt32(a / valuesPerSample)];
                    values[a] = ToDecibelFixed(value);
                }
            }
        }

        public static SpectrumRendererData Create(IOutput output, int width, int height, int count, int fftSize, bool showPeaks, bool highCut)
        {
            if (count > width)
            {
                //Not enough space.
                return null;
            }

            var data = new SpectrumRendererData()
            {
                Output = output,
                Width = width,
                Height = height,
                Count = count,
                FFTSize = fftSize,
                Samples = output.GetBuffer(fftSize),
                Values = new float[count],
                Elements = new Int32Rect[count]
            };
            if (showPeaks)
            {
                data.Peaks = CreatePeaks(count);
                data.Holds = new int[count];
            }
            if (highCut)
            {
                data.FFTRange = data.Samples.Length - (data.Samples.Length / 4);
            }
            else
            {
                data.FFTRange = data.Samples.Length;
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
            public IOutput Output;

            public int FFTSize;

            public int FFTRange;

            public float[] Samples;

            public int SampleCount;

            public float[] Values;

            public int Width;

            public int Height;

            public int Count;

            public Int32Rect[] Elements;

            public Int32Rect[] Peaks;

            public int[] Holds;

            public DateTime LastUpdated;

            public bool Update()
            {
                this.SampleCount = this.Output.GetData(this.Samples, this.FFTSize);
                return this.SampleCount > 0;
            }
        }
    }
}
