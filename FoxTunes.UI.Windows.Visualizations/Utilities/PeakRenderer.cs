using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
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

        public IntegerConfigurationElement HoldInterval { get; private set; }

        public BooleanConfigurationElement ShowRms { get; private set; }

        public TextConfigurationElement ColorPalette { get; private set; }

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
                this.HoldInterval = this.Configuration.GetElement<IntegerConfigurationElement>(
                   PeakMeterConfiguration.SECTION,
                   PeakMeterConfiguration.HOLD
                );
                this.ShowRms = this.Configuration.GetElement<BooleanConfigurationElement>(
                    PeakMeterConfiguration.SECTION,
                    PeakMeterConfiguration.RMS
                );
                this.ColorPalette = this.Configuration.GetElement<TextConfigurationElement>(
                    PeakMeterConfiguration.SECTION,
                    PeakMeterConfiguration.COLOR_PALETTE
                );
                this.ShowPeaks.ValueChanged += this.OnValueChanged;
                this.ShowRms.ValueChanged += this.OnValueChanged;
                this.ColorPalette.ValueChanged += this.OnValueChanged;
                var task = this.CreateBitmap(true);
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
            var colors = PeakMeterConfiguration.GetColorPalette(this.ColorPalette.Value, this.Colors);
            this.RendererData = Create(
                this,
                width,
                height,
                this.GetColorPalettes(this.ColorPalette.Value, this.ShowPeaks.Value, this.ShowRms.Value, this.Colors, this.Orientation),
                this.Orientation
            );
            return true;
        }

        protected virtual IDictionary<string, IntPtr> GetColorPalettes(string value, bool showPeak, bool showRms, Color[] colors, Orientation orientation)
        {
            var palettes = PeakMeterConfiguration.GetColorPalette(value, colors);
            //Switch the default colors to the VALUE palette if one was provided.
            colors = palettes.GetOrAdd(
                PeakMeterConfiguration.COLOR_PALETTE_VALUE,
                () => GetDefaultColors(PeakMeterConfiguration.COLOR_PALETTE_VALUE, showRms, colors)
            );
            palettes.GetOrAdd(
                PeakMeterConfiguration.COLOR_PALETTE_BACKGROUND,
                () => GetDefaultColors(PeakMeterConfiguration.COLOR_PALETTE_BACKGROUND, showRms, colors)
            );
            if (showPeak)
            {
                palettes.GetOrAdd(
                    PeakMeterConfiguration.COLOR_PALETTE_PEAK,
                    () => GetDefaultColors(PeakMeterConfiguration.COLOR_PALETTE_PEAK, showRms, colors)
                );
            }
            if (showRms)
            {
                palettes.GetOrAdd(
                    PeakMeterConfiguration.COLOR_PALETTE_RMS,
                    () => GetDefaultColors(PeakMeterConfiguration.COLOR_PALETTE_RMS, showRms, colors)
                );
            }
            return palettes.ToDictionary(
                pair => pair.Key,
                pair =>
                {
                    var flags = 0;
                    if (pair.Value.Length > 1)
                    {
                        if (string.Equals(pair.Key, PeakMeterConfiguration.COLOR_PALETTE_BACKGROUND, StringComparison.OrdinalIgnoreCase))
                        {
                            flags |= BitmapHelper.COLOR_FROM_Y;
                        }
                        else
                        {
                            if (orientation == Orientation.Horizontal)
                            {
                                flags |= BitmapHelper.COLOR_FROM_X;
                            }
                            else if (orientation == Orientation.Vertical)
                            {
                                flags |= BitmapHelper.COLOR_FROM_Y;
                            }
                        }
                    }
                    if (showPeak || showRms)
                    {
                        if (!string.Equals(pair.Key, PeakMeterConfiguration.COLOR_PALETTE_BACKGROUND, StringComparison.OrdinalIgnoreCase))
                        {
                            flags |= BitmapHelper.ALPHA_BLENDING;
                        }
                    }
                    return BitmapHelper.GetOrCreatePalette(flags, pair.Value);
                },
                StringComparer.OrdinalIgnoreCase
            );
        }

        private static Color[] GetDefaultColors(string name, bool showRms, Color[] colors)
        {
            switch (name)
            {
                case PeakMeterConfiguration.COLOR_PALETTE_PEAK:
                    return new[]
                    {
                        colors.FirstOrDefault()
                    };
                case PeakMeterConfiguration.COLOR_PALETTE_RMS:
                    return colors.WithAlpha(-50);
                case PeakMeterConfiguration.COLOR_PALETTE_VALUE:
                    if (showRms)
                    {
                        return colors.WithAlpha(-25);
                    }
                    else
                    {
                        return colors;
                    }
                case PeakMeterConfiguration.COLOR_PALETTE_BACKGROUND:
                    return new[]
                    {
                        global::System.Windows.Media.Colors.Black
                    };
            }
            throw new NotImplementedException();
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
                    var palettes = this.GetColorPalettes(this.ColorPalette.Value, false, false, this.Colors, this.Orientation);
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

        protected virtual async Task Render(PeakRendererData data)
        {
            var bitmap = default(WriteableBitmap);
            var success = default(bool);
            var info = default(PeakRenderInfo);

            await Windows.Invoke(() =>
            {
                bitmap = this.Bitmap;
                if (bitmap == null)
                {
                    return;
                }

                success = bitmap.TryLock(LockTimeout);
                if (!success)
                {
                    return;
                }
                info = GetRenderInfo(bitmap, data);
            }, DISPATCHER_PRIORITY).ConfigureAwait(false);

            if (!success)
            {
                //Failed to establish lock.
                this.Restart();
                return;
            }

            try
            {
                Render(ref info, data);
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

            await Windows.Invoke(() =>
            {
                bitmap.AddDirtyRect(new global::System.Windows.Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
                bitmap.Unlock();
            }, DISPATCHER_PRIORITY).ConfigureAwait(false);

            if (!success)
            {
                return;
            }
            this.Restart();
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
                if (data.PeakElements != null)
                {
                    UpdatePeaks(data, this.UpdateInterval, this.HoldInterval.Value);
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
            UpdateValues(data.Data, data.Values, data.Rms, data.Channels, data.SampleCount);
        }

        private static void UpdateValues(float[,] samples, float[] values, float[] rms, int channels, int count)
        {
            var doRms = rms != null;
            for (var channel = 0; channel < channels; channel++)
            {
                values[channel] = 0;
                if (doRms)
                {
                    rms[channel] = 0;
                }
            }

            if (count == 0)
            {
                return;
            }

            for (var position = 0; position < count; position += channels)
            {
                for (var channel = 0; channel < channels; channel++)
                {
                    var value = Math.Abs(samples[channel, position]);
                    values[channel] = Math.Max(value, values[channel]);
                    if (doRms)
                    {
                        rms[channel] += value * value;
                    }
                }
            }

            for (var channel = 0; channel < channels; channel++)
            {
                values[channel] = ToDecibelFixed(values[channel]);
            }

            if (doRms)
            {
                for (var channel = 0; channel < channels; channel++)
                {
                    rms[channel] = ToDecibelFixed(Convert.ToSingle(Math.Sqrt(rms[channel] / (count / channels))));
                }
            }
        }

        private static void UpdateElementsFast(PeakRendererData data)
        {
            UpdateElementsFast(data.Values, data.ValueElements, data.Width, data.Height, MARGIN, data.Orientation);
            if (data.Rms != null && data.RmsElements != null)
            {
                UpdateElementsFast(data.Rms, data.RmsElements, data.Width, data.Height, MARGIN, data.Orientation);
            }
        }

        private static void UpdateElementsSmooth(PeakRendererData data)
        {
            UpdateElementsSmooth(data.Values, data.ValueElements, data.Width, data.Height, MARGIN, data.Orientation);
            if (data.Rms != null && data.RmsElements != null)
            {
                UpdateElementsSmooth(data.Rms, data.RmsElements, data.Width, data.Height, MARGIN, data.Orientation);
            }
        }

        private static void UpdatePeaks(PeakRendererData data, int updateInterval, int holdInterval)
        {
            if (data.RmsElements != null)
            {
                for (var a = 0; a < data.Peaks.Length; a++)
                {
                    if (data.Orientation == Orientation.Horizontal)
                    {
                        data.Peaks[a] = Math.Max(data.ValueElements[a].Width, data.RmsElements[a].Width);
                    }
                    else if (data.Orientation == Orientation.Vertical)
                    {
                        data.Peaks[a] = Math.Min(data.ValueElements[a].Y, data.RmsElements[a].Y);
                    }
                }
            }
            else
            {
                for (var a = 0; a < data.Peaks.Length; a++)
                {
                    if (data.Orientation == Orientation.Horizontal)
                    {
                        data.Peaks[a] = data.ValueElements[a].Width;
                    }
                    else if (data.Orientation == Orientation.Vertical)
                    {
                        data.Peaks[a] = data.ValueElements[a].Y;
                    }
                }
            }
            var duration = Convert.ToInt32(
                Math.Min(
                    (DateTime.UtcNow - data.LastUpdated).TotalMilliseconds,
                    updateInterval * 100
                )
            );
            UpdateElementsSmooth(data.Peaks, data.PeakElements, data.Holds, data.Width, data.Height, MARGIN, holdInterval, duration, data.Orientation);
        }

        public static PeakRendererData Create(PeakRenderer renderer, int width, int height, IDictionary<string, IntPtr> colors, Orientation orientation)
        {
            var data = new PeakRendererData()
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
            public PeakRenderer Renderer;

            public int Width;

            public int Height;

            public IDictionary<string, IntPtr> Colors;

            public Orientation Orientation;

            public float[] Values;

            public float[] Rms;

            public Int32Rect[] ValueElements;

            public Int32Rect[] RmsElements;

            public Int32Rect[] PeakElements;

            public int[] Peaks;

            public int[] Holds;

            public DateTime LastUpdated;

            public override void OnAllocated()
            {
                this.Values = new float[this.Channels];
                this.ValueElements = new Int32Rect[this.Channels];

                if (this.Renderer.ShowRms.Value)
                {
                    this.Rms = new float[this.Channels];
                    this.RmsElements = new Int32Rect[this.Channels];
                }
                if (this.Renderer.ShowPeaks.Value)
                {
                    this.Peaks = new int[this.Channels];
                    this.Holds = new int[this.Channels];
                    this.PeakElements = CreatePeaks(this.Channels);
                }
                base.OnAllocated();
            }

            private static Int32Rect[] CreatePeaks(int count)
            {
                var peaks = new Int32Rect[count];
                for (var a = 0; a < count; a++)
                {
                    peaks[a].Y = int.MaxValue;
                }
                return peaks;
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
    }
}
