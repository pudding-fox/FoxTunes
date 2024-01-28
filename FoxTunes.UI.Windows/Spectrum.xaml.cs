using FoxTunes.Interfaces;
using System;
using System.Configuration;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for Spectrum.xaml
    /// </summary>
    [UIComponent("381328C3-C2CE-4FDA-AC92-71A15C3FC387", UIComponentSlots.NONE, "Spectrum")]
    public partial class Spectrum : UIComponentBase
    {
        public static readonly IOutput Output = ComponentRegistry.Instance.GetComponent<IOutput>();

        public static readonly BooleanConfigurationElement Enabled;

        public static readonly SelectionConfigurationElement BarCount;

        public static readonly BooleanConfigurationElement ShowPeaks;

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
            UpdateInterval = configuration.GetElement<IntegerConfigurationElement>(
                SpectrumBehaviourConfiguration.SECTION,
                SpectrumBehaviourConfiguration.INTERVAL_ELEMENT
            );
        }

        public Spectrum()
        {
            this.InitializeComponent();
            var renderer = new Renderer();
            this.Border.Child = renderer;
            if (Enabled != null)
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

            const int UPDATE_INTERVAL = 100;

            const int HOLD_INTERVAL = 10;

            public static readonly int SampleCount = 512;

            public static readonly float[] Buffer = new float[SampleCount];

            public int[] Dimentions = new int[2];

            public int Step;

            public int ElementCount;

            public int[,] Elements;

            public int[,] Peaks;

            public int[] Holds;

            public int SamplesPerElement;

            public float Weight;

            public int Iteration;

            public bool HasData = false;

            public Renderer()
            {
                if (BarCount != null)
                {
                    BarCount.ConnectValue(value =>
                    {
                        this.ElementCount = SpectrumBehaviourConfiguration.GetBars(value);
                        var task = Windows.Invoke(() => this.MinWidth = SpectrumBehaviourConfiguration.GetWidth(value));
                        Configure();
                    });
                }
                if (UpdateInterval != null)
                {
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
            }

            public Timer Timer { get; private set; }

            public void Start()
            {
                lock (SyncRoot)
                {
                    if (this.Timer == null)
                    {
                        this.Timer = new Timer();
                        if (UpdateInterval != null)
                        {
                            this.Timer.Interval = UpdateInterval.Value;
                        }
                        else
                        {
                            this.Timer.Interval = UPDATE_INTERVAL;
                        }
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
                        this.Iteration++;
                        var iterations = 10 / this.Timer.Interval;
                        Update(this.ElementCount, this.SamplesPerElement, this.Weight, this.Step, this.Dimentions[1], this.Elements, this.Peaks, this.Holds);
                        if (this.Iteration >= iterations)
                        {
                            Update(this.ElementCount, this.Dimentions[1], this.Elements, this.Peaks, this.Holds);
                            this.Iteration = 0;
                        }
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
                this.SamplesPerElement = SampleCount / this.ElementCount;
                this.Weight = (float)16 / this.ElementCount;
                this.Step = this.Dimentions[0] / ElementCount;
            }

            private static void Update(int count, int samples, float weight, int step, int height, int[,] elements, int[,] peaks, int[] holds)
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
                    if (elements[a, 1] < peaks[a, 1])
                    {
                        peaks[a, 0] = a * step;
                        peaks[a, 2] = step;
                        peaks[a, 3] = 1;
                        peaks[a, 1] = elements[a, 1];
                        holds[a] = HOLD_INTERVAL;
                    }
                }
            }

            private static void Update(int count, int height, int[,] elements, int[,] peaks, int[] holds)
            {
                const int HOLD = 3;
                var fast = height / 6;
                for (int a = 0; a < count; a++)
                {
                    if (elements[a, 1] > peaks[a, 1] && peaks[a, 1] < height - 1)
                    {
                        if (holds[a] > 0)
                        {
                            if (holds[a] < HOLD_INTERVAL - HOLD)
                            {
                                var distance = 1 - (float)holds[a] / (HOLD_INTERVAL - HOLD);
                                var increment = distance * distance * distance;
                                peaks[a, 1] += (int)Math.Round(fast * increment);
                            }
                            holds[a]--;
                        }
                        else if (peaks[a, 1] < height - fast)
                        {
                            peaks[a, 1] += fast;
                        }
                        else if (peaks[a, 1] < height - 1)
                        {
                            peaks[a, 1]++;
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
        }
    }
}