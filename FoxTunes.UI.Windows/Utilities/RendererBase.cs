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
    public abstract class RendererBase : FrameworkElement, IBaseComponent, IConfigurationTarget, INotifyPropertyChanged, IDisposable
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
            new FrameworkPropertyMetadata(Brushes.Transparent, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender)
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

        protected RendererBase(bool initialize = true)
        {
            if (initialize && Core.Instance != null)
            {
                this.InitializeComponent(Core.Instance);
            }
        }

        private IConfiguration _Configuration { get; set; }

        public IConfiguration Configuration
        {
            get
            {
                return this._Configuration;
            }
            set
            {
                this._Configuration = value;
                this.OnConfigurationChanged();
            }
        }

        protected virtual void OnConfigurationChanged()
        {
            if (this.ConfigurationChanged != null)
            {
                this.ConfigurationChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Configuration");
        }

        public event EventHandler ConfigurationChanged;

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
                return global::System.Windows.Media.Colors.Transparent;
            }
        }

        public Color[] Colors { get; private set; }

        protected virtual void OnColorsChanged()
        {
            var task = this.CreateData();
        }

        protected virtual bool LoadColorPalette
        {
            get
            {
                return true;
            }
        }

        public PixelSizeConverter PixelSizeConverter { get; private set; }

        public ThemeLoader ThemeLoader { get; private set; }

        public IOutputDataSource OutputDataSource { get; private set; }

        public IVisualizationDataSource VisualizationDataSource { get; private set; }

        public virtual void InitializeComponent(ICore core)
        {
            this.PixelSizeConverter = ComponentRegistry.Instance.GetComponent<PixelSizeConverter>();
            if (this.LoadColorPalette)
            {
                this.ThemeLoader = ComponentRegistry.Instance.GetComponent<ThemeLoader>();
                this.ThemeLoader.ConnectTheme(theme =>
                {
                    var raise = this.Colors != null;
                    this.Colors = theme.ColorPalettes.First().Value.ToColorStops().ToGradient();
                    if (raise)
                    {
                        this.OnColorsChanged();
                    }
                });
            }
            this.OutputDataSource = core.Components.OutputDataSource;
            this.VisualizationDataSource = core.Components.VisualizationDataSource;
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            var task = this.CreateBitmap(false);
            base.OnRenderSizeChanged(sizeInfo);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawRectangle(this.Background, null, new Rect(0, 0, this.ActualWidth, this.ActualHeight));
        }

        protected virtual Task CreateBitmap(bool force)
        {
            return Windows.Invoke(() =>
            {
                var width = default(int);
                var height = default(int);
                if (!this.GetPixelSize(out width, out height))
                {
                    return;
                }
                if (this.Bitmap != null && !force)
                {
                    if (this.Bitmap.PixelWidth == width && this.Bitmap.PixelHeight == height)
                    {
                        //Nothing to do.
                        return;
                    }
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

        protected virtual void ClearBitmap(WriteableBitmap bitmap)
        {
            if (!bitmap.TryLock(LockTimeout))
            {
                return;
            }
            try
            {
                var info = BitmapHelper.CreateRenderInfo(bitmap, IntPtr.Zero);
                BitmapHelper.Clear(ref info);
                bitmap.AddDirtyRect(new global::System.Windows.Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
            }
            finally
            {
                bitmap.Unlock();
            }
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
                this.ClearBitmap(bitmap);
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

        protected static float ToDecibel(float value)
        {
            return Math.Min(Math.Max((float)(20 * Math.Log10(value)), DB_MIN), DB_MAX);
        }

        protected static float ToDecibelFixed(float value)
        {
            var dB = ToDecibel(value);
            return 1.0f - Math.Abs(dB) / Math.Abs(DB_MIN);
        }

        protected static float FromDecibel(int value)
        {
            return Math.Min(Math.Max((float)value / Math.Abs(DB_MIN), 0.0f), 1.0f);
        }

        public static int IndexToFrequency(int index, int fftSize, int rate)
        {
            var frequency = (int)Math.Floor((double)index * (double)rate / (double)fftSize);
            if (frequency > rate / 2)
            {
                frequency = rate / 2;
            }
            return frequency;
        }

        public static int FrequencyToIndex(int frequency, int fftSize, int rate)
        {
            var index = (int)Math.Floor((double)fftSize * (double)frequency / (double)rate);
            if (index > fftSize / 2 - 1)
            {
                index = fftSize / 2 - 1;
            }
            return index;
        }

        protected static void UpdateElementsFast(float[] values, Int32Rect[] elements, int width, int height, int margin, Orientation orientation)
        {
            if (values.Length == 0)
            {
                return;
            }
            if (orientation == Orientation.Horizontal)
            {
                var step = (height / values.Length) + margin;
                for (var a = 0; a < values.Length; a++)
                {
                    var elementWidth = Math.Max(Convert.ToInt32(values[a] * width), 1);
                    var elementHeight = step - margin;
                    elements[a].X = 0;
                    elements[a].Y = a * step;
                    elements[a].Height = elementHeight;
                    if (elementWidth > 0)
                    {
                        elements[a].Width = elementWidth;
                    }
                    else
                    {
                        elements[a].Width = 1;
                    }
                }
            }
            else if (orientation == Orientation.Vertical)
            {
                var step = (width / values.Length) + margin;
                for (var a = 0; a < values.Length; a++)
                {
                    var elementWidth = step - margin;
                    var elementHeight = Math.Max(Convert.ToInt32(values[a] * height), 1);
                    elements[a].X = a * step;
                    elements[a].Width = elementWidth;
                    if (elementHeight > 0)
                    {
                        elements[a].Height = elementHeight;
                    }
                    else
                    {
                        elements[a].Height = 1;
                    }
                    elements[a].Y = height - elements[a].Height;
                }
            }
        }

        protected static void UpdateElementsSmooth(float[] values, Int32Rect[] elements, int width, int height, int margin, Orientation orientation)
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
                    var elementWidth = Math.Max(Convert.ToInt32(values[a] * width), 1);
                    var elementHeight = step - margin;
                    elements[a].X = 0;
                    elements[a].Y = a * step;
                    elements[a].Height = elementHeight;
                    Animate(ref elements[a].Width, elementWidth, 1, width, minChange, maxChange);
                }
            }
            else if (orientation == Orientation.Vertical)
            {
                var step = Math.Max(width / values.Length, 1);
                var minChange = 1;
                var maxChange = Convert.ToInt32(height * 0.05f);
                for (var a = 0; a < values.Length; a++)
                {
                    var elementWidth = step - margin;
                    var elementHeight = Math.Max(Convert.ToInt32(values[a] * height), 1);
                    elements[a].X = a * step;
                    elements[a].Width = elementWidth;
                    Animate(ref elements[a].Height, elementHeight, 1, height, minChange, maxChange);
                    elements[a].Y = height - elements[a].Height;
                }
            }
        }

        protected static void UpdateElementsSmooth(int[] values, Int32Rect[] elements, int[] holds, int width, int height, int margin, int interval, int duration, Orientation orientation)
        {
            if (values.Length == 0)
            {
                return;
            }
            if (orientation == Orientation.Horizontal)
            {
                var fast = width / 4;
                var step = Math.Max(height / values.Length, 1);
                for (int a = 0; a < values.Length; a++)
                {
                    var elementWidth = 1;
                    var elementHeight = step - margin;
                    var target = values[a];
                    elements[a].Y = a * step;
                    elements[a].Width = elementWidth;
                    elements[a].Height = elementHeight;
                    if (target > elements[a].X)
                    {
                        elements[a].X = target;
                        holds[a] = interval + ROLLOFF_INTERVAL;
                    }
                    else if (target < elements[a].X)
                    {
                        if (holds[a] > 0)
                        {
                            if (holds[a] < interval)
                            {
                                var distance = 1 - ((float)holds[a] / interval);
                                var increment = fast * (distance * distance * distance);
                                if (elements[a].X > increment)
                                {
                                    elements[a].X -= (int)Math.Round(increment);
                                }
                                else if (elements[a].X > 0)
                                {
                                    elements[a].X = 0;
                                }
                            }
                            holds[a] -= duration;
                        }
                        else if (elements[a].X > fast)
                        {
                            elements[a].X -= fast;
                        }
                        else if (elements[a].X > 0)
                        {
                            elements[a].X = 0;
                        }
                    }
                }
            }
            else if (orientation == Orientation.Vertical)
            {
                var fast = height / 4;
                var step = Math.Max(width / values.Length, 1);
                for (int a = 0; a < values.Length; a++)
                {
                    var elementWidth = step - margin;
                    var elementHeight = 1;
                    var target = values[a];
                    elements[a].X = a * step;
                    elements[a].Width = elementWidth;
                    elements[a].Height = elementHeight;
                    if (target < elements[a].Y)
                    {
                        elements[a].Y = target;
                        holds[a] = interval + ROLLOFF_INTERVAL;
                    }
                    else if (target > elements[a].Y)
                    {
                        if (holds[a] > 0)
                        {
                            if (holds[a] < interval)
                            {
                                var distance = 1 - ((float)holds[a] / interval);
                                var increment = fast * (distance * distance * distance);
                                if (elements[a].Y < height - increment)
                                {
                                    elements[a].Y += (int)Math.Round(increment);
                                }
                                else if (elements[a].Y < height - 1)
                                {
                                    elements[a].Y = height - 1;
                                }
                            }
                            holds[a] -= duration;
                        }
                        else if (elements[a].Y < height - fast)
                        {
                            elements[a].Y += fast;
                        }
                        else if (elements[a].Y < height - 1)
                        {
                            elements[a].Y = height - 1;
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

        protected static void NoiseReduction(float[] values, int count, int smoothing)
        {
            for (var a = 0; a < count; a++)
            {
                var start = Math.Max(a - smoothing, 0);
                var end = Math.Min(a + smoothing, count);
                var value = default(float);
                for (var b = start; b < end; b++)
                {
                    value += values[b];
                }
                value /= end - start;
                values[a] = value;
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
    }

    public static partial class Extensions
    {
        public static bool IsLighter(this Color color)
        {
            return color.R > byte.MaxValue / 2 && color.G > byte.MaxValue / 2 && color.B > byte.MaxValue / 2;
        }

        public static Color[] Shade(this Color[] colors, Color color)
        {
            return colors.Select(_color => _color.Shade(color)).ToArray();
        }

        public static Color Shade(this Color color1, Color color2)
        {
            if (color1.IsLighter())
            {
                //Create darner shade.
                return Color.FromArgb(
                    color1.A,
                    Convert.ToByte(Math.Min(color1.R - color2.R, byte.MaxValue)),
                    Convert.ToByte(Math.Min(color1.G - color2.G, byte.MaxValue)),
                    Convert.ToByte(Math.Min(color1.B - color2.B, byte.MaxValue))
                );
            }
            else
            {
                //Create lighter shade.
                return Color.FromArgb(
                    color1.A,
                    Convert.ToByte(Math.Min(color1.R + color2.R, byte.MaxValue)),
                    Convert.ToByte(Math.Min(color1.G + color2.G, byte.MaxValue)),
                    Convert.ToByte(Math.Min(color1.B + color2.B, byte.MaxValue))
                );
            }
        }

        public static Color[] ToPair(this Color color, byte shade)
        {
            var contrast = Color.FromRgb(shade, shade, shade);
            return new[]
            {
                color.Shade(contrast),
                color
            };
        }

        public static Color[] WithAlpha(this Color[] colors, int alpha)
        {
            return colors.Select(color => color.WithAlpha(alpha)).ToArray();
        }

        public static Color WithAlpha(this Color color, int alpha)
        {
            color.A = Convert.ToByte(Math.Max(Math.Min(color.A + alpha, byte.MaxValue), byte.MinValue));
            return color;
        }

        public static IDictionary<string, ColorStop[]> ToNamedColorStops(this string value)
        {
            const string PREFIX = "//";
            var groups = new Dictionary<string, IList<string>>(StringComparer.OrdinalIgnoreCase);
            var lines = value
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .ToArray();
            var group = string.Empty;
            foreach (var line in lines)
            {
                if (line.StartsWith(PREFIX))
                {
                    group = line.Substring(PREFIX.Length).Trim();
                    continue;
                }
                groups.GetOrAdd(group, () => new List<string>()).Add(line);
            }
            return groups.ToDictionary(
                pair => pair.Key,
                pair => pair.Value.ToColorStops(),
                StringComparer.OrdinalIgnoreCase
            );
        }

        public static ColorStop[] ToColorStops(this string value)
        {
            var lines = value
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .ToArray();
            return lines.ToColorStops();
        }

        public static ColorStop[] ToColorStops(this IEnumerable<string> lines)
        {
            var colors = new List<ColorStop>();
            foreach (var line in lines)
            {
                var parts = line
                    .Split(new[] { ':' }, 2)
                    .Select(part => part.Trim())
                    .ToArray();
                var index = default(int);
                var color = default(Color);
                if (parts.Length == 2)
                {
                    if (!int.TryParse(parts[0], out index))
                    {
                        index = -1;
                    }
                    color = parts[1].ToColor();
                }
                else
                {
                    index = -1;
                    color = parts[0].ToColor();
                }
                colors.Add(new ColorStop(index, color));
            }
            var max = Math.Max(colors.Max(color => color.Index), colors.Count);
            for (var index = 0; index < colors.Count; index++)
            {
                var color = colors[index];
                if (color.Index >= 0)
                {
                    continue;
                }
                if (max == colors.Count)
                {
                    color.Index = index;
                }
                else
                {
                    //TODO: Sorry, couldn't work this out.
                    color.Index = Convert.ToInt32(((float)index / colors.Count) * max);
                }
            }
            return colors.ToArray();
        }

        public static Color[] ToGradient(this ColorStop[] colors)
        {
            switch (colors.Length)
            {
                case 0:
                    return new Color[] { };
                case 1:
                    return new Color[] { colors[0].Color };
            }
            var min = Math.Max(colors.Min(color => color.Index), 0);
            var max = Math.Max(colors.Max(color => color.Index), colors.Length);
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
                    previousOffset = colors[a - 1].Index;
                    previousColor = colors[a - 1].Color;
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
                    nextOffset = colors[a + 1].Index;
                    nextColor = colors[a + 1].Color;
                }
                var offset = colors[a].Index;
                var color = colors[a].Color;
                for (int b = 0, c = previousOffset; c < offset; b++, c++)
                {
                    var d = offset - previousOffset - 1;
                    if (d > 0)
                    {
                        var ratio = (float)b / d;
                        result[c] = Interpolate(previousColor, color, ratio);
                    }
                    else
                    {
                        result[c] = previousColor;
                    }
                }
            }
            //TODO: Hack hack hack.
            result[result.Length - 1] = colors[colors.Length - 1].Color;
            return result;
        }

        public static Color[] MirrorGradient(this Color[] colors, bool invert)
        {
            return new[]
            {
                invert ? colors.Reverse() : colors,
                invert ? colors : colors.Reverse()
            }.SelectMany(a => a).ToArray();
        }

        public static Color[] DuplicateGradient(this Color[] colors, int count)
        {
            return Enumerable.Range(0, count).SelectMany(position => colors).ToArray();
        }

        public static Color ToColor(this string value)
        {
            try
            {
                var color = default(string);
                var alpha = default(byte);
                if (!value.TryParse(out color, out alpha))
                {
                    //Failed to parse the color.
                    return Colors.Red;
                }
                var result = (Color)ColorConverter.ConvertFromString(color);
                if (alpha != byte.MaxValue)
                {
                    result.A = alpha;
                }
                return result;
            }
            catch
            {
                //Failed to parse the color.
                return Colors.Red;
            }
        }

        public static bool TryParse(this string value, out string color, out byte alpha)
        {
            const char DELIMITER = ',';
            var parts = value
                .Split(new[] { DELIMITER }, 2)
                .Select(element => element.Trim())
                .ToArray();
            if (parts.Length == 0)
            {
                color = default(string);
                alpha = default(byte);
                return false;
            }
            else
            {
                color = parts[0];
                if (parts.Length == 2)
                {
                    const string PERCENT = "%";
                    if (parts[1].EndsWith(PERCENT))
                    {
                        if (byte.TryParse(parts[1].Substring(0, parts[1].Length - 1), out alpha) && alpha <= 100)
                        {
                            alpha = Convert.ToByte(((float)alpha / 100) * byte.MaxValue);
                        }
                        else
                        {
                            alpha = byte.MaxValue;
                        }
                    }
                    else
                    {
                        if (!byte.TryParse(parts[1], out alpha))
                        {
                            alpha = byte.MaxValue;
                        }
                    }
                }
                else
                {
                    alpha = byte.MaxValue;
                }
            }
            return true;
        }

        public static Color Interpolate(Color color1, Color color2, float ratio)
        {
            var ratio1 = 1 - ratio;
            var ratio2 = ratio;
            return Color.FromArgb(
                Convert.ToByte((color1.A * ratio1) + (color2.A * ratio2)),
                Convert.ToByte((color1.R * ratio1) + (color2.R * ratio2)),
                Convert.ToByte((color1.G * ratio1) + (color2.G * ratio2)),
                Convert.ToByte((color1.B * ratio1) + (color2.B * ratio2))
            );
        }
    }

    public class ColorStop
    {
        public ColorStop(int index, Color color)
        {
            this.Index = index;
            this.Color = color;
        }

        public int Index;

        public Color Color;
    }
}
