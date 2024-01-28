using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FoxTunes
{
    public class SpectrogramRenderer : VisualizationBase
    {
        const double FACTOR = 262140; //4.0 * 65535.0

        public SpectrogramRendererData RendererData { get; private set; }

        public SelectionConfigurationElement Mode { get; private set; }

        public TextConfigurationElement ColorPalette { get; private set; }

        public SelectionConfigurationElement FFTSize { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            base.InitializeComponent(core);
            this.Mode = this.Configuration.GetElement<SelectionConfigurationElement>(
                SpectrogramBehaviourConfiguration.SECTION,
                SpectrogramBehaviourConfiguration.MODE_ELEMENT
            );
            this.ColorPalette = this.Configuration.GetElement<TextConfigurationElement>(
                SpectrogramBehaviourConfiguration.SECTION,
                SpectrogramBehaviourConfiguration.COLOR_PALETTE_ELEMENT
            );
            this.Configuration.GetElement<IntegerConfigurationElement>(
               VisualizationBehaviourConfiguration.SECTION,
               VisualizationBehaviourConfiguration.INTERVAL_ELEMENT
            ).ConnectValue(value => this.UpdateInterval = value);
            this.FFTSize = this.Configuration.GetElement<SelectionConfigurationElement>(
               VisualizationBehaviourConfiguration.SECTION,
               VisualizationBehaviourConfiguration.FFT_SIZE_ELEMENT
            );
            this.Mode.ValueChanged += this.OnValueChanged;
            this.ColorPalette.ValueChanged += this.OnValueChanged;
            this.FFTSize.ValueChanged += this.OnValueChanged;
            var task = this.CreateBitmap();
        }

        protected virtual void OnValueChanged(object sender, EventArgs e)
        {
            var task = this.RefreshBitmap();
        }

        protected override void CreateViewBox()
        {
            var bitmap = this.Bitmap;
            if (bitmap == null)
            {
                return;
            }
            this.RendererData = Create(
                this.Output,
                bitmap.PixelWidth,
                bitmap.PixelHeight,
                VisualizationBehaviourConfiguration.GetFFTSize(this.FFTSize.Value),
                SpectrogramBehaviourConfiguration.GetColorPalette(this.ColorPalette.Value),
                SpectrogramBehaviourConfiguration.GetMode(this.Mode.Value)
            );
            this.Viewbox = new Rect(0, 0, this.RendererData.Width, this.RendererData.Height);
        }

        protected virtual Task RefreshBitmap()
        {
            return Windows.Invoke(() =>
            {
                var bitmap = this.Bitmap;
                if (bitmap == null)
                {
                    return;
                }
                this.RendererData = Create(
                    this.Output,
                    bitmap.PixelWidth,
                    bitmap.PixelHeight,
                    VisualizationBehaviourConfiguration.GetFFTSize(this.FFTSize.Value),
                    SpectrogramBehaviourConfiguration.GetColorPalette(this.ColorPalette.Value),
                    SpectrogramBehaviourConfiguration.GetMode(this.Mode.Value)
                );
            });
        }

        protected virtual async Task Render()
        {
            if (!PlaybackStateNotifier.IsPlaying)
            {
                this.Start();
                return;
            }

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
                if (!object.ReferenceEquals(this.Bitmap, bitmap))
                {
                    return;
                }

                bitmap.AddDirtyRect(new global::System.Windows.Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
                bitmap.Unlock();
            }).ConfigureAwait(false);

            this.Start();
        }

        protected override void OnElapsed(object sender, ElapsedEventArgs e)
        {
            var data = this.RendererData;
            if (data == null)
            {
                this.Start();
                return;
            }
            try
            {
                if (!data.Update())
                {
                    data.Clear();
                }
                UpdateValues(data);

                var task = this.Render();
            }
            catch (Exception exception)
            {
#if DEBUG
                Logger.Write(this.GetType(), LogLevel.Warn, "Failed to update spectrogram data: {0}", exception.Message);
                var task = this.Render();
#else
                Logger.Write(this.GetType(), LogLevel.Warn, "Failed to update spectrogram data, disabling: {0}", exception.Message);
#endif
            }
        }

        protected override Freezable CreateInstanceCore()
        {
            return new SpectrogramRenderer();
        }

        protected override void OnDisposing()
        {
            if (this.Mode != null)
            {
                this.Mode.ValueChanged -= this.OnValueChanged;
            }
            if (this.FFTSize != null)
            {
                this.FFTSize.ValueChanged -= this.OnValueChanged;
            }
            base.OnDisposing();
        }

        private static void Render(BitmapHelper.RenderInfo info, SpectrogramRendererData data)
        {
            if (data.SampleCount == 0)
            {
                //No data.
                return;
            }

            BitmapHelper.ShiftLeft(info, 1);

            switch (data.Mode)
            {
                default:
                case SpectrogramRendererMode.Mono:
                    RenderMono(info, data);
                    break;
                case SpectrogramRendererMode.Seperate:
                    RenderSeperate(info, data);
                    break;
            }
        }

        private static void RenderMono(BitmapHelper.RenderInfo info, SpectrogramRendererData data)
        {
            var x = data.Width - 1;
            var h = data.Height - 1;
            var values = data.Values;
            var colors = data.Colors;
            for (var y = 0; y < data.Height; y++)
            {
                var value1 = (double)values[0, h - y] / FACTOR;
                var value2 = Convert.ToInt32(value1 * colors.Length);
                var color = colors[value2];
                info.Red = color.R;
                info.Green = color.G;
                info.Blue = color.B;
                BitmapHelper.DrawDot(info, x, y);
            }
        }

        private static void RenderSeperate(BitmapHelper.RenderInfo info, SpectrogramRendererData data)
        {
            var x = data.Width - 1;
            var h = (data.Height - 1) / data.Channels;
            var values = data.Values;
            var colors = data.Colors;
            for (var channel = 0; channel < data.Channels; channel++)
            {
                var offset = channel * h;
                for (var y = 0; y < h; y++)
                {
                    var value1 = (double)values[channel, h - y] / FACTOR;
                    var value2 = Convert.ToInt32(value1 * colors.Length);
                    var color = colors[value2];
                    info.Red = color.R;
                    info.Green = color.G;
                    info.Blue = color.B;
                    BitmapHelper.DrawDot(info, x, offset + y);
                }
            }
        }

        private static void UpdateValues(SpectrogramRendererData data)
        {
            switch (data.Mode)
            {
                default:
                case SpectrogramRendererMode.Mono:
                    UpdateValuesMono(data.Width, data.Height, data.Samples, data.Values);
                    break;
                case SpectrogramRendererMode.Seperate:
                    UpdateValuesSeperate(data.Width, data.Height, data.Samples, data.Values, data.Channels);
                    break;
            }
        }

        private static void UpdateValuesMono(int width, int height, float[] samples, int[,] values)
        {
            var num1 = 0f;
            var num2 = 0f;
            var num3 = 0;
            var num4 = 0;
            var num5 = (double)samples.Length / height;

            Array.Clear(values, 0, values.Length);

            for (var a = 1; a < samples.Length; a++)
            {
                num3 = (int)(Math.Sqrt(samples[a]) * FACTOR);
                num4 = Math.Max(num3, num4);
                num4 = Math.Max(num4, 0);
                num1 = (float)Math.Round((double)a / num5) - 1f;
                if (num1 > num2)
                {
                    values[0, Convert.ToInt32(num2)] = num4;
                    num2 = num1;
                    num4 = 0;
                }
            }
        }

        private static void UpdateValuesSeperate(int width, int height, float[] samples, int[,] values, int channels)
        {
            var num1 = 0f;
            var num2 = 0f;
            var num3 = 0;
            var num4 = 0;
            var num5 = (double)samples.Length / height;

            Array.Clear(values, 0, values.Length);

            for (var channel = 0; channel < channels; channel++)
            {
                num2 = 0f;
                for (var a = channel; a < samples.Length; a += channels)
                {
                    num3 = (int)(Math.Sqrt(samples[a]) * FACTOR);
                    num4 = Math.Max(num3, num4);
                    num4 = Math.Max(num4, 0);
                    num1 = (float)Math.Round((double)a / num5) - 1f;
                    if (num1 > num2)
                    {
                        values[channel, Convert.ToInt32(num2)] = num4;
                        num2 = num1;
                        num4 = 0;
                    }
                }
            }
        }

        public static SpectrogramRendererData Create(IOutput output, int width, int height, int fftSize, Color[] colors, SpectrogramRendererMode mode)
        {
            var data = new SpectrogramRendererData()
            {
                Output = output,
                Width = width,
                Height = height,
                FFTSize = fftSize,
                Colors = colors,
                Mode = mode
            };
            return data;
        }

        public class SpectrogramRendererData
        {
            public IOutput Output;

            public int Width;

            public int Height;

            public int Rate;

            public int Channels;

            public OutputStreamFormat Format;

            public int FFTSize;

            public float[] Samples;

            public int SampleCount;

            public int[,] Values;

            public Color[] Colors;

            public SpectrogramRendererMode Mode;

            public bool Update()
            {
                var rate = default(int);
                var channels = default(int);
                var format = default(OutputStreamFormat);
                if (!this.Output.GetFormat(out rate, out channels, out format))
                {
                    return false;
                }
                this.Update(rate, channels, format);
                var individual = default(bool);
                switch (this.Mode)
                {
                    default:
                    case SpectrogramRendererMode.Mono:
                        individual = false;
                        break;
                    case SpectrogramRendererMode.Seperate:
                        individual = true;
                        break;
                }
                this.SampleCount = this.Output.GetData(this.Samples, this.FFTSize, individual);
                return this.SampleCount > 0;
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

                //TODO: Only realloc if required.
                switch (this.Mode)
                {
                    default:
                    case SpectrogramRendererMode.Mono:
                        this.Samples = this.Output.GetBuffer(this.FFTSize, false);
                        this.Values = new int[1, this.Height];
                        break;
                    case SpectrogramRendererMode.Seperate:
                        this.Samples = this.Output.GetBuffer(this.FFTSize, true);
                        this.Values = new int[this.Channels, this.Height];
                        break;
                }
            }

            public void Clear()
            {
                Array.Clear(this.Samples, 0, this.Samples.Length);
                this.SampleCount = 0;
            }
        }
    }

    public enum SpectrogramRendererMode : byte
    {
        None,
        Mono,
        Seperate
    }
}
