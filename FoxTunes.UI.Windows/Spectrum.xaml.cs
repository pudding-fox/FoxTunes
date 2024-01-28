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
    [UIComponent("381328C3-C2CE-4FDA-AC92-71A15C3FC387", UIComponentSlots.NONE, "Spectrum", role: UIComponentRole.Hidden)]
    public partial class Spectrum : UIComponentBase
    {
        public static readonly IOutput Output = ComponentRegistry.Instance.GetComponent<IOutput>();

        public static readonly BooleanConfigurationElement Enabled;

        public static readonly SelectionConfigurationElement BarCount;

        public static readonly BooleanConfigurationElement ShowPeaks;

        public static readonly BooleanConfigurationElement HighCut;

        public static readonly BooleanConfigurationElement Smooth;

        public static readonly IntegerConfigurationElement SmoothFactor;

        public static readonly IntegerConfigurationElement HoldInterval;

        public static readonly IntegerConfigurationElement UpdateInterval;

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
            BarCount = configuration.GetElement<SelectionConfigurationElement>(
                SpectrumBehaviourConfiguration.SECTION,
                SpectrumBehaviourConfiguration.BARS_ELEMENT
            );
            ShowPeaks = configuration.GetElement<BooleanConfigurationElement>(
                SpectrumBehaviourConfiguration.SECTION,
                SpectrumBehaviourConfiguration.PEAKS_ELEMENT
            );
            HighCut = configuration.GetElement<BooleanConfigurationElement>(
                SpectrumBehaviourConfiguration.SECTION,
                SpectrumBehaviourConfiguration.HIGH_CUT_ELEMENT
            );
            Smooth = configuration.GetElement<BooleanConfigurationElement>(
                SpectrumBehaviourConfiguration.SECTION,
                SpectrumBehaviourConfiguration.SMOOTH_ELEMENT
            );
            SmoothFactor = configuration.GetElement<IntegerConfigurationElement>(
                SpectrumBehaviourConfiguration.SECTION,
                SpectrumBehaviourConfiguration.SMOOTH_FACTOR_ELEMENT
            );
            HoldInterval = configuration.GetElement<IntegerConfigurationElement>(
                SpectrumBehaviourConfiguration.SECTION,
                SpectrumBehaviourConfiguration.HOLD_ELEMENT
            );
            UpdateInterval = configuration.GetElement<IntegerConfigurationElement>(
                SpectrumBehaviourConfiguration.SECTION,
                SpectrumBehaviourConfiguration.INTERVAL_ELEMENT
            );
        }

        public Spectrum()
        {
            this.InitializeComponent();
            this.Border.Child = new Renderer();
        }

        protected virtual void OnLoaded(object sender, RoutedEventArgs e)
        {
            var renderer = this.Border.Child as Renderer;
            if (renderer == null)
            {
                return;
            }
            //The Enabled setting is for showing/hiding on the main window only.
            //If we're hosted somewhere else then always enable.
            else if (!this.IsHostedIn<MainWindow>())
            {
                this.Visibility = Visibility.Visible;
                renderer.Start();
            }
            else if (Enabled != null)
            {
                Enabled.ConnectValue(value =>
                {
                    if (value)
                    {
                        this.Visibility = Visibility.Visible;
                        renderer.Start();
                    }
                    else
                    {
                        this.Visibility = Visibility.Collapsed;
                        renderer.Stop();
                    }
                });
            }
        }

        private class Renderer : Control
        {
            public static readonly object SyncRoot = new object();

            protected static ILogger Logger
            {
                get
                {
                    return LogManager.Logger;
                }
            }

            const int FACTOR = 4;

            const int ROLLOFF_INTERVAL = 500;

            public static readonly int SampleCount = 512;

            public static readonly int HighCutOff = 128;

            public float[] Buffer = new float[SampleCount];

            public int[] Dimentions = new int[2];

            public int Step;

            public int ElementCount;

            public int[,] Elements;

            public int[,] Peaks;

            public int[] Holds;

            public int SamplesPerElement;

            public float Weight;

            public DateTime LastUpdated;

            public bool HasData = false;

            public Renderer()
            {
                this.SnapsToDevicePixels = true;
                this.LastUpdated = DateTime.UtcNow;
                BarCount.ConnectValue(value =>
                {
                    this.ElementCount = SpectrumBehaviourConfiguration.GetBars(value);
                    var task = Windows.Invoke(() => this.MinWidth = SpectrumBehaviourConfiguration.GetWidth(value));
                    Configure();
                });
                HighCut.ConnectValue(value => Configure());
                UpdateInterval.ConnectValue(value =>
                {
                    lock (SyncRoot)
                    {
                        if (this.Timer != null)
                        {
                            this.Timer.Interval = value;
                        }
                    }
                });
            }

            public Timer Timer { get; private set; }

            public void Start()
            {
                lock (SyncRoot)
                {
                    if (this.Timer == null)
                    {
                        this.Timer = new Timer();
                        this.Timer.Interval = UpdateInterval.Value;
                        this.Timer.AutoReset = false;
                        this.Timer.Elapsed += this.OnElapsed;
                        this.Timer.Start();
                    }
                }
            }

            public void Stop()
            {
                lock (SyncRoot)
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

            protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
            {
                base.OnRenderSizeChanged(sizeInfo);
                this.Dimentions[0] = Convert.ToInt32(this.ActualWidth);
                this.Dimentions[1] = Convert.ToInt32(this.ActualHeight);
                Configure();
            }

            protected override void OnRender(DrawingContext drawingContext)
            {
                bool showPeaks = ShowPeaks.Value;
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
                        if (showPeaks && this.Elements[a, 1] > this.Peaks[a, 1])
                        {
                            drawingContext.DrawRectangle(
                                brush,
                                pen,
                                new Rect(
                                    this.Peaks[a, 0],
                                    this.Peaks[a, 1],
                                    this.Peaks[a, 2],
                                    this.Peaks[a, 3]
                                )
                            );
                        }
                    }
                }
                if (this.Timer != null)
                {
                    this.Timer.Start();
                }
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
                        var now = DateTime.UtcNow;
                        var duration = now - this.LastUpdated;
                        var data = new SpectrumData()
                        {
                            Samples = Buffer,
                            ElementCount = this.ElementCount,
                            SamplesPerElement = this.SamplesPerElement,
                            Weight = this.Weight,
                            Step = this.Step,
                            Height = this.Dimentions[1],
                            Elements = this.Elements,
                            Peaks = this.Peaks,
                            Holds = this.Holds,
                            //We want a value kind of like the actual update interval but not too far off.
                            Duration = Math.Min(Convert.ToInt32(duration.TotalMilliseconds), UpdateInterval.Value * 100),
                            HoldInterval = HoldInterval.Value,
                            UpdateInterval = UpdateInterval.Value,
                            Smoothing = SmoothFactor.Value
                        };
                        if (Smooth == null || !Smooth.Value)
                        {
                            UpdateFast(data);
                        }
                        else
                        {
                            UpdateSmooth(data);
                        }
                        UpdatePeaks(data);
                        this.LastUpdated = DateTime.UtcNow;
                        this.HasData = true;
                    }
                    Windows.Invoke(() => this.InvalidateVisual());
                }
                catch (Exception exception)
                {
                    Logger.Write(this.GetType(), LogLevel.Warn, "Failed to update spectrum data, disabling: {0}", exception.Message);
                }
            }

            protected virtual void Configure()
            {
                this.Elements = new int[this.ElementCount, 4];
                this.Peaks = new int[this.ElementCount, 4];
                this.Holds = new int[this.ElementCount];
                if (HighCut != null && HighCut.Value)
                {
                    this.SamplesPerElement = (SampleCount - HighCutOff) / this.ElementCount;
                }
                else
                {
                    this.SamplesPerElement = SampleCount / this.ElementCount;
                }
                this.Weight = (float)16 / this.ElementCount;
                this.Step = this.Dimentions[0] / ElementCount;
            }

            private static void UpdateFast(SpectrumData data)
            {
                for (int a = 0, b = 0; a < data.ElementCount; a++)
                {
                    var sample = 0f;
                    for (var c = 0; c < data.SamplesPerElement; b++, c++)
                    {
                        sample += data.Samples[b];
                    }
                    sample /= data.ElementCount;
                    var factor = FACTOR + (a * data.Weight);
                    var value = Math.Sqrt(sample) * factor;
                    if (value > 1)
                    {
                        value = 1;
                    }
                    else if (value < 0)
                    {
                        value = 0;
                    }
                    var barHeight = Convert.ToInt32(value * data.Height);
                    data.Elements[a, 0] = a * data.Step;
                    data.Elements[a, 2] = data.Step;
                    if (barHeight > 0)
                    {
                        data.Elements[a, 3] = Convert.ToInt32(value * data.Height);
                    }
                    else
                    {
                        data.Elements[a, 3] = 0;
                    }
                    data.Elements[a, 1] = data.Height - data.Elements[a, 3];
                    if (data.Elements[a, 1] < data.Peaks[a, 1])
                    {
                        data.Peaks[a, 0] = a * data.Step;
                        data.Peaks[a, 2] = data.Step;
                        data.Peaks[a, 3] = 1;
                        data.Peaks[a, 1] = data.Elements[a, 1];
                        data.Holds[a] = data.HoldInterval;
                    }
                }
            }

            private static void UpdateSmooth(SpectrumData data)
            {
                var fast = (float)data.Height / data.Smoothing;
                for (int a = 0, b = 0; a < data.ElementCount; a++)
                {
                    var sample = 0f;
                    for (var c = 0; c < data.SamplesPerElement; b++, c++)
                    {
                        sample += data.Samples[b];
                    }
                    sample /= data.SamplesPerElement;
                    var factor = FACTOR + (a * data.Weight);
                    var value = Math.Sqrt(sample) * factor;
                    if (value > 1)
                    {
                        value = 1;
                    }
                    else if (value < 0)
                    {
                        value = 0;
                    }
                    var barHeight = Convert.ToInt32(value * data.Height);
                    data.Elements[a, 0] = a * data.Step;
                    data.Elements[a, 2] = data.Step;
                    if (barHeight > 0)
                    {
                        var difference = Math.Abs(data.Elements[a, 3] - barHeight);
                        if (difference > 0)
                        {
                            if (difference < 2)
                            {
                                if (barHeight > data.Elements[a, 3])
                                {
                                    data.Elements[a, 3]++;
                                }
                                else if (barHeight < data.Elements[a, 3])
                                {
                                    data.Elements[a, 3]--;
                                }
                            }
                            else
                            {
                                var distance = (float)difference / barHeight;
                                //TODO: We should use some kind of easing function.
                                //var increment = distance * distance * distance;
                                //var increment = 1 - Math.Pow(1 - distance, 5);
                                var increment = distance;
                                if (barHeight > data.Elements[a, 3])
                                {
                                    data.Elements[a, 3] = (int)Math.Min(data.Elements[a, 3] + Math.Min(Math.Max(fast * increment, 1), difference), data.Height);
                                }
                                else if (barHeight < data.Elements[a, 3])
                                {
                                    data.Elements[a, 3] = (int)Math.Max(data.Elements[a, 3] - Math.Min(Math.Max(fast * increment, 1), difference), 0);
                                }
                            }
                        }
                    }
                    else
                    {
                        data.Elements[a, 3] = 0;
                    }
                    data.Elements[a, 1] = data.Height - data.Elements[a, 3];
                    if (data.Elements[a, 1] < data.Peaks[a, 1])
                    {
                        data.Peaks[a, 0] = a * data.Step;
                        data.Peaks[a, 2] = data.Step;
                        data.Peaks[a, 3] = 1;
                        data.Peaks[a, 1] = data.Elements[a, 1];
                        data.Holds[a] = data.HoldInterval + ROLLOFF_INTERVAL;
                    }
                }
            }

            private static void UpdatePeaks(SpectrumData data)
            {
                var fast = data.Height / 4;
                for (int a = 0; a < data.ElementCount; a++)
                {
                    if (data.Elements[a, 1] > data.Peaks[a, 1] && data.Peaks[a, 1] < data.Height - 1)
                    {
                        if (data.Holds[a] > 0)
                        {
                            if (data.Holds[a] < data.HoldInterval)
                            {
                                var distance = 1 - ((float)data.Holds[a] / data.HoldInterval);
                                var increment = distance * distance * distance;
                                data.Peaks[a, 1] += (int)Math.Round(fast * increment);
                            }
                            data.Holds[a] -= data.Duration;
                        }
                        else if (data.Peaks[a, 1] < data.Height - fast)
                        {
                            data.Peaks[a, 1] += fast;
                        }
                        else if (data.Peaks[a, 1] < data.Height - 1)
                        {
                            data.Peaks[a, 1] = data.Height - 1;
                        }
                    }
                }
            }

            ~Renderer()
            {
                if (this.Timer != null)
                {
                    this.Timer.Stop();
                    this.Timer.Elapsed -= this.OnElapsed;
                    this.Timer.Dispose();
                    this.Timer = null;
                }
            }

            private struct SpectrumData
            {
                public float[] Samples;

                public int ElementCount;

                public int SamplesPerElement;

                public float Weight;

                public int Step;

                public int Height;

                public int[,] Elements;

                public int[,] Peaks;

                public int[] Holds;

                public int UpdateInterval;

                public int HoldInterval;

                public int Duration;

                public int Smoothing;
            }
        }
    }
}