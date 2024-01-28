using FoxTunes.Interfaces;
using System;
using System.Windows;
using System.Windows.Media;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for Spectrum.xaml
    /// </summary>
    [UIComponent("381328C3-C2CE-4FDA-AC92-71A15C3FC387", UIComponentSlots.NONE, "Spectrum", role: UIComponentRole.Hidden)]
    public partial class Spectrum : UIComponentBase
    {
        const int TIMEOUT = 1000;

        public static readonly SelectionConfigurationElement BarCount;

        public static readonly BooleanConfigurationElement ShowPeaks;

        public static readonly BooleanConfigurationElement HighCut;

        public static readonly BooleanConfigurationElement Smooth;

        public static readonly IntegerConfigurationElement SmoothFactor;

        public static readonly IntegerConfigurationElement HoldInterval;

        public static readonly IntegerConfigurationElement UpdateInterval;

        public static readonly SelectionConfigurationElement FFTSize;

        public static readonly IntegerConfigurationElement Amplitude;

        public static readonly DoubleConfigurationElement ScalingFactor;

        static Spectrum()
        {
            var configuration = ComponentRegistry.Instance.GetComponent<IConfiguration>();
            if (configuration == null)
            {
                return;
            }
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
            FFTSize = configuration.GetElement<SelectionConfigurationElement>(
                SpectrumBehaviourConfiguration.SECTION,
                SpectrumBehaviourConfiguration.FFT_SIZE_ELEMENT
            );
            Amplitude = configuration.GetElement<IntegerConfigurationElement>(
                SpectrumBehaviourConfiguration.SECTION,
                SpectrumBehaviourConfiguration.AMPLITUDE_ELEMENT
            );
            ScalingFactor = configuration.GetElement<DoubleConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                WindowsUserInterfaceConfiguration.UI_SCALING_ELEMENT
            );
        }

        public Spectrum()
        {
            this.Debouncer = new Debouncer(TIMEOUT);
            this.InitializeComponent();
            BarCount.ValueChanged += this.OnValueChanged;
            ShowPeaks.ValueChanged += this.OnValueChanged;
            HighCut.ValueChanged += this.OnValueChanged;
            Smooth.ValueChanged += this.OnValueChanged;
            SmoothFactor.ValueChanged += this.OnValueChanged;
            HoldInterval.ValueChanged += this.OnValueChanged;
            UpdateInterval.ValueChanged += this.OnValueChanged;
            FFTSize.ValueChanged += this.OnValueChanged;
            Amplitude.ValueChanged += this.OnValueChanged;
            ScalingFactor.ValueChanged += this.OnValueChanged;
        }

        public Debouncer Debouncer { get; private set; }

        public SpectrumRenderer Renderer { get; private set; }

        protected virtual void OnValueChanged(object sender, EventArgs e)
        {
            this.Debouncer.Exec(this.UpdateRenderer);
        }

        protected virtual void UpdateRenderer()
        {
            if (this.Renderer != null)
            {
                Logger.Write(this, LogLevel.Debug, "Settings were updated, releasing current renderer.");
                this.Renderer.Dispose();
                this.Renderer = null;
            }

            var task = Windows.Invoke(() =>
            {
                if (!(this.DataContext is ICore core))
                {
                    //We need a core.
                    return;
                }

                //Fix the width so all 2d math is integer.
                this.MinWidth = SpectrumBehaviourConfiguration.GetWidth(BarCount.Value);

                var width = this.ActualWidth;
                var height = this.ActualHeight;
                if (width < this.MinWidth || double.IsNaN(width) || height == 0 || double.IsNaN(height))
                {
                    //We need proper dimentions.
                    return;
                }
                var flags = SpectrumRendererFlags.None;
                if (ShowPeaks.Value)
                {
                    flags |= SpectrumRendererFlags.ShowPeaks;
                }
                if (HighCut.Value)
                {
                    flags |= SpectrumRendererFlags.HighCut;
                }
                if (Smooth.Value)
                {
                    flags |= SpectrumRendererFlags.Smooth;
                }
                var color = default(Color);
                if (this.Foreground is SolidColorBrush brush)
                {
                    color = brush.Color;
                }
                else
                {
                    color = Colors.Black;
                }

                var size = Windows.ActiveWindow.GetElementPixelSize(
                    width * ScalingFactor.Value,
                    height * ScalingFactor.Value
                );

                this.Renderer = new SpectrumRenderer(
                    Convert.ToInt32(size.Width),
                    Convert.ToInt32(size.Height),
                    SpectrumBehaviourConfiguration.GetBars(BarCount.Value),
                    SpectrumBehaviourConfiguration.GetFFTSize(FFTSize.Value),
                    UpdateInterval.Value,
                    HoldInterval.Value,
                    SmoothFactor.Value,
                    Amplitude.Value,
                    color,
                    flags
                );
                this.Renderer.InitializeComponent(core);
                this.Background = new ImageBrush()
                {
                    ImageSource = this.Renderer.Bitmap
                };
            });
        }

        protected virtual void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.Debouncer.Exec(this.UpdateRenderer);
        }

        protected virtual void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (this.Renderer != null)
            {
                Logger.Write(this, LogLevel.Debug, "Unloaded, releasing current renderer.");
                this.Background = null;
                this.Renderer.Dispose();
                this.Renderer = null;
            }
        }

        protected virtual void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.Renderer != null)
            {
                if (this.Renderer.Data.Width == this.ActualWidth && this.Renderer.Data.Height == this.ActualHeight)
                {
                    return;
                }
            }
            this.Debouncer.Exec(this.UpdateRenderer);
        }
    }
}