using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace FoxTunes
{
    public abstract class RendererBase : Freezable, IBaseComponent, INotifyPropertyChanged, IDisposable
    {
        public const int DB_MIN = -90;

        public const int DB_MAX = 0;

        public const int ROLLOFF_INTERVAL = 500;

        public const DispatcherPriority DISPATCHER_PRIORITY = DispatcherPriority.Render;

        protected static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
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

        const int TIMEOUT = 100;

        public RendererBase(bool initialize = true)
        {
            this.Debouncer = new AsyncDebouncer(TIMEOUT);
            if (initialize && Core.Instance != null)
            {
                this.InitializeComponent(Core.Instance);
            }
        }

        public AsyncDebouncer Debouncer { get; private set; }

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
            this.Debouncer.Exec(this.CreateBitmap);
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
            this.Debouncer.Exec(this.CreateBitmap);
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

        public virtual void InitializeComponent(ICore core)
        {
            this.Output = core.Components.Output;
            this.Output.IsStartedChanged += this.OnIsStartedChanged;
            this.Configuration = core.Components.Configuration;
            this.ScalingFactor = this.Configuration.GetElement<DoubleConfigurationElement>(
               WindowsUserInterfaceConfiguration.SECTION,
               WindowsUserInterfaceConfiguration.UI_SCALING_ELEMENT
            );
            this.ScalingFactor.ValueChanged += this.OnScalingFactorChanged;
        }

        protected virtual void OnIsStartedChanged(object sender, AsyncEventArgs e)
        {
            if (!this.Output.IsStarted)
            {
                var task = this.Clear();
            }
        }

        protected virtual void OnScalingFactorChanged(object sender, EventArgs e)
        {
            var task = this.CreateBitmap();
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

                this.Bitmap = this.CreateBitmap(size);

                this.CreateViewBox();
            });
        }

        protected virtual WriteableBitmap CreateBitmap(Size size)
        {
            return new WriteableBitmap(
                Convert.ToInt32(size.Width),
                Convert.ToInt32(size.Height),
                96,
                96,
                PixelFormats.Pbgra32,
                null
            );
        }

        protected abstract void CreateViewBox();

        protected virtual Task Clear()
        {
            return Windows.Invoke(() =>
            {
                var bitmap = this.Bitmap;
                if (bitmap == null)
                {
                    return;
                }
                if (!bitmap.TryLock(LockTimeout))
                {
                    return;
                }
                BitmapHelper.Clear(BitmapHelper.CreateRenderInfo(bitmap, this.Color));
                bitmap.AddDirtyRect(new global::System.Windows.Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
                bitmap.Unlock();
            });
        }

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
            if (this.ScalingFactor != null)
            {
                this.ScalingFactor.ValueChanged -= this.OnScalingFactorChanged;
            }
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

        protected static float ToDecibel(float value)
        {
            return Math.Min(Math.Max((float)(20 * Math.Log10(value)), DB_MIN), DB_MAX);
        }

        protected static float ToDecibelFixed(float value)
        {
            var dB = ToDecibel(value);
            return 1.0f - Math.Abs(dB) / Math.Abs(DB_MIN);
        }

        protected static float ToCrestFactor(float value, float rms, float offset)
        {
            return Math.Min(Math.Max((value - rms) + offset, 0), 1);
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
                var step = Math.Max(height / values.Length, 1);
                for (var a = 0; a < values.Length; a++)
                {
                    var barWidth = Math.Max(Convert.ToInt32(values[a] * width), 1);
                    elements[a].X = 0;
                    elements[a].Y = a * step;
                    elements[a].Height = step;
                    Animate(ref elements[a].Width, barWidth, 1, width, smoothing);
                }
            }
            else if (orientation == Orientation.Vertical)
            {
                var step = Math.Max(width / values.Length, 1);
                for (var a = 0; a < values.Length; a++)
                {
                    var barHeight = Math.Max(Convert.ToInt32(values[a] * height), 1);
                    elements[a].X = a * step;
                    elements[a].Width = step;
                    Animate(ref elements[a].Height, barHeight, 1, height, smoothing);
                    elements[a].Y = height - elements[a].Height;
                }
            }
        }

        protected static void UpdateElementsSmooth(Int32Rect[] elements, Int32Rect[] peaks, int[] holds, int width, int height, int interval, int duration, Orientation orientation)
        {
            if (elements.Length == 0)
            {
                return;
            }
            if (orientation == Orientation.Horizontal)
            {
                var fast = width / 4;
                var step = Math.Max(height / elements.Length, 1);
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
                var step = Math.Max(width / elements.Length, 1);
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

        protected static void UpdateElementsSmooth(Int32Rect[][] elements, Int32Rect[] peaks, int[] holds, int width, int height, int interval, int duration, Orientation orientation)
        {
            if (elements.Length == 0)
            {
                return;
            }
            //TODO: Assuming all arrays are the same length.
            var length = elements[0].Length;
            if (length == 0)
            {
                return;
            }
            if (orientation == Orientation.Horizontal)
            {
                var fast = width / 4;
                var step = height / length;
                for (int a = 0; a < length; a++)
                {
                    var target = elements.Max(sequence => sequence[a].Width);
                    peaks[a].Y = a * step;
                    peaks[a].Width = 1;
                    peaks[a].Height = step;
                    if (target > peaks[a].X)
                    {
                        peaks[a].X = target;
                        holds[a] = interval + ROLLOFF_INTERVAL;
                    }
                    else if (target < peaks[a].X)
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
                var step = width / length;
                for (int a = 0; a < length; a++)
                {
                    var target = elements.Min(sequence => sequence[a].Y);
                    peaks[a].X = a * step;
                    peaks[a].Width = step;
                    peaks[a].Height = 1;
                    if (target < peaks[a].Y)
                    {
                        peaks[a].Y = target;
                        holds[a] = interval + ROLLOFF_INTERVAL;
                    }
                    else if (target > peaks[a].Y)
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

        protected static void Animate(ref int value, int target, int min, int max, int smoothing)
        {
            var difference = Math.Abs(value - target);
            if (difference == 0)
            {
                //Nothing to do.
                return;
            }

            var distance = default(float);
            if (target > 0)
            {
                distance = (float)difference / target;
            }
            else
            {
                distance = (float)difference / (max - target);
            }

            var increment = 1 - (float)Math.Pow(1 - distance, 3);
            var fast = Math.Max(Math.Min((float)Math.Abs(max - min) / smoothing, 10), 1);
            var smoothed = Math.Min(Math.Max((float)Math.Floor(fast * increment), 1), fast);
            if (target > value)
            {
                value = (int)Math.Min(value + smoothed, max);
            }
            else if (target < value)
            {
                value = (int)Math.Max(value - smoothed, min);
            }
        }

        protected static void NoiseReduction(float[,] values, int countx, int county, int smoothing)
        {
            var value = default(float);
            for (var y = 0; y < county; y++)
            {
                var start = Math.Max(y - smoothing, 0);
                var end = Math.Min(y + smoothing, county);
                for (var x = 0; x < countx; x++)
                {
                    value = 0;
                    for (var a = start; a < end; a++)
                    {
                        value += values[x, a];
                    }
                    value /= end - start;
                    values[x, y] = value;
                }
            }
        }

        protected static int Deinterlace(float[,] destination, float[] source, int channels, int count)
        {
            for (int a = 0, b = 0; a < count; a += channels, b++)
            {
                for (var channel = 0; channel < channels; channel++)
                {
                    destination[channel, b] = source[a + channel];
                }
            }
            return count / channels;
        }

        protected static int DownmixMono(float[,] destination, float[] source, int channels, int count)
        {
            for (int a = 0, b = 0; a < count; a += channels, b++)
            {
                destination[0, b] = 0f;
                for (var channel = 0; channel < channels; channel++)
                {
                    destination[0, b] += source[a + channel];
                }
                destination[0, b] /= channels;
            }
            return count / channels;
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

        public static Color[] ToGradient(this KeyValuePair<int, Color>[] colors)
        {
            var min = colors.Min(color => color.Key);
            var max = colors.Max(color => color.Key);
            var result = new Color[max];
            for (var a = 0; a < colors.Length; a++)
            {
                var previousOffset = 0;
                var previousColor = default(Color);
                if (a == 0)
                {
                    previousOffset = 0;
                    previousColor = Colors.Black;
                }
                else
                {
                    previousOffset = colors[a - 1].Key;
                    previousColor = colors[a - 1].Value;
                }
                var nextOffset = 0;
                var nextColor = default(Color);
                if (a == colors.Length - 1)
                {
                    nextOffset = max;
                    nextColor = Colors.White;
                }
                else
                {
                    nextOffset = colors[a + 1].Key;
                    nextColor = colors[a + 1].Value;
                }
                var offset = colors[a].Key;
                var color = colors[a].Value;
                for (int b = previousOffset; b < offset; b++)
                {
                    result[b] = Interpolate(previousColor, color, (float)b / (offset - 1));
                }
            }
            return result;
        }

        public static Color Interpolate(Color color1, Color color2, float ratio)
        {
            var ratio1 = 1 - ratio;
            var ratio2 = ratio;
            return Color.FromArgb(
                (byte)Math.Round(color1.A * ratio1 + color2.A * ratio2),
                (byte)Math.Round(color1.R * ratio1 + color2.R * ratio2),
                (byte)Math.Round(color1.G * ratio1 + color2.G * ratio2),
                (byte)Math.Round(color1.B * ratio1 + color2.B * ratio2)
            );
        }
    }
}
