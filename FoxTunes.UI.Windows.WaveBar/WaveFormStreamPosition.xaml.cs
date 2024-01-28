using FoxTunes.Interfaces;
using System;
using System.Windows;
using System.Windows.Media;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for WaveFormStreamPosition.xaml
    /// </summary>
    [UIComponent("1CABFBE6-C5BD-4818-A092-2D79509D3A52", "Wave Form Seekbar")]
    public partial class WaveFormStreamPosition : UIComponentBase
    {
        const int TIMEOUT = 1000;

        const int RESOLUTION = 1;

        public static readonly SelectionConfigurationElement Mode;

        public static readonly DoubleConfigurationElement ScalingFactor;

        static WaveFormStreamPosition()
        {
            var configuration = ComponentRegistry.Instance.GetComponent<IConfiguration>();
            if (configuration == null)
            {
                return;
            }
            Mode = configuration.GetElement<SelectionConfigurationElement>(
                WaveBarBehaviourConfiguration.SECTION,
                WaveBarBehaviourConfiguration.MODE_ELEMENT
            );
            ScalingFactor = configuration.GetElement<DoubleConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                WindowsUserInterfaceConfiguration.UI_SCALING_ELEMENT
            );
        }

        public WaveFormStreamPosition()
        {
            this.Debouncer = new Debouncer(TIMEOUT);
            this.InitializeComponent();
            Mode.ValueChanged += this.OnValueChanged;
            ScalingFactor.ValueChanged += this.OnValueChanged;
        }

        public Debouncer Debouncer { get; private set; }

        public WaveFormRenderer Renderer { get; private set; }

        protected virtual void OnValueChanged(object sender, EventArgs e)
        {
            this.Debouncer.Exec(this.UpdateRenderer);
        }

        protected virtual void UpdateRenderer()
        {
            var task = Windows.Invoke(() =>
            {
                if (!(this.DataContext is ICore core))
                {
                    //We need a core.
                    return;
                }

                if (this.Renderer == null)
                {
                    this.Renderer = new WaveFormRenderer(RESOLUTION);
                    this.Renderer.InitializeComponent(core);
                }

                this.UpdateBitmap();
            });
        }

        protected virtual void UpdateBitmap()
        {
            var width = this.ActualWidth;
            var height = this.ActualHeight;
            if (width < this.MinWidth || double.IsNaN(width) || height == 0 || double.IsNaN(height))
            {
                //We need proper dimentions.
                return;
            }

            var size = Windows.ActiveWindow.GetElementPixelSize(
                width * ScalingFactor.Value,
                height * ScalingFactor.Value
            );

            this.UpdateBitmap(size);
        }

        protected virtual void UpdateBitmap(Size size)
        {
            var color = default(Color);
            if (this.Foreground is SolidColorBrush brush)
            {
                color = brush.Color;
            }
            else
            {
                color = Colors.Black;
            }

            var bitmap = this.Renderer.CreateBitmap(
                Convert.ToInt32(size.Width),
                Convert.ToInt32(size.Height),
                color,
                WaveBarBehaviourConfiguration.GetMode(Mode.Value)
            );

            this.Background = new ImageBrush()
            {
                ImageSource = bitmap
            };

            this.Dispatch(this.Renderer.Update);
        }

        protected virtual void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.Debouncer.ExecNow(this.UpdateRenderer);
        }

        protected virtual void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (this.Renderer != null)
            {
                Logger.Write(this, LogLevel.Debug, "Unloaded, releasing current renderer.");
                this.Background = null;
            }
        }

        protected virtual void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.Renderer != null && this.Renderer.Bitmap != null)
            {
                var width = this.ActualWidth;
                var height = this.ActualHeight;
                if (width < this.MinWidth || double.IsNaN(width) || height == 0 || double.IsNaN(height))
                {
                    //We need proper dimentions.
                    return;
                }

                var size = Windows.ActiveWindow.GetElementPixelSize(
                    width * ScalingFactor.Value,
                    height * ScalingFactor.Value
                );

                if (this.Renderer.Bitmap.Width == size.Width && this.Renderer.Bitmap.Height == size.Height)
                {
                    //Nothing to do.
                    return;
                }
            }
            this.Debouncer.Exec(this.UpdateRenderer);
        }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.IsDisposed || !disposing)
            {
                return;
            }
            this.OnDisposing();
            this.IsDisposed = true;
        }

        protected virtual void OnDisposing()
        {
            if (this.Debouncer != null)
            {
                this.Debouncer.Dispose();
            }
        }

        ~WaveFormStreamPosition()
        {
            try
            {
                this.Dispose(true);
            }
            catch
            {
                //Nothing can be done, never throw on GC thread.
            }
        }
    }
}
