using FoxTunes.Interfaces;
using System;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for Spectrum.xaml
    /// </summary>
    public partial class Spectrum : UserControl
    {
        protected static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        const int FACTOR = 4;

        const int UPDATE_INTERVAL = 100;

        public static readonly IOutput Output = ComponentRegistry.Instance.GetComponent<IOutput>();

        public static readonly BooleanConfigurationElement Enabled;

        public static readonly SelectionConfigurationElement Bars;

        static Spectrum()
        {
            var configuration = ComponentRegistry.Instance.GetComponent<IConfiguration>();
            if (configuration == null)
            {
                return;
            }
            Enabled = configuration.GetElement<BooleanConfigurationElement>(
                    SpectrumBehaviourConfiguration.SECTION,
                    SpectrumBehaviourConfiguration.ENABLED_ELEMENT
            );
            Bars = configuration.GetElement<SelectionConfigurationElement>(
                    SpectrumBehaviourConfiguration.SECTION,
                    SpectrumBehaviourConfiguration.BARS_ELEMENT
            );
        }

        public static readonly int SampleCount = 512;

        public static readonly float[] Buffer = new float[SampleCount];

        public int[] Dimentions = new int[2];

        public int Step;

        public int ElementCount;

        public int[,] Elements;

        public int SamplesPerElement;

        public float Weight;

        public bool HasData = false;

        public Spectrum()
        {
            this.InitializeComponent();
            this.Timer = new Timer();
            this.Timer.Interval = UPDATE_INTERVAL;
            this.Timer.AutoReset = false;
            this.Timer.Elapsed += this.OnElapsed;
            if (Enabled != null)
            {
                Enabled.ConnectValue(value =>
                {
                    if (value)
                    {
                        this.Visibility = Visibility.Visible;
                        this.Timer.Start();
                    }
                    else
                    {
                        this.Visibility = Visibility.Collapsed;
                        this.Timer.Stop();
                    }
                });
            }
            if (Bars != null)
            {
                Bars.ConnectValue(value =>
                    this.Configure(
                        SpectrumBehaviourConfiguration.GetBars(value),
                        SpectrumBehaviourConfiguration.GetWidth(value)
                    )
                );
            }
        }

        public Timer Timer { get; private set; }

        protected virtual void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Dimentions[0] = Convert.ToInt32(this.ActualWidth);
            this.Dimentions[1] = Convert.ToInt32(this.ActualHeight);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            if (this.HasData)
            {
                var brush = this.Foreground;
                var pen = new Pen(this.Foreground, 0.4);
                for (var a = 0; a < ElementCount; a++)
                {
                    drawingContext.DrawRectangle(
                        brush,
                        pen,
                        new Rect(
                            this.Elements[a, 0],
                            this.Elements[a, 1],
                            this.Elements[a, 2],
                            this.Elements[a, 3]
                        )
                    );
                }
            }
            this.Timer.Start();
        }

        protected virtual void OnElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                var length = Output.GetData(Buffer);
                if (length <= 0)
                {
                    this.HasData = false;
                }
                else
                {
                    Update(this.ElementCount, this.SamplesPerElement, this.Weight, this.Step, this.Dimentions[1], this.Elements);
                    this.HasData = true;
                }
                Windows.Invoke(() => this.InvalidateVisual());
            }
            catch (Exception exception)
            {
                Logger.Write(this.GetType(), LogLevel.Warn, "Failed to update spectrum data, disabling: {0}", exception.Message);
            }
        }

        protected virtual void Configure(int count, int width)
        {
            Windows.Invoke(() => this.Width = width);
            this.ElementCount = count;
            this.Elements = new int[count, 4];
            this.SamplesPerElement = SampleCount / count;
            this.Weight = (float)16 / this.ElementCount;
            this.Step = width / ElementCount;
        }

        private static void Update(int count, int samples, float weight, int step, int height, int[,] elements)
        {
            for (int a = 0, b = 0; a < count; a++)
            {
                var sample = 0f;
                for (var c = 0; c < samples; b++, c++)
                {
                    sample += Buffer[b];
                }
                sample /= samples;
                var factor = FACTOR + (a * weight);
                var value = Math.Sqrt(sample) * factor;
                if (value > 1)
                {
                    value = 1;
                }
                else if (value < 0)
                {
                    value = 0;
                }
                elements[a, 0] = a * step;
                elements[a, 2] = step;
                elements[a, 3] = Convert.ToInt32(value * height);
                elements[a, 1] = height - elements[a, 3];
            }
        }

        ~Spectrum()
        {
            if (this.Timer != null)
            {
                this.Timer.Stop();
                this.Timer.Elapsed -= this.OnElapsed;
                this.Timer.Dispose();
                this.Timer = null;
            }
        }
    }
}
