using FoxTunes.Interfaces;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FoxTunes
{
    public abstract class RendererBase : Freezable, IBaseComponent, INotifyPropertyChanged, IDisposable
    {
        public const int ROLLOFF_INTERVAL = 500;

        protected static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        public static readonly DependencyProperty CoreProperty = DependencyProperty.Register(
            "Core",
            typeof(ICore),
            typeof(RendererBase),
            new PropertyMetadata(new PropertyChangedCallback(OnCoreChanged))
        );

        public static ICore GetCore(RendererBase source)
        {
            return (ICore)source.GetValue(CoreProperty);
        }

        public static void SetCore(RendererBase source, ICore value)
        {
            source.SetValue(CoreProperty, value);
        }

        public static void OnCoreChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var renderer = sender as RendererBase;
            if (renderer == null)
            {
                return;
            }
            if (renderer.Core != null && !renderer.IsInitialized)
            {
                renderer.InitializeComponent(renderer.Core);
            }
            renderer.OnCoreChanged();
        }


        public static readonly Duration LockTimeout = new Duration(TimeSpan.FromMilliseconds(1));

        public static readonly DependencyProperty BitmapProperty = DependencyProperty.Register(
            "Bitmap",
            typeof(WriteableBitmap),
            typeof(RendererBase),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnBitmapChanged))
        );

        public static WriteableBitmap GetBitmap(RendererBase source)
        {
            return (WriteableBitmap)source.GetValue(BitmapProperty);
        }

        public static void SetBitmap(RendererBase source, WriteableBitmap value)
        {
            source.SetValue(BitmapProperty, value);
        }

        public static void OnBitmapChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var renderer = sender as RendererBase;
            if (renderer == null)
            {
                return;
            }
            renderer.OnBitmapChanged();
        }

        public static readonly DependencyProperty WidthProperty = DependencyProperty.Register(
            "Width",
            typeof(double),
            typeof(RendererBase),
            new FrameworkPropertyMetadata(double.NaN, new PropertyChangedCallback(OnWidthChanged))
        );

        public static double GetWidth(RendererBase source)
        {
            return (double)source.GetValue(WidthProperty);
        }

        public static void SetWidth(RendererBase source, double value)
        {
            source.SetValue(WidthProperty, value);
        }

        public static void OnWidthChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var renderer = sender as RendererBase;
            if (renderer == null)
            {
                return;
            }
            renderer.OnWidthChanged();
        }

        public static readonly DependencyProperty HeightProperty = DependencyProperty.Register(
           "Height",
           typeof(double),
           typeof(RendererBase),
           new FrameworkPropertyMetadata(double.NaN, new PropertyChangedCallback(OnHeightChanged))
       );

        public static double GetHeight(RendererBase source)
        {
            return (double)source.GetValue(HeightProperty);
        }

        public static void SetHeight(RendererBase source, double value)
        {
            source.SetValue(HeightProperty, value);
        }

        public static void OnHeightChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var renderer = sender as RendererBase;
            if (renderer == null)
            {
                return;
            }
            renderer.OnHeightChanged();
        }

        public static readonly DependencyProperty ViewboxProperty = DependencyProperty.Register(
           "Viewbox",
           typeof(Rect),
           typeof(RendererBase),
           new FrameworkPropertyMetadata(new Rect(0, 0, 1, 1), new PropertyChangedCallback(OnViewboxChanged))
       );

        public static Rect GetViewbox(RendererBase source)
        {
            return (Rect)source.GetValue(ViewboxProperty);
        }

        protected static void SetViewbox(RendererBase source, Rect value)
        {
            source.SetValue(ViewboxProperty, value);
        }

        public static void OnViewboxChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var renderer = sender as RendererBase;
            if (renderer == null)
            {
                return;
            }
            renderer.OnViewboxChanged();
        }

        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(
            "Color",
            typeof(Color),
            typeof(RendererBase),
            new FrameworkPropertyMetadata(Colors.Transparent, new PropertyChangedCallback(OnColorChanged))
        );

        public static Color GetColor(RendererBase source)
        {
            return (Color)source.GetValue(ColorProperty);
        }

        public static void SetColor(RendererBase source, Color value)
        {
            source.SetValue(ColorProperty, value);
        }

        public static void OnColorChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var renderer = sender as RendererBase;
            if (renderer == null)
            {
                return;
            }
            renderer.OnColorChanged();
        }


        public ICore Core
        {
            get
            {
                return this.GetValue(CoreProperty) as ICore;
            }
            set
            {
                this.SetValue(CoreProperty, value);
            }
        }

        protected virtual void OnCoreChanged()
        {
            if (this.CoreChanged != null)
            {
                this.CoreChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Core");
        }

        public event EventHandler CoreChanged;


        public WriteableBitmap Bitmap
        {
            get
            {
                return (WriteableBitmap)this.GetValue(BitmapProperty);
            }
            set
            {
                this.SetValue(BitmapProperty, value);
            }
        }

        protected virtual void OnBitmapChanged()
        {
            if (this.BitmapChanged != null)
            {
                this.BitmapChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Bitmap");
        }

        public event EventHandler BitmapChanged;

        public double Width
        {
            get
            {
                return (double)this.GetValue(WidthProperty);
            }
            set
            {
                this.SetValue(WidthProperty, value);
            }
        }

        protected virtual void OnWidthChanged()
        {
            if (this.IsInitialized)
            {
                var task = this.CreateBitmap();
            }
            if (this.WidthChanged != null)
            {
                this.WidthChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Width");
        }

        public event EventHandler WidthChanged;

        public double Height
        {
            get
            {
                return (double)this.GetValue(HeightProperty);
            }
            set
            {
                this.SetValue(HeightProperty, value);
            }
        }

        protected virtual void OnHeightChanged()
        {
            if (this.IsInitialized)
            {
                var task = this.CreateBitmap();
            }
            if (this.HeightChanged != null)
            {
                this.HeightChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Height");
        }

        public event EventHandler HeightChanged;

        public Rect Viewbox
        {
            get
            {
                return (Rect)this.GetValue(ViewboxProperty);
            }
            protected set
            {
                this.SetValue(ViewboxProperty, value);
            }
        }

        protected virtual void OnViewboxChanged()
        {
            if (this.ViewboxChanged != null)
            {
                this.ViewboxChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Viewbox");
        }

        public event EventHandler ViewboxChanged;

        public Color Color
        {
            get
            {
                return (Color)this.GetValue(ColorProperty);
            }
            set
            {
                this.SetValue(ColorProperty, value);
            }
        }

        protected virtual void OnColorChanged()
        {
            if (this.IsInitialized)
            {
                var task = this.CreateBitmap();
            }
            if (this.ColorChanged != null)
            {
                this.ColorChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Color");
        }

        public event EventHandler ColorChanged;

        public IOutput Output { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public DoubleConfigurationElement ScalingFactor { get; private set; }

        public bool IsInitialized { get; private set; }

        public virtual void InitializeComponent(ICore core)
        {
            this.Output = core.Components.Output;
            this.Configuration = core.Components.Configuration;
            this.ScalingFactor = this.Configuration.GetElement<DoubleConfigurationElement>(
               WindowsUserInterfaceConfiguration.SECTION,
               WindowsUserInterfaceConfiguration.UI_SCALING_ELEMENT
            );
            this.IsInitialized = true;
        }

        protected virtual Task CreateBitmap()
        {
            return Windows.Invoke(() =>
           {
               var width = this.Width;
               var height = this.Height;
               if (width == 0 || double.IsNaN(width) || height == 0 || double.IsNaN(height))
               {
                   //We need proper dimentions.
                   return;
               }

               var size = Windows.ActiveWindow.GetElementPixelSize(
                   width * this.ScalingFactor.Value,
                   height * this.ScalingFactor.Value
               );

               this.Bitmap = new WriteableBitmap(
                   Convert.ToInt32(size.Width),
                   Convert.ToInt32(size.Height),
                   96,
                   96,
                   PixelFormats.Pbgra32,
                   null
               );

               this.CreateViewBox();
           });
        }

        protected abstract void CreateViewBox();

        protected virtual void Dispatch(Action action)
        {
#if NET40
            var task = TaskEx.Run(action);
#else
            var task = Task.Run(action);
#endif
        }

        protected virtual void Dispatch(Func<Task> function)
        {
#if NET40
            var task = TaskEx.Run(function);
#else
            var task = Task.Run(function);
#endif
        }

        protected virtual void OnPropertyChanging(string propertyName)
        {
            if (this.PropertyChanging == null)
            {
                return;
            }
            this.PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
        }

        public event PropertyChangingEventHandler PropertyChanging;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged == null)
            {
                return;
            }
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual Task OnError(string message, Exception exception)
        {
            Logger.Write(this, LogLevel.Error, message, exception);
            if (Error == null)
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            return Error(this, new ComponentErrorEventArgs(message, exception));
        }

        event ComponentErrorEventHandler IBaseComponent.Error
        {
            add
            {
                Error += value;
            }
            remove
            {
                Error -= value;
            }
        }

        public static event ComponentErrorEventHandler Error;

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
            //Nothing to do.
        }

        ~RendererBase()
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

        protected static void UpdateElementsFast(float[] values, Int32Rect[] elements, int width, int height, Orientation orientation)
        {
            if (values.Length == 0)
            {
                return;
            }
            if (orientation == Orientation.Horizontal)
            {
                var step = height / values.Length;
                for (var a = 0; a < values.Length; a++)
                {
                    var barWidth = Convert.ToInt32(values[a] * width);
                    elements[a].X = 0;
                    elements[a].Y = a * step;
                    elements[a].Height = step;
                    if (barWidth > 0)
                    {
                        elements[a].Width = barWidth;
                    }
                    else
                    {
                        elements[a].Width = 1;
                    }
                }
            }
            else if (orientation == Orientation.Vertical)
            {
                var step = width / values.Length;
                for (var a = 0; a < values.Length; a++)
                {
                    var barHeight = Convert.ToInt32(values[a] * height);
                    elements[a].X = a * step;
                    elements[a].Width = step;
                    if (barHeight > 0)
                    {
                        elements[a].Height = barHeight;
                    }
                    else
                    {
                        elements[a].Height = 1;
                    }
                    elements[a].Y = height - elements[a].Height;
                }
            }
        }

        protected static void UpdateElementsSmooth(float[] values, Int32Rect[] elements, int width, int height, int smoothing, Orientation orientation)
        {
            if (values.Length == 0)
            {
                return;
            }
            if (orientation == Orientation.Horizontal)
            {
                var step = height / values.Length;
                var fast = Math.Min((float)width / smoothing, 10);
                for (var a = 0; a < values.Length; a++)
                {
                    var barWidth = Math.Max(Convert.ToInt32(values[a] * width), 1);
                    elements[a].X = 0;
                    elements[a].Y = a * step;
                    elements[a].Height = step;
                    var difference = Math.Abs(elements[a].Width - barWidth);
                    if (difference > 0)
                    {
                        if (difference < fast)
                        {
                            if (barWidth > elements[a].Width)
                            {
                                elements[a].Width++;
                            }
                            else if (barWidth < elements[a].Width)
                            {
                                elements[a].Width--;
                            }
                        }
                        else
                        {
                            var distance = (float)difference / width;
                            var increment = Math.Sqrt(1 - Math.Pow(distance - 1, 2));
                            var smoothed = Math.Min(Math.Max(fast * increment, 1), fast);
                            if (barWidth > elements[a].Width)
                            {
                                elements[a].Width = (int)Math.Min(elements[a].Width + smoothed, width);
                            }
                            else if (barWidth < elements[a].Width)
                            {
                                elements[a].Width = (int)Math.Max(elements[a].Width - smoothed, 1);
                            }
                        }
                    }
                }
            }
            else if (orientation == Orientation.Vertical)
            {
                var step = width / values.Length;
                var fast = Math.Min((float)height / smoothing, 10);
                for (var a = 0; a < values.Length; a++)
                {
                    var barHeight = Math.Max(Convert.ToInt32(values[a] * height), 1);
                    elements[a].X = a * step;
                    elements[a].Width = step;
                    var difference = Math.Abs(elements[a].Height - barHeight);
                    if (difference > 0)
                    {
                        if (difference < fast)
                        {
                            if (barHeight > elements[a].Height)
                            {
                                elements[a].Height++;
                            }
                            else if (barHeight < elements[a].Height)
                            {
                                elements[a].Height--;
                            }
                        }
                        else
                        {
                            var distance = (float)difference / height;
                            var increment = Math.Sqrt(1 - Math.Pow(distance - 1, 2));
                            var smoothed = Math.Min(Math.Max(fast * increment, 1), fast);
                            if (barHeight > elements[a].Height)
                            {
                                elements[a].Height = (int)Math.Min(elements[a].Height + smoothed, height);
                            }
                            else if (barHeight < elements[a].Height)
                            {
                                elements[a].Height = (int)Math.Max(elements[a].Height - smoothed, 1);
                            }
                        }
                    }
                    elements[a].Y = height - elements[a].Height;
                }
            }
        }

        protected static void UpdateElementsSmooth(Int32Rect[] elements, Int32Rect[] peaks, int[] holds, int width, int height, int interval, int duration, Orientation orientation)
        {
            if (orientation == Orientation.Horizontal)
            {
                var fast = width / 4;
                var step = height / elements.Length;
                for (int a = 0; a < elements.Length; a++)
                {
                    peaks[a].Y = a * step;
                    peaks[a].Width = 1;
                    peaks[a].Height = step;
                    if (elements[a].Width > peaks[a].X)
                    {
                        peaks[a].X = elements[a].Width;
                        holds[a] = interval + ROLLOFF_INTERVAL;
                    }
                    else if (elements[a].Width < peaks[a].X)
                    {
                        if (holds[a] > 0)
                        {
                            if (holds[a] < interval)
                            {
                                var distance = 1 - ((float)holds[a] / interval);
                                var increment = fast * (distance * distance * distance);
                                if (peaks[a].X > increment)
                                {
                                    peaks[a].X -= (int)Math.Round(increment);
                                }
                                else if (peaks[a].X > 0)
                                {
                                    peaks[a].X = 0;
                                }
                            }
                            holds[a] -= duration;
                        }
                        else if (peaks[a].X > fast)
                        {
                            peaks[a].X -= fast;
                        }
                        else if (peaks[a].X > 0)
                        {
                            peaks[a].X = 0;
                        }
                    }
                }
            }
            else if (orientation == Orientation.Vertical)
            {
                var fast = height / 4;
                var step = width / elements.Length;
                for (int a = 0; a < elements.Length; a++)
                {
                    peaks[a].X = a * step;
                    peaks[a].Width = step;
                    peaks[a].Height = 1;
                    if (elements[a].Y < peaks[a].Y)
                    {
                        peaks[a].Y = elements[a].Y;
                        holds[a] = interval + ROLLOFF_INTERVAL;
                    }
                    else if (elements[a].Y > peaks[a].Y)
                    {
                        if (holds[a] > 0)
                        {
                            if (holds[a] < interval)
                            {
                                var distance = 1 - ((float)holds[a] / interval);
                                var increment = fast * (distance * distance * distance);
                                if (peaks[a].Y < height - increment)
                                {
                                    peaks[a].Y += (int)Math.Round(increment);
                                }
                                else if (peaks[a].Y < height - 1)
                                {
                                    peaks[a].Y = height - 1;
                                }
                            }
                            holds[a] -= duration;
                        }
                        else if (peaks[a].Y < height - fast)
                        {
                            peaks[a].Y += fast;
                        }
                        else if (peaks[a].Y < height - 1)
                        {
                            peaks[a].Y = height - 1;
                        }
                    }
                }
            }
        }
    }

    public static partial class Extensions
    {
        public static bool IsLighter(this Color color)
        {
            return color.A > byte.MaxValue / 2 && color.R > byte.MaxValue / 2 && color.G > byte.MaxValue / 2 && color.B > byte.MaxValue / 2;
        }

        public static Color Shade(this Color color1, Color color2)
        {
            if (color1.IsLighter())
            {
                //Create darner shade.
                return new Color()
                {
                    A = Convert.ToByte(Math.Min(color1.A - color2.A, byte.MaxValue)),
                    R = Convert.ToByte(Math.Min(color1.R - color2.R, byte.MaxValue)),
                    G = Convert.ToByte(Math.Min(color1.G - color2.G, byte.MaxValue)),
                    B = Convert.ToByte(Math.Min(color1.B - color2.B, byte.MaxValue))
                };
            }
            else
            {
                //Create lighter shade.
                return new Color()
                {
                    A = Convert.ToByte(Math.Min(color1.A + color2.A, byte.MaxValue)),
                    R = Convert.ToByte(Math.Min(color1.R + color2.R, byte.MaxValue)),
                    G = Convert.ToByte(Math.Min(color1.G + color2.G, byte.MaxValue)),
                    B = Convert.ToByte(Math.Min(color1.B + color2.B, byte.MaxValue))
                };
            }
        }

        public static Color[] ToPair(this Color color, byte shade)
        {
            var contrast = new Color()
            {
                R = shade,
                G = shade,
                B = shade
            };
            if (color.IsLighter())
            {
                return new[]
                {
                    color.Shade(contrast),
                    color
                };
            }
            else
            {
                return new[]
                {
                    color,
                    color.Shade(contrast)
                };
            }
        }
    }
}
