using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Media.Imaging;

namespace FoxTunes
{
    public class OscilloscopeRenderer : VisualizationBase
    {
        public OscilloscopeRendererData RendererData { get; private set; }

        public SelectionConfigurationElement Mode { get; private set; }

        public BooleanConfigurationElement Smooth { get; private set; }

        public IntegerConfigurationElement SmoothingFactor { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            base.InitializeComponent(core);
            this.Mode = this.Configuration.GetElement<SelectionConfigurationElement>(
                OscilloscopeBehaviourConfiguration.SECTION,
                OscilloscopeBehaviourConfiguration.MODE_ELEMENT
            );
            this.Smooth = this.Configuration.GetElement<BooleanConfigurationElement>(
               VisualizationBehaviourConfiguration.SECTION,
               VisualizationBehaviourConfiguration.SMOOTH_ELEMENT
            );
            this.SmoothingFactor = this.Configuration.GetElement<IntegerConfigurationElement>(
               VisualizationBehaviourConfiguration.SECTION,
               VisualizationBehaviourConfiguration.SMOOTH_FACTOR_ELEMENT
            );
            this.Configuration.GetElement<IntegerConfigurationElement>(
               VisualizationBehaviourConfiguration.SECTION,
               VisualizationBehaviourConfiguration.INTERVAL_ELEMENT
            ).ConnectValue(value => this.UpdateInterval = value);
            this.Mode.ValueChanged += this.OnValueChanged;
            this.Smooth.ValueChanged += this.OnValueChanged;
            this.SmoothingFactor.ValueChanged += this.OnValueChanged;
            var task = this.CreateBitmap();
        }

        protected virtual void OnValueChanged(object sender, EventArgs e)
        {
            var task = this.RefreshBitmap();
        }

        protected override void CreateViewBox()
        {
            this.RendererData = Create(
                this,
                this.Bitmap.PixelWidth,
                this.Bitmap.PixelHeight,
                OscilloscopeBehaviourConfiguration.GetMode(this.Mode.Value)
            );
            this.Viewbox = new Rect(0, 0, this.Bitmap.PixelWidth, this.Bitmap.PixelHeight);
        }

        protected virtual Task RefreshBitmap()
        {
            return Windows.Invoke(() =>
            {
                this.RendererData = Create(
                    this,
                    this.Bitmap.PixelWidth,
                    this.Bitmap.PixelHeight,
                    OscilloscopeBehaviourConfiguration.GetMode(this.Mode.Value)
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
                return;
            }

            Render(info, this.RendererData);

            await Windows.Invoke(() =>
            {
                bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
                bitmap.Unlock();
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

                switch (data.Mode)
                {
                    default:
                    case OscilloscopeRendererMode.Mono:
                        if (this.Smooth.Value)
                        {
                            UpdateElementsSmoothMono(data.Values, data.Peaks, data.Elements, data.Width, data.Height, this.SmoothingFactor.Value);
                        }
                        else
                        {
                            UpdateElementsFastMono(data.Values, data.Peaks, data.Elements, data.Width, data.Height);
                        }
                        break;
                    case OscilloscopeRendererMode.Seperate:
                        if (this.Smooth.Value)
                        {
                            UpdateElementsSmoothSeperate(data.Values, data.Peaks, data.Elements, data.Width, data.Height, data.Channels, this.SmoothingFactor.Value);
                        }
                        else
                        {
                            UpdateElementsFastSeperate(data.Values, data.Peaks, data.Elements, data.Width, data.Height, data.Channels);
                        }
                        break;
                }

                data.LastUpdated = DateTime.UtcNow;

                var task = this.Render();
            }
            catch (Exception exception)
            {
                Logger.Write(this.GetType(), LogLevel.Warn, "Failed to update spectrum data, disabling: {0}", exception.Message);
            }
        }

        private static void UpdateElementsFastMono(float[,] values, float[] peaks, Int32Point[,] elements, int width, int height)
        {
            height = height - 1;
            var step = width / values.Length;
            for (var a = 0; a < values.Length; a++)
            {
                var x = (a * step);
                var y = default(int);
                if (values[0, a] > 0)
                {
                    y = height - Convert.ToInt32((values[0, a] / peaks[0]) * height);
                }
                else
                {
                    y = height;
                }
                elements[0, a].X = x;
                elements[0, a].Y = y;
            }
        }

        private static void UpdateElementsFastSeperate(float[,] values, float[] peaks, Int32Point[,] elements, int width, int height, int channels)
        {

        }

        private static void UpdateElementsSmoothMono(float[,] values, float[] peaks, Int32Point[,] elements, int width, int height, int smoothing)
        {
            height = height - 1;
            var fast = Math.Min((float)height / smoothing, 10);
            var step = width / values.Length;
            for (var a = 0; a < values.Length; a++)
            {
                var x = (a * step) + (step / 2);
                var y = default(int);
                if (values[0, a] > 0)
                {
                    y = height - Convert.ToInt32((values[0, a] / peaks[0]) * height);
                }
                else
                {
                    y = height;
                }
                elements[0, a].X = x;
                var difference = Math.Abs(elements[0, a].Y - y);
                if (difference > 0)
                {
                    if (difference < fast)
                    {
                        if (y > elements[0, a].Y)
                        {
                            elements[0, a].Y++;
                        }
                        else if (y < elements[0, a].Y)
                        {
                            elements[0, a].Y--;
                        }
                    }
                    else
                    {
                        var distance = (float)difference / height;
                        var increment = Math.Sqrt(1 - Math.Pow(distance - 1, 2));
                        var smoothed = Math.Min(Math.Max(fast * increment, 1), fast);
                        if (y > elements[0, a].Y)
                        {
                            elements[0, a].Y = (int)Math.Min(elements[0, a].Y + smoothed, height);
                        }
                        else if (y < elements[0, a].Y)
                        {
                            elements[0, a].Y = (int)Math.Max(elements[0, a].Y - smoothed, 1);
                        }
                    }
                }
            }
        }

        private static void UpdateElementsSmoothSeperate(float[,] values, float[] peaks, Int32Point[,] elements, int width, int height, int channels, int smoothing)
        {
        }

        protected override Freezable CreateInstanceCore()
        {
            return new OscilloscopeRenderer();
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
            if (this.Smooth != null)
            {
                this.Smooth.ValueChanged -= this.OnValueChanged;
            }
            if (this.SmoothingFactor != null)
            {
                this.SmoothingFactor.ValueChanged -= this.OnValueChanged;
            }
        }

        private static void Render(BitmapHelper.RenderInfo info, OscilloscopeRendererData data)
        {
            BitmapHelper.Clear(info);

            if (data.Elements != null)
            {
                switch (data.Mode)
                {
                    default:
                    case OscilloscopeRendererMode.Mono:
                        RenderMono(info, data.Elements, data.Width);
                        break;
                    case OscilloscopeRendererMode.Seperate:
                        RenderSeperate(info, data.Elements, data.Channels, data.Width);
                        break;
                }
            }
        }

        private static void RenderMono(BitmapHelper.RenderInfo info, Int32Point[,] elements, int width)
        {
            for (var a = 0; a < width - 1; a++)
            {
                var point1 = elements[0, a];
                var point2 = elements[0, a + 1];
                BitmapHelper.DrawLine(
                    info,
                    point1.X,
                    point1.Y,
                    point2.X,
                    point2.Y
                );
            }
        }

        private static void RenderSeperate(BitmapHelper.RenderInfo info, Int32Point[,] elements, int channels, int width)
        {
            for (var a = 0; a < channels; a++)
            {
                for (var b = 0; b < width - 1; b++)
                {
                    var point1 = elements[a, b];
                    var point2 = elements[a, b + 1];
                    BitmapHelper.DrawLine(
                        info,
                        point1.X,
                        point1.Y,
                        point2.X,
                        point2.Y
                    );
                }
            }
        }

        private static void UpdateValues(OscilloscopeRendererData data)
        {
            switch (data.Format)
            {
                case OutputStreamFormat.Short:
                    UpdateValues(data.Samples16, data.Samples32, data.SampleCount);
                    break;
            }
            switch (data.Mode)
            {
                default:
                case OscilloscopeRendererMode.Mono:
                    UpdateValuesMono(data.Samples32, data.Values, data.Peaks, data.SampleCount);
                    break;
                case OscilloscopeRendererMode.Seperate:
                    UpdateValuesSeperate(data.Samples32, data.Values, data.Channels, data.SampleCount);
                    break;
            }
        }

        private static void UpdateValues(short[] samples16, float[] samples32, int count)
        {
            for (var a = 0; a < count; a++)
            {
                samples32[a] = samples16[a] / short.MaxValue;
            }
        }

        private static void UpdateValuesMono(float[] samples, float[,] values, float[] peaks, int count)
        {
            var samplesPerValue = count / values.Length;
            peaks[0] = 0.1f;
            for (int a = 0, b = 0; a < count && b < values.Length; a += samplesPerValue, b++)
            {
                var value = default(float);
                for (var c = 0; c < samplesPerValue; c++)
                {
                    value += samples[a + c];
                }
                values[0, b] = value / samplesPerValue;
                peaks[0] = Math.Max(Math.Abs(values[0, b]), peaks[0]);
            }
        }

        private static void UpdateValuesSeperate(float[] samples, float[,] values, int channels, int count)
        {

        }

        public static OscilloscopeRendererData Create(OscilloscopeRenderer renderer, int width, int height, OscilloscopeRendererMode mode)
        {
            var data = new OscilloscopeRendererData()
            {
                Renderer = renderer,
                Width = width,
                Height = height,
                Mode = mode
            };
            return data;
        }

        public class OscilloscopeRendererData
        {
            public OscilloscopeRenderer Renderer;

            public int Width;

            public int Height;

            public int Rate;

            public int Channels;

            public OutputStreamFormat Format;

            public short[] Samples16;

            public float[] Samples32;

            public int SampleCount;

            public float[,] Values;

            public float[] Peaks;

            public Int32Point[,] Elements;

            public DateTime LastUpdated;

            public OscilloscopeRendererMode Mode;

            public bool Update()
            {
                var rate = default(int);
                var channels = default(int);
                var format = default(OutputStreamFormat);
                if (!this.Renderer.Output.GetFormat(out rate, out channels, out format))
                {
                    return false;
                }
                this.Update(rate, channels, format);
                switch (this.Format)
                {
                    case OutputStreamFormat.Short:
                        this.SampleCount = this.Renderer.Output.GetData(this.Samples16) / sizeof(short);
                        break;
                    case OutputStreamFormat.Float:
                        this.SampleCount = this.Renderer.Output.GetData(this.Samples32) / sizeof(float);
                        break;
                }
                return this.Rate > 0 && this.Channels > 0 && this.SampleCount > 0;
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

                if (format == OutputStreamFormat.Short)
                {
                    this.Samples16 = this.Renderer.Output.GetBuffer<short>(TimeSpan.FromMilliseconds(this.Renderer.UpdateInterval));
                    this.Samples32 = new float[this.Samples16.Length];
                }
                else if (format == OutputStreamFormat.Float)
                {
                    this.Samples32 = this.Renderer.Output.GetBuffer<float>(TimeSpan.FromMilliseconds(this.Renderer.UpdateInterval));
                }

                //TODO: Only realloc if required.
                switch (this.Mode)
                {
                    default:
                    case OscilloscopeRendererMode.Mono:
                        this.Values = new float[1, this.Width];
                        this.Elements = new Int32Point[1, this.Width];
                        this.Peaks = new float[1];
                        break;
                    case OscilloscopeRendererMode.Seperate:
                        this.Values = new float[this.Channels, this.Width];
                        this.Elements = new Int32Point[this.Channels, this.Width];
                        this.Peaks = new float[this.Channels];
                        break;
                }
            }

            public void Clear()
            {
                if (this.Samples16 != null)
                {
                    Array.Clear(this.Samples16, 0, this.Samples16.Length);
                }
                Array.Clear(this.Samples32, 0, this.Samples32.Length);
                this.SampleCount = 0;
            }
        }
    }

    public enum OscilloscopeRendererMode : byte
    {
        None,
        Mono,
        Seperate
    }
}
