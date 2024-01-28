using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FoxTunes
{
    public class PeakRenderer : VisualizationBase
    {
        const int MARGIN = 1;

        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
            "Orientation",
            typeof(Orientation),
            typeof(PeakRenderer),
            new FrameworkPropertyMetadata(Orientation.Horizontal, new PropertyChangedCallback(OnOrientationChanged))
        );

        public static Orientation GetOrientation(PeakRenderer source)
        {
            return (Orientation)source.GetValue(OrientationProperty);
        }

        public static void SetOrientation(PeakRenderer source, Orientation value)
        {
            source.SetValue(OrientationProperty, value);
        }

        public static void OnOrientationChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var renderer = sender as PeakRenderer;
            if (renderer == null)
            {
                return;
            }
            renderer.OnOrientationChanged();
        }

        public PeakRendererData RendererData { get; private set; }

        public BooleanConfigurationElement ShowPeaks { get; private set; }

        public BooleanConfigurationElement ShowRms { get; private set; }

        public TextConfigurationElement ColorPalette { get; private set; }

        public IntegerConfigurationElement Duration { get; private set; }

        public Orientation Orientation
        {
            get
            {
                return (Orientation)this.GetValue(OrientationProperty);
            }
            set
            {
                this.SetValue(OrientationProperty, value);
            }
        }

        protected virtual void OnOrientationChanged()
        {
            var data = this.RendererData;
            if (data != null)
            {
                data.Orientation = this.Orientation;
            }
            if (this.OrientationChanged != null)
            {
                this.OrientationChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Orientation");
        }

        public event EventHandler OrientationChanged;

        protected override void OnConfigurationChanged()
        {
            if (this.Configuration != null)
            {
                this.ShowPeaks = this.Configuration.GetElement<BooleanConfigurationElement>(
                   PeakMeterConfiguration.SECTION,
                   PeakMeterConfiguration.PEAKS
                );
                this.ShowRms = this.Configuration.GetElement<BooleanConfigurationElement>(
                    PeakMeterConfiguration.SECTION,
                    PeakMeterConfiguration.RMS
                );
                this.ColorPalette = this.Configuration.GetElement<TextConfigurationElement>(
                    PeakMeterConfiguration.SECTION,
                    PeakMeterConfiguration.COLOR_PALETTE
                );
                this.Duration = this.Configuration.GetElement<IntegerConfigurationElement>(
                    PeakMeterConfiguration.SECTION,
                    PeakMeterConfiguration.DURATION
                );
                this.ShowPeaks.ValueChanged += this.OnValueChanged;
                this.ShowRms.ValueChanged += this.OnValueChanged;
                this.ColorPalette.ValueChanged += this.OnValueChanged;
                this.Duration.ValueChanged += this.OnValueChanged;
                var task = this.CreateBitmap();
            }
            base.OnConfigurationChanged();
        }

        protected virtual void OnValueChanged(object sender, EventArgs e)
        {
            var task = this.CreateData();
        }

        protected override bool CreateData(int width, int height)
        {
            if (this.Configuration == null)
            {
                return false;
            }
            this.RendererData = Create(
                this,
                width,
                height,
                this.ShowPeaks.Value,
                this.ShowRms.Value,
                this.Duration.Value,
                this.GetColorPalettes(this.GetColorPaletteOrDefault(this.ColorPalette.Value), this.ShowPeaks.Value, this.ShowRms.Value, this.Orientation),
                this.Orientation
            );
            return true;
        }

        protected virtual IDictionary<string, IntPtr> GetColorPalettes(string value, bool showPeak, bool showRms, Orientation orientation)
        {
            var flags = default(int);
            var palettes = PeakMeterConfiguration.GetColorPalette(value);
            var background = palettes.GetOrAdd(
                PeakMeterConfiguration.COLOR_PALETTE_BACKGROUND,
                () => DefaultColors.GetBackground()
            );
            //Switch the default colors to the VALUE palette if one was provided.
            var colors = palettes.GetOrAdd(
                PeakMeterConfiguration.COLOR_PALETTE_VALUE,
                () => DefaultColors.GetValue(new[] { this.ForegroundColor })
            );
            if (showPeak)
            {
                palettes.GetOrAdd(
                    PeakMeterConfiguration.COLOR_PALETTE_PEAK,
                    () => DefaultColors.GetPeak(background, showRms, colors)
                );
            }
            if (showRms)
            {
                palettes.GetOrAdd(
                    PeakMeterConfiguration.COLOR_PALETTE_RMS,
                    () => DefaultColors.GetRms(background, showPeak, colors)
                );
            }
            if (showPeak || showRms)
            {
                colors = palettes[EnhancedSpectrumConfiguration.COLOR_PALETTE_VALUE];
                if (!colors.Any(color => color.A != byte.MaxValue))
                {
                    palettes[EnhancedSpectrumConfiguration.COLOR_PALETTE_VALUE] = colors.WithAlpha(-50);
                }
            }
            return palettes.ToDictionary(
                pair => pair.Key,
                pair =>
                {
                    flags = 0;
                    colors = pair.Value;
                    if (colors.Length > 1)
                    {
                        if (orientation == Orientation.Horizontal)
                        {
                            flags |= BitmapHelper.COLOR_FROM_X;
                            colors = colors.Reverse().ToArray();
                        }
                        else if (orientation == Orientation.Vertical)
                        {
                            flags |= BitmapHelper.COLOR_FROM_Y;
                        }
                    }
                    return BitmapHelper.CreatePalette(flags, GetAlphaBlending(pair.Key, colors), colors);
                },
                StringComparer.OrdinalIgnoreCase
            );
        }

        protected override WriteableBitmap CreateBitmap(int width, int height)
        {
            var bitmap = base.CreateBitmap(width, height);
            this.ClearBitmap(bitmap);
            return bitmap;
        }

        protected override void ClearBitmap(WriteableBitmap bitmap)
        {
            if (!bitmap.TryLock(LockTimeout))
            {
                return;
            }
            try
            {
                var info = default(BitmapHelper.RenderInfo);
                var data = this.RendererData;
                if (data != null)
                {
                    info = BitmapHelper.CreateRenderInfo(bitmap, data.Colors[PeakMeterConfiguration.COLOR_PALETTE_BACKGROUND]);
                }
                else
                {
                    var palettes = this.GetColorPalettes(this.GetColorPaletteOrDefault(this.ColorPalette.Value), false, false, this.Orientation);
                    info = BitmapHelper.CreateRenderInfo(bitmap, palettes[PeakMeterConfiguration.COLOR_PALETTE_BACKGROUND]);
                }
                BitmapHelper.DrawRectangle(ref info, 0, 0, data.Width, data.Height);
                bitmap.AddDirtyRect(new global::System.Windows.Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
            }
            finally
            {
                bitmap.Unlock();
            }
        }

        protected virtual Task Render(PeakRendererData data)
        {
            return Windows.Invoke(() =>
            {
                var bitmap = this.Bitmap;
                if (bitmap == null)
                {
                    this.Restart();
                    return;
                }

                if (!bitmap.TryLock(LockTimeout))
                {
                    this.Restart();
                    return;
                }
                var success = default(bool);
                var info = GetRenderInfo(bitmap, data);
                try
                {
                    Render(ref info, data);
                    success = true;
                }
                catch (Exception e)
                {
#if DEBUG
                    Logger.Write(this.GetType(), LogLevel.Warn, "Failed to render peaks: {0}", e.Message);
#else
                    Logger.Write(this.GetType(), LogLevel.Warn, "Failed to render peaks, disabling: {0}", e.Message);
                    success = false;
#endif
                }
                bitmap.AddDirtyRect(new global::System.Windows.Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
                bitmap.Unlock();
                if (!success)
                {
                    return;
                }
#if DEBUG
                if (this.ViewModel != null)
                {
                    Interlocked.Increment(ref this.ViewModel.Frames);
                }
#endif
                this.Restart();
            }, DISPATCHER_PRIORITY);
        }

        protected override void OnElapsed(object sender, ElapsedEventArgs e)
        {
            var data = this.RendererData;
            if (data == null)
            {
                this.Restart();
                return;
            }
            try
            {
                if (!this.VisualizationDataSource.Update(data))
                {
                    this.Restart();
                    return;
                }
                UpdateValues(data);
                if (!data.LastUpdated.Equals(default(DateTime)))
                {
                    UpdateElementsSmooth(data);
                }
                else
                {
                    UpdateElementsFast(data);
                }
                data.LastUpdated = DateTime.UtcNow;

                var task = this.Render(data);
            }
            catch (Exception exception)
            {
#if DEBUG
                Logger.Write(this.GetType(), LogLevel.Warn, "Failed to update peak data: {0}", exception.Message);
                this.Restart();
#else
                Logger.Write(this.GetType(), LogLevel.Warn, "Failed to update peak data, disabling: {0}", exception.Message);
#endif
            }
        }

        protected override void OnDisposing()
        {
            if (this.ShowPeaks != null)
            {
                this.ShowPeaks.ValueChanged -= this.OnValueChanged;
            }
            if (this.ShowRms != null)
            {
                this.ShowRms.ValueChanged -= this.OnValueChanged;
            }
            if (this.ColorPalette != null)
            {
                this.ColorPalette.ValueChanged -= this.OnValueChanged;
            }
            if (this.Duration != null)
            {
                this.Duration.ValueChanged -= this.OnValueChanged;
            }
            base.OnDisposing();
        }

        private static PeakRenderInfo GetRenderInfo(WriteableBitmap bitmap, PeakRendererData data)
        {
            var info = new PeakRenderInfo()
            {
                Background = BitmapHelper.CreateRenderInfo(bitmap, data.Colors[PeakMeterConfiguration.COLOR_PALETTE_BACKGROUND])
            };
            if (data.PeakElements != null)
            {
                info.Peak = BitmapHelper.CreateRenderInfo(bitmap, data.Colors[PeakMeterConfiguration.COLOR_PALETTE_PEAK]);
            }
            if (data.RmsElements != null)
            {
                info.Rms = BitmapHelper.CreateRenderInfo(bitmap, data.Colors[PeakMeterConfiguration.COLOR_PALETTE_RMS]);
            }
            if (data.ValueElements != null)
            {
                info.Value = BitmapHelper.CreateRenderInfo(bitmap, data.Colors[PeakMeterConfiguration.COLOR_PALETTE_VALUE]);
            }
            return info;
        }

        private static void Render(ref PeakRenderInfo info, PeakRendererData data)
        {
            if (info.Background.Width != data.Width || info.Background.Height != data.Height)
            {
                //Bitmap does not match data.
                return;
            }

            BitmapHelper.DrawRectangle(ref info.Background, 0, 0, data.Width, data.Height);

            if (data.SampleCount == 0)
            {
                //No data.
                return;
            }

            if (data.PeakElements != null)
            {
                BitmapHelper.DrawRectangles(ref info.Peak, data.PeakElements, data.PeakElements.Length);
            }
            if (data.RmsElements != null)
            {
                BitmapHelper.DrawRectangles(ref info.Rms, data.RmsElements, data.RmsElements.Length);
            }
            if (data.ValueElements != null)
            {
                BitmapHelper.DrawRectangles(ref info.Value, data.ValueElements, data.ValueElements.Length);
            }
        }

        private static void UpdateValues(PeakRendererData data)
        {
            Array.Clear(data.Values, 0, data.Values.Length);
            if (data.PeakValues != null)
            {
                Array.Clear(data.PeakValues, 0, data.PeakValues.Length);
            }
            if (data.RmsValues != null)
            {
                Array.Clear(data.RmsValues, 0, data.RmsValues.Length);
            }

            for (var channel = 0; channel < data.Channels; channel++)
            {
                for (var position = 0; position < data.SampleCount; position++)
                {
                    var value = Math.Min(Math.Max(Math.Abs(data.Data[channel, position]), 0.0f), 1.0f);
                    data.Values[channel] = Math.Max(data.Values[channel], value);
                    if (data.PeakValues != null)
                    {
                        value = Math.Min(Math.Max(Math.Abs(data.History.Peak[channel, position]), 0.0f), 1.0f);
                        data.PeakValues[channel] = Math.Max(data.PeakValues[channel], value);
                    }
                    if (data.RmsValues != null)
                    {
                        value = Math.Min(Math.Max(Math.Abs(data.History.Rms[channel, position]), 0.0f), 1.0f);
                        data.RmsValues[channel] = Math.Max(data.RmsValues[channel], value);
                    }
                }
            }
        }

        private static void UpdateElementsFast(PeakRendererData data)
        {
            UpdateElementsFast(data.Values, data.ValueElements, data.Width, data.Height, MARGIN, data.Orientation);
            if (data.PeakValues != null && data.PeakElements != null)
            {
                UpdateElementsFast(data.PeakValues, data.PeakElements, data.Width, data.Height, MARGIN, data.Orientation);
            }
            if (data.RmsValues != null && data.RmsElements != null)
            {
                UpdateElementsFast(data.RmsValues, data.RmsElements, data.Width, data.Height, MARGIN, data.Orientation);
            }
        }

        private static void UpdateElementsSmooth(PeakRendererData data)
        {
            UpdateElementsSmooth(data.Values, data.ValueElements, data.Width, data.Height, MARGIN, data.Orientation);
            if (data.PeakValues != null && data.PeakElements != null)
            {
                UpdateElementsSmooth(data.PeakValues, data.PeakElements, data.Width, data.Height, MARGIN, data.Orientation);
            }
            if (data.RmsValues != null && data.RmsElements != null)
            {
                UpdateElementsSmooth(data.RmsValues, data.RmsElements, data.Width, data.Height, MARGIN, data.Orientation);
            }
        }

        public static PeakRendererData Create(PeakRenderer renderer, int width, int height, bool showPeak, bool showRms, int history, IDictionary<string, IntPtr> colors, Orientation orientation)
        {
            var data = new PeakRendererData(history, showPeak, showRms)
            {
                Renderer = renderer,
                Width = width,
                Height = height,
                Colors = colors,
                Orientation = orientation,
                //TODO: This should be PeakRenderer.UpdateInterval I think.
                Interval = TimeSpan.FromMilliseconds(100),
                Flags = VisualizationDataFlags.Individual
            };
            return data;
        }
        public class PeakRendererData : PCMVisualizationData
        {
            public PeakRendererData(int history, bool showPeak, bool showRms)
            {
                this.History = new VisualizationDataHistory()
                {
                    Capacity = history,
                    Flags = VisualizationDataHistoryFlags.None
                };
                if (showPeak)
                {
                    this.History.Flags |= VisualizationDataHistoryFlags.Peak;
                }
                if (showRms)
                {
                    this.History.Flags |= VisualizationDataHistoryFlags.Rms;
                }
            }

            public PeakRenderer Renderer;

            public int Width;

            public int Height;

            public IDictionary<string, IntPtr> Colors;

            public Orientation Orientation;

            public float[] Values;

            public float[] PeakValues;

            public float[] RmsValues;

            public Int32Rect[] ValueElements;

            public Int32Rect[] PeakElements;

            public Int32Rect[] RmsElements;

            public DateTime LastUpdated;

            public override void OnAllocated()
            {
                this.Values = new float[this.Channels];
                this.ValueElements = new Int32Rect[this.Channels];

                if (this.Renderer.ShowRms.Value)
                {
                    this.RmsValues = new float[this.Channels];
                    this.RmsElements = new Int32Rect[this.Channels];
                }
                if (this.Renderer.ShowPeaks.Value)
                {
                    this.PeakValues = new float[this.Channels];
                    this.PeakElements = new Int32Rect[this.Channels];
                }
                base.OnAllocated();
            }

            ~PeakRendererData()
            {
                try
                {
                    if (this.Colors != null)
                    {
                        foreach (var pair in this.Colors)
                        {
                            var value = pair.Value;
                            BitmapHelper.DestroyPalette(ref value);
                        }
                        this.Colors.Clear();
                    }
                }
                catch
                {
                    //Nothing can be done, never throw on GC thread.
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PeakRenderInfo
        {
            public BitmapHelper.RenderInfo Peak;

            public BitmapHelper.RenderInfo Rms;

            public BitmapHelper.RenderInfo Value;

            public BitmapHelper.RenderInfo Background;
        }

        public static class DefaultColors
        {
            public static Color[] GetBackground()
            {
                return new[]
                {
                    global::System.Windows.Media.Colors.Black
                };
            }

            public static Color[] GetPeak(Color[] background, bool showRms, Color[] colors)
            {
                var transparency = background.Length > 1 || background.FirstOrDefault().A != 255;
                if (transparency)
                {
                    if (showRms)
                    {
                        return colors.WithAlpha(-200);
                    }
                    else
                    {
                        return colors.WithAlpha(-100);
                    }
                }
                else
                {
                    var color = background.FirstOrDefault();
                    if (showRms)
                    {
                        return colors.Interpolate(color, 0.8f);
                    }
                    else
                    {
                        return colors.Interpolate(color, 0.4f);
                    }
                }
            }

            public static Color[] GetRms(Color[] background, bool showPeak, Color[] colors)
            {
                var transparency = background.Length > 1 || background.FirstOrDefault().A != 255;
                if (transparency)
                {
                    if (showPeak)
                    {
                        return colors.WithAlpha(-100);
                    }
                    else
                    {
                        return colors.WithAlpha(-50);
                    }
                }
                else
                {
                    var color = background.FirstOrDefault();
                    if (showPeak)
                    {
                        return colors.Interpolate(color, 0.4f);
                    }
                    else
                    {
                        return colors.Interpolate(color, 0.2f);
                    }
                }
            }

            public static Color[] GetValue(Color[] colors)
            {
                return colors;
            }
        }
    }
}
