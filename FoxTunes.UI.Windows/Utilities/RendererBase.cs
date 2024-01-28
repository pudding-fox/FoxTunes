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
    public abstract class RendererBase : FrameworkElement, IBaseComponent, INotifyPropertyChanged, IDisposable
    {
        public const double DPIX = 96;

        public const double DPIY = 96;

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

        public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(
            "Background",
            typeof(Brush),
            typeof(RendererBase),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender)
        );

        public Brush GetBackground(RendererBase source)
        {
            return (Brush)source.GetValue(BackgroundProperty);
        }

        public void SetBackground(RendererBase source, Brush value)
        {
            source.SetValue(BackgroundProperty, value);
        }

        public static readonly DependencyProperty ForegroundProperty = Control.ForegroundProperty.AddOwner(
            typeof(RendererBase),
            new FrameworkPropertyMetadata(SystemColors.ControlTextBrush, FrameworkPropertyMetadataOptions.Inherits)
        );

        public Brush GetForeground(RendererBase source)
        {
            return (Brush)source.GetValue(ForegroundProperty);
        }

        public void SetForeground(RendererBase source, Brush value)
        {
            source.SetValue(ForegroundProperty, value);
        }

        const int TIMEOUT = 100;

        protected RendererBase(bool initialize = true)
        {
            this.Debouncer = new AsyncDebouncer(TIMEOUT);
            if (initialize && Core.Instance != null)
            {
                this.InitializeComponent(Core.Instance);
            }
        }

        public AsyncDebouncer Debouncer { get; private set; }

        public Brush Background
        {
            get
            {
                return GetBackground(this);
            }
            set
            {
                SetBackground(this, value);
            }
        }

        public Brush Foreground
        {
            get
            {
                return GetForeground(this);
            }
            set
            {
                SetForeground(this, value);
            }
        }

        public WriteableBitmap Bitmap
        {
            get
            {
                if (this.Background is ImageBrush brush)
                {
                    return brush.ImageSource as WriteableBitmap;
                }
                return null;
            }
            set
            {
                if (value != null)
                {
                    var brush = new ImageBrush(value);
                    this.Background = brush;
                }
                else
                {
                    this.Background = null;
                }
            }
        }

        public Color Color
        {
            get
            {
                if (this.Foreground is SolidColorBrush brush)
                {
                    return brush.Color;
                }
                return Colors.Transparent;
            }
        }

        public PixelSizeConverter PixelSizeConverter { get; private set; }

        public IOutput Output { get; private set; }

        public virtual void InitializeComponent(ICore core)
        {
            this.PixelSizeConverter = ComponentRegistry.Instance.GetComponent<PixelSizeConverter>();
            this.Output = core.Components.Output;
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            this.Debouncer.Exec(this.CreateBitmap);
            base.OnRenderSizeChanged(sizeInfo);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawRectangle(this.Background, null, new Rect(0, 0, this.ActualWidth, this.ActualHeight));
        }

        protected virtual Task CreateBitmap()
        {
            return Windows.Invoke(() =>
            {
                var width = default(int);
                var height = default(int);
                if (!this.GetPixelSize(out width, out height))
                {
                    return;
                }
                if (!this.CreateData(width, height))
                {
                    return;
                }
                this.Bitmap = this.CreateBitmap(
                    width,
                    height
                );
            });
        }

        protected virtual WriteableBitmap CreateBitmap(int width, int height)
        {
            return new WriteableBitmap(
                width,
                height,
                DPIX,
                DPIY,
                PixelFormats.Pbgra32,
                null
            );
        }

        protected virtual Task RefreshBitmap()
        {
            return Windows.Invoke(() =>
            {
                var width = default(int);
                var height = default(int);
                if (!this.GetPixelSize(out width, out height))
                {
                    return;
                }
                if (!this.CreateData(width, height))
                {
                    return;
                }
                var bitmap = this.Bitmap;
                if (bitmap != null && bitmap.PixelWidth == width && bitmap.PixelHeight == height)
                {
                    return;
                }
                this.Bitmap = this.CreateBitmap(
                    width,
                    height
                );
            });
        }

        protected virtual Task CreateData()
        {
            return Windows.Invoke(() =>
            {
                if (this.Bitmap == null)
                {
                    return;
                }
                this.CreateData(this.Bitmap.PixelWidth, this.Bitmap.PixelHeight);
            });
        }

        protected abstract bool CreateData(int width, int height);

        protected virtual bool GetPixelSize(out int width, out int height)
        {
            var actualWidth = this.ActualWidth;
            var actualHeight = this.ActualHeight;
            if (actualWidth == 0 || double.IsNaN(actualWidth) || actualHeight == 0 || double.IsNaN(actualHeight))
            {
                //We need proper dimentions.
                width = 0;
                height = 0;
                return false;
            }

            var size = new Size(actualWidth, actualHeight);
            this.PixelSizeConverter.Convert(ref size);

            width = this.GetPixelWidth(size.Width);
            height = this.GetPixelHeight(size.Height);
            if (width == 0 || height == 0)
            {
                //We need proper dimentions.
                width = 0;
                height = 0;
                return false;
            }

            return true;
        }

        protected virtual int GetPixelWidth(double width)
        {
            return Convert.ToInt32(width);
        }

        protected virtual int GetPixelHeight(double height)
        {
            return Convert.ToInt32(height);
        }

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
                var info = BitmapHelper.CreateRenderInfo(bitmap, this.Color);
                BitmapHelper.Clear(ref info);
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
            if (this.Debouncer != null)
            {
                this.Debouncer.Dispose();
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

        protected static void UpdateElementsSmooth(float[] values, Int32Rect[] elements, int width, int height, Orientation orientation)
        {
            if (values.Length == 0)
            {
                return;
            }
            if (orientation == Orientation.Horizontal)
            {
                var step = Math.Max(height / values.Length, 1);
                var minChange = 1;
                var maxChange = Convert.ToInt32(width * 0.05f);
                for (var a = 0; a < values.Length; a++)
                {
                    var barWidth = Math.Max(Convert.ToInt32(values[a] * width), 1);
                    elements[a].X = 0;
                    elements[a].Y = a * step;
                    elements[a].Height = step;
                    Animate(ref elements[a].Width, barWidth, 1, width, minChange, maxChange);
                }
            }
            else if (orientation == Orientation.Vertical)
            {
                var step = Math.Max(width / values.Length, 1);
                var minChange = 1;
                var maxChange = Convert.ToInt32(height * 0.05f);
                for (var a = 0; a < values.Length; a++)
                {
                    var barHeight = Math.Max(Convert.ToInt32(values[a] * height), 1);
                    elements[a].X = a * step;
                    elements[a].Width = step;
                    Animate(ref elements[a].Height, barHeight, 1, height, minChange, maxChange);
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

        protected static void Animate(ref int value, int target, int min, int max, int minChange, int maxChange)
        {
            var difference = Math.Abs(value - target);
            if (difference == 0)
            {
                //Nothing to do.
                return;
            }

            var distance = default(float);
            if (difference < target)
            {
                distance = (float)difference / target;
            }
            else
            {
                distance = (float)difference / (max - target);
            }

            var increment = (int)Math.Min(Math.Max((1 - Math.Pow(1 - distance, 4)) * difference, minChange), maxChange);
            if (value < target)
            {
                value += increment;
            }
            else
            {
                value -= increment;
            }
#if DEBUG
            if (value < min || value > max)
            {
                throw new InvalidOperationException();
            }
#endif
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
            return new[]
            {
                color.Shade(contrast),
                color
            };
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
                for (int b = 0, c = previousOffset; c < offset; b++, c++)
                {
                    if (offset > 1)
                    {
                        var ratio = (float)b / (offset - previousOffset - 1);
                        result[c] = Interpolate(previousColor, color, ratio);
                    }
                    else
                    {
                        result[c] = previousColor;
                    }
                }
            }
            return result;
        }

        public static Color Interpolate(Color color1, Color color2, float ratio)
        {
            var ratio1 = 1 - ratio;
            var ratio2 = ratio;
            return Color.FromArgb(
                (byte)Math.Round((color1.A * ratio1) + (color2.A * ratio2)),
                (byte)Math.Round((color1.R * ratio1) + (color2.R * ratio2)),
                (byte)Math.Round((color1.G * ratio1) + (color2.G * ratio2)),
                (byte)Math.Round((color1.B * ratio1) + (color2.B * ratio2))
            );
        }
    }
}
