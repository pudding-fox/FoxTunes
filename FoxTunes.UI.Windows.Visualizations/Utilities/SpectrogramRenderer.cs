using FoxTunes.Interfaces;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FoxTunes
{
    public class SpectrogramRenderer : VisualizationBase
    {
        public SpectrogramRendererData RendererData { get; private set; }

        public SelectionConfigurationElement Mode { get; private set; }

        public SelectionConfigurationElement Scale { get; private set; }

        public IntegerConfigurationElement Smoothing { get; private set; }

        public TextConfigurationElement ColorPalette { get; private set; }

        public IntegerConfigurationElement History { get; private set; }

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
            this.History = this.Configuration.GetElement<IntegerConfigurationElement>(
               VisualizationBehaviourConfiguration.SECTION,
               SpectrogramBehaviourConfiguration.HISTORY_ELEMENT
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
            this.History.ValueChanged += this.OnValueChanged;
            this.FFTSize.ValueChanged += this.OnValueChanged;
            var task = this.CreateBitmap();
        }

        protected virtual void OnValueChanged(object sender, EventArgs e)
        {
            this.Debouncer.Exec(this.CreateData);
        }

        protected override bool CreateData(int width, int height)
        {
            var data = this.RendererData;
            if (data != null)
            {
                lock (data)
                {
                    data.Width = width;
                    data.Height = height;
                    data.FFTSize = VisualizationBehaviourConfiguration.GetFFTSize(this.FFTSize.Value);
                    data.Colors = SpectrogramBehaviourConfiguration.GetColorPalette(this.ColorPalette.Value);
                    data.Mode = SpectrogramBehaviourConfiguration.GetMode(this.Mode.Value);
                    data.Scale = SpectrogramBehaviourConfiguration.GetScale(this.Scale.Value);
                    data.Smoothing = this.Smoothing.Value;
                    data.HistoryCapacity = this.History.Value;
                    data.Initialized = false;
                }
            }
            else
            {
                this.RendererData = Create(
                    this.Output,
                    width,
                    height,
                    VisualizationBehaviourConfiguration.GetFFTSize(this.FFTSize.Value),
                    SpectrogramBehaviourConfiguration.GetColorPalette(this.ColorPalette.Value),
                    SpectrogramBehaviourConfiguration.GetMode(this.Mode.Value),
                    SpectrogramBehaviourConfiguration.GetScale(this.Scale.Value),
                    this.Smoothing.Value,
                    this.History.Value
                );
            }
            return true;
        }

        protected override WriteableBitmap CreateBitmap(int width, int height)
        {
            var bitmap = base.CreateBitmap(width, height);
            var data = this.RendererData;
            if (data != null && data.HistoryCount > 0)
            {
                try
                {
                    if (bitmap.TryLock(LockTimeout))
                    {
                        var info = BitmapHelper.CreateRenderInfo(bitmap, this.Color);
                        lock (data)
                        {
                            Restore(info, data);
                        }
                        bitmap.AddDirtyRect(new global::System.Windows.Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
                        bitmap.Unlock();
                    }
                }
                catch (Exception e)
                {
                    Logger.Write(this, LogLevel.Error, "Failed to restore spectrogram from history: {0}", e.Message);
                }
            }
            return bitmap;
        }

        protected virtual async Task Render(SpectrogramRendererData data)
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
                lock (data)
                {
                    Render(info, data);
                }
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
                lock (data)
                {
                    if (!data.Update())
                    {
                        this.Restart();
                        return;
                    }
                    UpdateValues(data);
                    UpdateElements(data);
                }

                var task = this.Render(data);
            }
            catch (Exception exception)
            {
#if DEBUG
                Logger.Write(this.GetType(), LogLevel.Warn, "Failed to update spectrogram data: {0}", exception.Message);
                this.Restart();
#else
                Logger.Write(this.GetType(), LogLevel.Warn, "Failed to update spectrogram data, disabling: {0}", exception.Message);
#endif
            }
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
            if (this.History != null)
            {
                this.History.ValueChanged -= this.OnValueChanged;
            }
            if (this.FFTSize != null)
            {
                this.FFTSize.ValueChanged -= this.OnValueChanged;
            }
            base.OnDisposing();
        }

        private static void Render(BitmapHelper.RenderInfo info, SpectrogramRendererData data)
        {
            if (data.SampleCount == 0 || !data.Initialized)
            {
                //No data.
                return;
            }

            if (info.Width != data.Width || info.Height != data.Height)
            {
                //Bitmap does not match data.
                return;
            }

            BitmapHelper.ShiftLeft(ref info, 1);
            BitmapHelper.DrawDots(ref info, data.Elements, data.Elements.Length);
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

        private static void UpdateElements(SpectrogramRendererData data)
        {
            switch (data.Mode)
            {
                default:
                case SpectrogramRendererMode.Mono:
                    UpdateElementsMono(data.Values, data.Elements, data.Colors, data.Width - 1, data.Height);
                    break;
                case SpectrogramRendererMode.Seperate:
                    UpdateElementsSeperate(data.Values, data.Elements, data.Colors, data.Width - 1, data.Height, data.Channels);
                    break;
            }
        }

        private static void UpdateElementsMono(float[,] values, Int32Pixel[] elements, Color[] colors, int x, int height)
        {
            var h = height - 1;
            for (var y = 0; y < height; y++)
            {
                var value1 = values[0, h - y];
                var value2 = Convert.ToInt32(value1 * (colors.Length - 1));
                var color = colors[value2];
                elements[y].X = x;
                elements[y].Y = y;
                elements[y].Blue = color.B;
                elements[y].Green = color.G;
                elements[y].Red = color.R;
                elements[y].Alpha = color.A;
            }
        }

        private static void UpdateElementsSeperate(float[,] values, Int32Pixel[] elements, Color[] colors, int x, int height, int channels)
        {
            var h = (height - 1) / channels;
            for (var channel = 0; channel < channels; channel++)
            {
                var offset = channel * h;
                for (var y = 0; y < h; y++)
                {
                    var value1 = values[channel, h - y];
                    var value2 = Convert.ToInt32(value1 * (colors.Length - 1));
                    var color = colors[value2];
                    elements[offset + y].X = x;
                    elements[offset + y].Y = offset + y;
                    elements[offset + y].Blue = color.B;
                    elements[offset + y].Green = color.G;
                    elements[offset + y].Red = color.R;
                    elements[offset + y].Alpha = color.A;
                }
            }
        }

        private static void Restore(BitmapHelper.RenderInfo info, SpectrogramRendererData data)
        {
            if (info.Width != data.Width || info.Height != data.Height)
            {
                //Bitmap does not match data.
                return;
            }

            //TODO: Only realloc if required.
            switch (data.Mode)
            {
                default:
                case SpectrogramRendererMode.Mono:
                    data.Values = new float[1, data.Height];
                    break;
                case SpectrogramRendererMode.Seperate:
                    data.Values = new float[data.Channels, data.Height];
                    break;
            }

            var height = data.Height;
            var channels = data.Channels;
            var samples = data.Samples;
            var history = data.History;
            var position = data.HistoryPosition - 1;
            var count = data.HistoryCount;
            var capacity = data.HistoryCapacity;
            var values = data.Values;
            var colors = data.Colors;
            var elements = data.Elements;
            var mode = data.Mode;
            for (int a = 0, x = data.Width - 1; a < count && x > 0; a++, x--)
            {
                //TODO: Can't find an Array.Copy which can handle the dimention difference.
                for (var b = 0; b < samples.Length; b++)
                {
                    samples[b] = history[b, position];
                }

                UpdateValues(data);

                switch (mode)
                {
                    default:
                    case SpectrogramRendererMode.Mono:
                        UpdateElementsMono(values, elements, colors, x, height);
                        break;
                    case SpectrogramRendererMode.Seperate:
                        UpdateElementsSeperate(values, elements, colors, x, height, channels);
                        break;
                }

                BitmapHelper.DrawDots(ref info, elements, elements.Length);

                if (position > 0)
                {
                    position--;
                }
                else
                {
                    position = count - 1;
                }
            }
        }

        public static SpectrogramRendererData Create(IOutput output, int width, int height, int fftSize, Color[] colors, SpectrogramRendererMode mode, SpectrogramRendererScale scale, int smoothing, int history)
        {
            var data = new SpectrogramRendererData()
            {
                Output = output,
                Width = width,
                Height = height,
                FFTSize = fftSize,
                HistoryCapacity = history,
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

            public float[,] History;

            public int HistoryPosition;

            public int HistoryCount;

            public int HistoryCapacity;

            public float[,] Values;

            public Color[] Colors;

            public Int32Pixel[] Elements;

            public SpectrogramRendererMode Mode;

            public SpectrogramRendererScale Scale;

            public int Smoothing;

            public bool Initialized;

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
                if (this.SampleCount == 0)
                {
                    return false;
                }
                if (this.HistoryCapacity > 0)
                {
                    UpdateHistory(this.Samples, this.History, ref this.HistoryPosition, ref this.HistoryCount, this.HistoryCapacity);
                }
                return true;
            }

            private void UpdateHistory(float[] samples, float[,] history, ref int position, ref int count, int capacity)
            {
                //TODO: Can't find an Array.Copy which can handle the dimention difference.
                for (var a = 0; a < samples.Length; a++)
                {
                    history[a, position] = samples[a];
                }
                if (position < capacity - 1)
                {
                    position++;
                }
                else
                {
                    position = 0;
                }
                if (count < capacity)
                {
                    count++;
                }
            }

            private void Update(int rate, int channels, OutputStreamFormat format)
            {
                if (this.Rate == rate && this.Channels == channels && this.Format == format && this.Initialized)
                {
                    return;
                }

                this.Rate = rate;
                this.Channels = channels;
                this.Format = format;
                this.Initialized = true;

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

                this.Elements = new Int32Pixel[this.Height];

                if (this.HistoryCapacity > 0)
                {
                    if (this.History == null || this.History.GetLength(0) != this.Samples.Length || this.History.GetLength(1) != this.HistoryCapacity)
                    {
                        this.HistoryPosition = 0;
                        this.HistoryCount = 0;
                        this.History = new float[this.Samples.Length, this.HistoryCapacity];
                    }
                }
                else
                {
                    if (this.History != null)
                    {
                        this.HistoryPosition = 0;
                        this.HistoryCount = 0;
                        this.History = null;
                    }
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
