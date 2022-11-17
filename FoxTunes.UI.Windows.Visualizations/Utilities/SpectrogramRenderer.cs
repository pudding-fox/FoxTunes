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
    public class SpectrogramRenderer : VisualizationBase
    {
        const double FACTOR = 262140; //4.0 * 65535.0

        public Color[] ColorPalette = new KeyValuePair<int, Color>[]
        {
            new KeyValuePair<int, Color>(0, Colors.Black),
            new KeyValuePair<int, Color>(25, Colors.Gray),
            new KeyValuePair<int, Color>(100, Colors.White)
        }.ToGradient();

        public SpectrogramRendererData RendererData { get; private set; }

        public SelectionConfigurationElement FFTSize { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            base.InitializeComponent(core);
            this.Configuration.GetElement<IntegerConfigurationElement>(
               VisualizationBehaviourConfiguration.SECTION,
               VisualizationBehaviourConfiguration.INTERVAL_ELEMENT
            ).ConnectValue(value => this.UpdateInterval = value);
            this.FFTSize = this.Configuration.GetElement<SelectionConfigurationElement>(
               VisualizationBehaviourConfiguration.SECTION,
               VisualizationBehaviourConfiguration.FFT_SIZE_ELEMENT
            );
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
                false
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
                    false
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

            Render(info, this.RendererData, this.ColorPalette);

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
            if (this.FFTSize != null)
            {
                this.FFTSize.ValueChanged -= this.OnValueChanged;
            }
            base.OnDisposing();
        }

        private static void Render(BitmapHelper.RenderInfo info, SpectrogramRendererData data, Color[] colors)
        {
            if (data.SampleCount == 0)
            {
                //No data.
                BitmapHelper.Clear(info);
                return;
            }

            BitmapHelper.ShiftLeft(info, 1);

            var x = data.Width - 1;
            var h = data.Height - 1;
            var values = data.Values;
            for (var y = 0; y < data.Height; y++)
            {
                var value1 = (double)values[h - y] / FACTOR;
                var value2 = Convert.ToInt32(value1 * colors.Length);
                var color = colors[value2];
                info.Red = color.R;
                info.Green = color.G;
                info.Blue = color.B;
                BitmapHelper.DrawDot(info, x, y);
            }
        }

        private static void UpdateValues(SpectrogramRendererData data)
        {
            UpdateValues(data.Width, data.Height, data.Samples, data.FFTRange, data.Values);
        }

        private static void UpdateValues(int width, int height, float[] samples, int sampleCount, int[] values)
        {
            var num1 = 0f;
            var num2 = 0f;
            var num3 = 0;
            var num4 = 0;
            var num5 = (double)sampleCount / height;

            Array.Clear(values, 0, values.Length);

            for (var a = 1; a < sampleCount; a++)
            {
                num3 = (int)(Math.Sqrt(samples[a]) * FACTOR);
                num4 = Math.Max(num3, num4);
                num4 = Math.Max(num4, 0);
                num1 = (float)Math.Round((double)a / num5) - 1f;
                if (num1 > num2)
                {
                    values[Convert.ToInt32(num2)] = num4;
                    num2 = num1;
                    num4 = 0;
                }
            }
        }

        public static SpectrogramRendererData Create(IOutput output, int width, int height, int fftSize, bool highCut)
        {
            var data = new SpectrogramRendererData()
            {
                Output = output,
                Width = width,
                Height = height,
                FFTSize = fftSize,
                Samples = output.GetBuffer(fftSize),
                Values = new int[height],
            };
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

        public class SpectrogramRendererData
        {
            public IOutput Output;

            public int Width;

            public int Height;

            public int FFTSize;

            public int FFTRange;

            public float[] Samples;

            public int SampleCount;

            public int[] Values;

            public bool Update()
            {
                this.SampleCount = this.Output.GetData(this.Samples, this.FFTSize);
                return this.SampleCount > 0;
            }

            public void Clear()
            {
                Array.Clear(this.Samples, 0, this.Samples.Length);
                this.SampleCount = 0;
            }
        }
    }
}
