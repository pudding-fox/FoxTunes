using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace FoxTunes
{
    public class SpectrogramRenderer : VisualizationBase
    {
        public SpectrogramRendererData RendererData { get; private set; }

        public SelectionConfigurationElement Mode { get; private set; }

        public SelectionConfigurationElement Scale { get; private set; }

        public IntegerConfigurationElement Smoothing { get; private set; }

        public TextConfigurationElement ColorPalette { get; private set; }

        public SelectionConfigurationElement FFTSize { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            base.InitializeComponent(core);
            this.Mode = this.Configuration.GetElement<SelectionConfigurationElement>(
                SpectrogramBehaviourConfiguration.SECTION,
                SpectrogramBehaviourConfiguration.MODE_ELEMENT
            );
            this.Scale = this.Configuration.GetElement<SelectionConfigurationElement>(
                SpectrogramBehaviourConfiguration.SECTION,
                SpectrogramBehaviourConfiguration.SCALE_ELEMENT
            );
            this.Smoothing = this.Configuration.GetElement<IntegerConfigurationElement>(
                SpectrogramBehaviourConfiguration.SECTION,
                SpectrogramBehaviourConfiguration.SMOOTHING_ELEMENT
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
            this.Scale.ValueChanged += this.OnValueChanged;
            this.Smoothing.ValueChanged += this.OnValueChanged;
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
                SpectrogramBehaviourConfiguration.GetMode(this.Mode.Value),
                SpectrogramBehaviourConfiguration.GetScale(this.Scale.Value),
                this.Smoothing.Value
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
                    SpectrogramBehaviourConfiguration.GetMode(this.Mode.Value),
                    SpectrogramBehaviourConfiguration.GetScale(this.Scale.Value),
                    this.Smoothing.Value
                );
            });
        }

        protected override WriteableBitmap CreateBitmap(Size size)
        {
            if (this.Bitmap != null)
            {
                return this.Bitmap.Resize(size);
            }
            return base.CreateBitmap(size);
        }

        protected virtual async Task Render(SpectrogramRendererData data)
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
            }, DISPATCHER_PRIORITY).ConfigureAwait(false);

            if (!success)
            {
                //Failed to establish lock.
                this.Start();
                return;
            }

            try
            {
                Render(info, data);
            }
            catch (Exception e)
            {
#if DEBUG
                Logger.Write(this.GetType(), LogLevel.Warn, "Failed to render spectrogram: {0}", e.Message);
#else
                Logger.Write(this.GetType(), LogLevel.Warn, "Failed to render spectrogram, disabling: {0}", e.Message);
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
                    this.Start();
                    return;
                }
                UpdateValues(data);

                var task = this.Render(data);
            }
            catch (Exception exception)
            {
#if DEBUG
                Logger.Write(this.GetType(), LogLevel.Warn, "Failed to update spectrogram data: {0}", exception.Message);
                this.Start();
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
            if (this.Scale != null)
            {
                this.Scale.ValueChanged -= this.OnValueChanged;
            }
            if (this.Smoothing != null)
            {
                this.Smoothing.ValueChanged -= this.OnValueChanged;
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

        private static void Render(BitmapHelper.RenderInfo info, SpectrogramRendererData data)
        {
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
                var value1 = values[0, h - y];
                var value2 = Convert.ToInt32(value1 * (colors.Length - 1));
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
                    var value1 = values[channel, h - y];
                    var value2 = Convert.ToInt32(value1 * (colors.Length - 1));
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
                    UpdateValuesMono(data.Width, data.Height, data.Samples, data.Values, data.Scale, data.Smoothing);
                    break;
                case SpectrogramRendererMode.Seperate:
                    UpdateValuesSeperate(data.Width, data.Height, data.Samples, data.Values, data.Channels, data.Scale, data.Smoothing);
                    break;
            }
        }

        private static void UpdateValuesMono(int width, int height, float[] samples, float[,] values, SpectrogramRendererScale scale, int smoothing)
        {
            var num1 = 0f;
            var num2 = 0f;
            var num3 = 0f;
            var num4 = 0f;
            var num5 = (float)(samples.Length - 1) / (height - 1);

            Array.Clear(values, 0, values.Length);

            for (var a = 0; a < samples.Length; a++)
            {
                switch (scale)
                {
                    default:
                    case SpectrogramRendererScale.Linear:
                        num1 = (float)a / num5;
                        break;
                    case SpectrogramRendererScale.Logarithmic:
                        num1 = (float)a / (samples.Length - 1);
                        num1 = Convert.ToSingle(1 - Math.Pow(1 - num1, 5));
                        num1 = num1 * (height - 1);
                        break;
                }
                num3 = ToDecibelFixed(samples[a]);
                num4 = Math.Max(num3, num4);
                num4 = Math.Min(num4, 1);
                num4 = Math.Max(num4, 0);
                if (num1 > num2)
                {
                    for (; num2 < num1; num2++)
                    {
                        values[0, Convert.ToInt32(num2)] = num4;
                    }
                    num4 = 0;
                }
            }
            if (smoothing > 0)
            {
                NoiseReduction(values, 1, height, smoothing);
            }
        }

        private static void UpdateValuesSeperate(int width, int height, float[] samples, float[,] values, int channels, SpectrogramRendererScale scale, int smoothing)
        {
            var num1 = 0f;
            var num2 = 0f;
            var num3 = 0f;
            var num4 = 0f;
            var num5 = (float)samples.Length / ((height / channels) - 1);

            Array.Clear(values, 0, values.Length);

            for (var channel = 0; channel < channels; channel++)
            {
                num2 = 0f;
                for (var a = channel; a < samples.Length; a += channels)
                {
                    switch (scale)
                    {
                        default:
                        case SpectrogramRendererScale.Linear:
                            num1 = (float)a / num5;
                            break;
                        case SpectrogramRendererScale.Logarithmic:
                            num1 = (float)a / (samples.Length - 1);
                            num1 = Convert.ToSingle(1 - Math.Pow(1 - num1, 5));
                            num1 = num1 * ((height / channels) - 1);
                            break;
                    }
                    num3 = ToDecibelFixed(samples[a]);
                    num4 = Math.Max(num3, num4);
                    num4 = Math.Max(num4, 0);
                    if (num1 > num2)
                    {
                        for (; num2 < num1; num2++)
                        {
                            values[channel, Convert.ToInt32(num2)] = num4;
                        }
                        num4 = 0;
                    }
                }
            }
            if (smoothing > 0)
            {
                NoiseReduction(values, channels, height, smoothing);
            }
        }

        public static SpectrogramRendererData Create(IOutput output, int width, int height, int fftSize, Color[] colors, SpectrogramRendererMode mode, SpectrogramRendererScale scale, int smoothing)
        {
            var data = new SpectrogramRendererData()
            {
                Output = output,
                Width = width,
                Height = height,
                FFTSize = fftSize,
                Colors = colors,
                Mode = mode,
                Scale = scale,
                Smoothing = smoothing
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

            public float[,] Values;

            public Color[] Colors;

            public SpectrogramRendererMode Mode;

            public SpectrogramRendererScale Scale;

            public int Smoothing;

            public bool Update()
            {
                var rate = default(int);
                var channels = default(int);
                var format = default(OutputStreamFormat);
                if (!this.Output.GetDataFormat(out rate, out channels, out format))
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
                        this.Values = new float[1, this.Height];
                        break;
                    case SpectrogramRendererMode.Seperate:
                        this.Samples = this.Output.GetBuffer(this.FFTSize, true);
                        this.Values = new float[this.Channels, this.Height];
                        break;
                }
            }
        }
    }

    public enum SpectrogramRendererMode : byte
    {
        None,
        Mono,
        Seperate
    }

    public enum SpectrogramRendererScale : byte
    {
        None,
        Linear,
        Logarithmic
    }
}
