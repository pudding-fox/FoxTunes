using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace FoxTunes
{
    public class SpectrumRenderer : VisualizationBase
    {
        const int SCALE_FACTOR = 4;

        public SpectrumRendererData RendererData { get; private set; }

        public SelectionConfigurationElement Bars { get; private set; }

        public BooleanConfigurationElement ShowPeaks { get; private set; }

        public BooleanConfigurationElement HighCut { get; private set; }

        public BooleanConfigurationElement Smooth { get; private set; }

        public IntegerConfigurationElement SmoothingFactor { get; private set; }

        public IntegerConfigurationElement HoldInterval { get; private set; }

        public SelectionConfigurationElement FFTSize { get; private set; }

        public IntegerConfigurationElement Amplitude { get; private set; }

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
            this.Configuration.GetElement<IntegerConfigurationElement>(
               SpectrumBehaviourConfiguration.SECTION,
               SpectrumBehaviourConfiguration.INTERVAL_ELEMENT
            ).ConnectValue(value => this.UpdateInterval = value);
            this.FFTSize = this.Configuration.GetElement<SelectionConfigurationElement>(
               SpectrumBehaviourConfiguration.SECTION,
               SpectrumBehaviourConfiguration.FFT_SIZE_ELEMENT
            );
            this.Amplitude = this.Configuration.GetElement<IntegerConfigurationElement>(
               SpectrumBehaviourConfiguration.SECTION,
               SpectrumBehaviourConfiguration.AMPLITUDE_ELEMENT
            );
            this.ScalingFactor.ValueChanged += this.OnValueChanged;
            this.Bars.ValueChanged += this.OnValueChanged;
            this.ShowPeaks.ValueChanged += this.OnValueChanged;
            this.HighCut.ValueChanged += this.OnValueChanged;
            this.Smooth.ValueChanged += this.OnValueChanged;
            this.SmoothingFactor.ValueChanged += this.OnValueChanged;
            this.HoldInterval.ValueChanged += this.OnValueChanged;
            this.FFTSize.ValueChanged += this.OnValueChanged;
            this.Amplitude.ValueChanged += this.OnValueChanged;
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

        protected virtual void OnValueChanged(object sender, EventArgs e)
        {
            if (object.ReferenceEquals(sender, this.Bars))
            {
                //Changing bars requires full refresh.
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
                SpectrumBehaviourConfiguration.GetBars(this.Bars.Value),
                SpectrumBehaviourConfiguration.GetFFTSize(this.FFTSize.Value),
                this.Amplitude.Value,
                this.ShowPeaks.Value,
                this.HighCut.Value
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
                    SpectrumBehaviourConfiguration.GetBars(this.Bars.Value),
                    SpectrumBehaviourConfiguration.GetFFTSize(this.FFTSize.Value),
                    this.Amplitude.Value,
                    this.ShowPeaks.Value,
                    this.HighCut.Value
                );
            });
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

        protected override void OnElapsed(object sender, ElapsedEventArgs e)
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
                    data.Clear();
                }
                UpdateValues(data);
                if (this.Smooth.Value)
                {
                    UpdateElementsSmooth(data.Values, data.Elements, data.Width, data.Height, this.SmoothingFactor.Value, Orientation.Vertical);
                }
                else
                {
                    UpdateElementsFast(data.Values, data.Elements, data.Width, data.Height, Orientation.Vertical);
                }
                if (this.ShowPeaks.Value)
                {
                    var duration = Convert.ToInt32(
                        Math.Min(
                            (DateTime.UtcNow - data.LastUpdated).TotalMilliseconds,
                            this.UpdateInterval * 100
                        )
                    );
                    UpdatePeaks(data.Elements, data.Peaks, data.Holds, data.Width, data.Height, this.HoldInterval.Value, duration, Orientation.Vertical);
                }

                data.LastUpdated = DateTime.UtcNow;

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
            return this.RendererData.Count * this.RendererData.Step;
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
            if (this.SmoothingFactor != null)
            {
                this.SmoothingFactor.ValueChanged -= this.OnValueChanged;
            }
            if (this.HoldInterval != null)
            {
                this.HoldInterval.ValueChanged -= this.OnValueChanged;
            }
            if (this.FFTSize != null)
            {
                this.FFTSize.ValueChanged -= this.OnValueChanged;
            }
            if (this.Amplitude != null)
            {
                this.Amplitude.ValueChanged -= this.OnValueChanged;
            }
        }

        private static void Render(BitmapHelper.RenderInfo info, SpectrumRendererData rendererData)
        {
            var elements = rendererData.Elements;
            var peaks = rendererData.Peaks;

            BitmapHelper.Clear(info);

            for (var a = 0; a < rendererData.Count; a++)
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

            if (data.SamplesPerElement > 1)
            {
                for (int a = 0, b = 0; a < data.FFTRange && b < values.Length; a += data.SamplesPerElement, b++)
                {
                    var value = 0.0f;
                    for (var c = 0; c < data.SamplesPerElement; c++)
                    {
                        var boost = (float)(1.0f + a * data.Amplitude);
                        value += (float)(Math.Sqrt(samples[a + c] * boost) * SCALE_FACTOR);
                    }
                    values[b] = Math.Min(Math.Max(value / data.SamplesPerElement, 0), 1);
                }
            }
            else
            {
                //Not enough samples to fill the values, do the best we can.
                for (int a = 0; a < data.Count; a++)
                {
                    var boost = (float)(1.0f + a * data.Amplitude);
                    var value = (float)(Math.Sqrt(samples[a] * boost) * SCALE_FACTOR);
                    data.Values[a] = Math.Min(Math.Max(value, 0), 1);
                }
            }
        }

        public static SpectrumRendererData Create(IOutput output, int width, int height, int count, int fftSize, int amplitude, bool showPeaks, bool highCut)
        {
            var data = new SpectrumRendererData()
            {
                Output = output,
                Width = width,
                Height = height,
                Count = count,
                FFTSize = fftSize,
                Amplitude = (float)amplitude / 500,
                Samples = output.GetBuffer(fftSize),
                Values = new float[count],
                Elements = new Int32Rect[count]
            };
            if (showPeaks)
            {
                data.Peaks = new Int32Rect[count];
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
            data.SamplesPerElement = Math.Max(data.FFTRange / count, 1);
            data.Step = width / count;
            return data;
        }

        public class SpectrumRendererData
        {
            public IOutput Output;

            public int FFTSize;

            public int FFTRange;

            public float Amplitude;

            public float[] Samples;

            public float[] Values;

            public int SamplesPerElement;

            public int Step;

            public int Width;

            public int Height;

            public int Count;

            public Int32Rect[] Elements;

            public Int32Rect[] Peaks;

            public int[] Holds;

            public DateTime LastUpdated;

            public bool Update()
            {
                return this.Output.GetData(this.Samples, this.FFTSize) > 0;
            }

            public void Clear()
            {
                Array.Clear(this.Samples, 0, this.Samples.Length);
            }
        }
    }
}
