using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FoxTunes
{
    public class EnhancedSpectrumRenderer : VisualizationBase
    {
        const int MARGIN_MIN = 4;

        const int MARGIN_ZERO = 0;

        const int MARGIN_ONE = 1;

        public SpectrumRendererData RendererData { get; private set; }

        public SelectionConfigurationElement Bands { get; private set; }

        public BooleanConfigurationElement ShowPeak { get; private set; }

        public BooleanConfigurationElement ShowRms { get; private set; }

        public BooleanConfigurationElement ShowCrestFactor { get; private set; }

        public TextConfigurationElement ColorPalette { get; private set; }

        public IntegerConfigurationElement Duration { get; private set; }

        public SelectionConfigurationElement FFTSize { get; private set; }

        protected override void OnConfigurationChanged()
        {
            if (this.Configuration != null)
            {
                this.Bands = this.Configuration.GetElement<SelectionConfigurationElement>(
                   EnhancedSpectrumConfiguration.SECTION,
                   EnhancedSpectrumConfiguration.BANDS_ELEMENT
               );
                this.ShowPeak = this.Configuration.GetElement<BooleanConfigurationElement>(
                    EnhancedSpectrumConfiguration.SECTION,
                    EnhancedSpectrumConfiguration.PEAK_ELEMENT
                 );
                this.ShowRms = this.Configuration.GetElement<BooleanConfigurationElement>(
                    EnhancedSpectrumConfiguration.SECTION,
                    EnhancedSpectrumConfiguration.RMS_ELEMENT
                 );
                this.ShowCrestFactor = this.Configuration.GetElement<BooleanConfigurationElement>(
                    EnhancedSpectrumConfiguration.SECTION,
                    EnhancedSpectrumConfiguration.CREST_ELEMENT
                );
                this.ColorPalette = this.Configuration.GetElement<TextConfigurationElement>(
                    EnhancedSpectrumConfiguration.SECTION,
                    EnhancedSpectrumConfiguration.COLOR_PALETTE_ELEMENT
                );
                this.Duration = this.Configuration.GetElement<IntegerConfigurationElement>(
                    EnhancedSpectrumConfiguration.SECTION,
                    EnhancedSpectrumConfiguration.DURATION_ELEMENT
                );
                this.FFTSize = this.Configuration.GetElement<SelectionConfigurationElement>(
                   VisualizationBehaviourConfiguration.SECTION,
                   VisualizationBehaviourConfiguration.FFT_SIZE_ELEMENT
                );
                this.Bands.ValueChanged += this.OnValueChanged;
                this.ShowPeak.ValueChanged += this.OnValueChanged;
                this.ShowRms.ValueChanged += this.OnValueChanged;
                this.ShowCrestFactor.ValueChanged += this.OnValueChanged;
                this.ColorPalette.ValueChanged += this.OnValueChanged;
                this.Duration.ValueChanged += this.OnValueChanged;
                this.FFTSize.ValueChanged += this.OnValueChanged;
                var task = this.CreateBitmap(true);
            }
            base.OnConfigurationChanged();
        }

        protected virtual void OnValueChanged(object sender, EventArgs e)
        {
            if (object.ReferenceEquals(sender, this.Bands))
            {
                //Changing bands requires full refresh.
                var task = this.CreateBitmap(true);
            }
            else
            {
                var task = this.CreateData();
            }
        }

        protected override bool CreateData(int width, int height)
        {
            if (this.Configuration == null)
            {
                return false;
            }
            this.RendererData = Create(
                width,
                height,
                EnhancedSpectrumConfiguration.GetBands(this.Bands.Value),
                EnhancedSpectrumConfiguration.GetFFTSize(this.FFTSize.Value, this.Bands.Value),
                this.ShowPeak.Value,
                this.ShowRms.Value,
                this.ShowCrestFactor.Value,
                this.Duration.Value,
                this.GetColorPalettes(this.ColorPalette.Value, this.ShowPeak.Value, this.ShowRms.Value, this.ShowCrestFactor.Value, this.Colors)
            );
            return true;
        }

        protected virtual IDictionary<string, IntPtr> GetColorPalettes(string value, bool showPeak, bool showRms, bool showCrest, Color[] colors)
        {
            var palettes = EnhancedSpectrumConfiguration.GetColorPalette(value, colors);
            //Switch the default colors to the VALUE palette if one was provided.
            colors = palettes.GetOrAdd(
                EnhancedSpectrumConfiguration.COLOR_PALETTE_VALUE,
                () => GetDefaultColors(EnhancedSpectrumConfiguration.COLOR_PALETTE_VALUE, showPeak, showRms, colors)
            );
            palettes.GetOrAdd(
                EnhancedSpectrumConfiguration.COLOR_PALETTE_BACKGROUND,
                () => GetDefaultColors(EnhancedSpectrumConfiguration.COLOR_PALETTE_BACKGROUND, showPeak, showRms, colors)
            );
            if (showPeak)
            {
                palettes.GetOrAdd(
                    EnhancedSpectrumConfiguration.COLOR_PALETTE_PEAK,
                    () => GetDefaultColors(EnhancedSpectrumConfiguration.COLOR_PALETTE_PEAK, showPeak, showRms, colors)
                );
            }
            if (showRms)
            {
                palettes.GetOrAdd(
                    EnhancedSpectrumConfiguration.COLOR_PALETTE_RMS,
                    () => GetDefaultColors(EnhancedSpectrumConfiguration.COLOR_PALETTE_RMS, showPeak, showRms, colors)
                );
            }
            if (showCrest)
            {
                palettes.GetOrAdd(
                    EnhancedSpectrumConfiguration.COLOR_PALETTE_CREST,
                    () => GetDefaultColors(EnhancedSpectrumConfiguration.COLOR_PALETTE_CREST, showPeak, showRms, colors)
                );
            }
            return palettes.ToDictionary(
                pair => pair.Key,
                pair =>
                {
                    var flags = 0;
                    if (pair.Value.Length > 1)
                    {
                        flags |= BitmapHelper.COLOR_FROM_Y;
                    }
                    return BitmapHelper.CreatePalette(flags, pair.Value);
                },
                StringComparer.OrdinalIgnoreCase
            );
        }

        private static Color[] GetDefaultColors(string name, bool showPeak, bool showRms, Color[] colors)
        {
            switch (name)
            {
                case EnhancedSpectrumConfiguration.COLOR_PALETTE_PEAK:
                    if (showRms)
                    {
                        return colors.WithAlpha(-200);
                    }
                    else
                    {
                        return colors.WithAlpha(-100);
                    }
                case EnhancedSpectrumConfiguration.COLOR_PALETTE_RMS:
                    if (showPeak)
                    {
                        return colors.WithAlpha(-100);
                    }
                    else
                    {
                        return colors.WithAlpha(-50);
                    }
                case EnhancedSpectrumConfiguration.COLOR_PALETTE_VALUE:
                    if (showPeak || showRms)
                    {
                        return colors.WithAlpha(-25);
                    }
                    else
                    {
                        return colors;
                    }
                case EnhancedSpectrumConfiguration.COLOR_PALETTE_CREST:
                    return new[]
                    {
                        global::System.Windows.Media.Colors.Red
                    };
                case EnhancedSpectrumConfiguration.COLOR_PALETTE_BACKGROUND:
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
                    info = BitmapHelper.CreateRenderInfo(bitmap, data.Colors[EnhancedSpectrumConfiguration.COLOR_PALETTE_BACKGROUND]);
                }
                else
                {
                    var palettes = this.GetColorPalettes(this.ColorPalette.Value, false, false, false, this.Colors);
                    info = BitmapHelper.CreateRenderInfo(bitmap, palettes[EnhancedSpectrumConfiguration.COLOR_PALETTE_BACKGROUND]);
                }
                BitmapHelper.DrawRectangle(ref info, 0, 0, data.Width, data.Height);
                bitmap.AddDirtyRect(new global::System.Windows.Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
            }
            finally
            {
                bitmap.Unlock();
            }
        }

        protected virtual async Task Render(SpectrumRendererData data)
        {
            var bitmap = default(WriteableBitmap);
            var success = default(bool);
            var info = default(SpectrumRenderInfo);

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
                Logger.Write(this.GetType(), LogLevel.Warn, "Failed to render spectrum: {0}", e.Message);
#else
                Logger.Write(this.GetType(), LogLevel.Warn, "Failed to render spectrum, disabling: {0}", e.Message);
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
                else
                {
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
                }

                var task = this.Render(data);
            }
            catch (Exception exception)
            {
#if DEBUG
                Logger.Write(this.GetType(), LogLevel.Warn, "Failed to update spectrum data: {0}", exception.Message);
                this.Restart();
#else
                Logger.Write(this.GetType(), LogLevel.Warn, "Failed to update spectrum data, disabling: {0}", exception.Message);
#endif
            }
        }

        protected override int GetPixelWidth(double width)
        {
            if (this.Bands == null)
            {
                return 0;
            }
            var bands = EnhancedSpectrumConfiguration.GetBands(this.Bands.Value);
            return base.GetPixelWidth(Math.Max(bands.Length * (Convert.ToInt32(width) / bands.Length), bands.Length));
        }

        protected override void OnDisposing()
        {
            if (this.ShowPeak != null)
            {
                this.ShowPeak.ValueChanged -= this.OnValueChanged;
            }
            if (this.ShowRms != null)
            {
                this.ShowRms.ValueChanged -= this.OnValueChanged;
            }
            if (this.ShowCrestFactor != null)
            {
                this.ShowCrestFactor.ValueChanged -= this.OnValueChanged;
            }
            if (this.ColorPalette != null)
            {
                this.ColorPalette.ValueChanged -= this.OnValueChanged;
            }
            if (this.Duration != null)
            {
                this.Duration.ValueChanged -= this.OnValueChanged;
            }
            if (this.FFTSize != null)
            {
                this.FFTSize.ValueChanged -= this.OnValueChanged;
            }
            base.OnDisposing();
        }

        private static SpectrumRenderInfo GetRenderInfo(WriteableBitmap bitmap, SpectrumRendererData data)
        {
            var info = new SpectrumRenderInfo()
            {
                Background = BitmapHelper.CreateRenderInfo(bitmap, data.Colors[EnhancedSpectrumConfiguration.COLOR_PALETTE_BACKGROUND])
            };
            if (data.PeakElements != null)
            {
                info.Peak = BitmapHelper.CreateRenderInfo(bitmap, data.Colors[EnhancedSpectrumConfiguration.COLOR_PALETTE_PEAK]);
            }
            if (data.RmsElements != null)
            {
                info.Rms = BitmapHelper.CreateRenderInfo(bitmap, data.Colors[EnhancedSpectrumConfiguration.COLOR_PALETTE_RMS]);
            }
            if (data.ValueElements != null)
            {
                info.Value = BitmapHelper.CreateRenderInfo(bitmap, data.Colors[EnhancedSpectrumConfiguration.COLOR_PALETTE_VALUE]);
            }
            if (data.CrestPoints != null)
            {
                info.Crest = BitmapHelper.CreateRenderInfo(bitmap, data.Colors[EnhancedSpectrumConfiguration.COLOR_PALETTE_CREST]);
            }
            return info;
        }

        private static void Render(ref SpectrumRenderInfo info, SpectrumRendererData data)
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

            if (data.CrestPoints != null)
            {
                //TODO: Switch to BitmapHelper.DrawLines.
                for (var a = 0; a < data.CrestPoints.Length - 1; a++)
                {
                    var point1 = data.CrestPoints[a];
                    var point2 = data.CrestPoints[a + 1];
                    BitmapHelper.DrawLine(
                        ref info.Crest,
                        point1.X,
                        point1.Y,
                        point2.X,
                        point2.Y
                    );
                }
            }
        }

        private static void UpdateValues(SpectrumRendererData data)
        {
            var bands = data.Bands;
            var position = default(int);

            for (int a = FrequencyToIndex(data.MinBand, data.FFTSize, data.Rate), b = a; a < data.FFTSize; a++)
            {
                var frequency = IndexToFrequency(a, data.FFTSize, data.Rate);
                while (frequency > bands[position])
                {
                    if (position < (bands.Length - 1))
                    {
                        UpdateValue(data, position, b, a);
                        b = a;
                        position++;
                    }
                    else
                    {
                        UpdateValue(data, position, b, a);
                        return;
                    }
                }
            }
        }

        private static void UpdateValue(SpectrumRendererData data, int band, int start, int end)
        {
            var values = data.Data;
            var peakValues = data.History.Peak;
            var rmsValues = data.History.Rms;
            var value = default(float);
            var peak = default(float);
            var rms = default(float);
            var doPeaks = peakValues != null;
            var doRms = rmsValues != null;
            var count = end - start;

            if (count > 0)
            {
                for (var a = start; a < end; a++)
                {
                    value = Math.Max(values[0, a], value);
                    if (doPeaks)
                    {
                        peak = Math.Max(peakValues[0, a], peak);
                    }
                    if (doRms)
                    {
                        rms = Math.Max(rmsValues[0, a], rms);
                    }
                }
            }
            else
            {
                //If we don't have data then average the closest available bins.
                if (start > 0)
                {
                    start--;
                }
                if (end < data.FFTSize)
                {
                    end++;
                }
                count = end - start;
                if (count == 0)
                {
                    //Sorry.
                    return;
                }
                for (var a = start; a < end; a++)
                {
                    value += values[0, a];
                    if (doPeaks)
                    {
                        peak += peakValues[0, a];
                    }
                    if (doRms)
                    {
                        rms += rmsValues[0, a];
                    }
                }
                value /= count;
                if (doPeaks)
                {
                    peak /= count;
                }
                if (doRms)
                {
                    rms /= count;
                }
            }

            data.Values[band] = ToDecibelFixed(value);
            if (doPeaks)
            {
                data.PeakValues[band] = ToDecibelFixed(peak);
            }
            if (doRms)
            {
                data.RmsValues[band] = ToDecibelFixed(rms);
            }
        }

        private static void UpdateElementsFast(SpectrumRendererData data)
        {
            UpdateElementsFast(data.Values, data.ValueElements, data.Width, data.Height, data.Margin, Orientation.Vertical);
            if (data.PeakValues != null && data.PeakElements != null)
            {
                UpdateElementsFast(data.PeakValues, data.PeakElements, data.Width, data.Height, data.Margin, Orientation.Vertical);
            }
            if (data.RmsValues != null && data.RmsElements != null)
            {
                UpdateElementsFast(data.RmsValues, data.RmsElements, data.Width, data.Height, data.Margin, Orientation.Vertical);
            }
            if (data.PeakValues != null && data.RmsValues != null && data.CrestPoints != null)
            {
                UpdateCrestPointsFast(data.PeakValues, data.RmsValues, data.CrestPoints, data.Width, data.Height);
            }
        }

        private static void UpdateCrestPointsFast(float[] values, float[] rms, Int32Point[] elements, int width, int height)
        {
            if (values.Length == 0)
            {
                return;
            }
            height = height - 1;
            var step = width / values.Length;
            var offset = default(float);
            for (var a = 0; a < values.Length; a++)
            {
                var x = (a * step) + (step / 2);
                var y = default(int);
                if (values[a] > 0)
                {
                    y = height - Convert.ToInt32(ToCrestFactor(values[a], rms[a], offset) * height);
                }
                else
                {
                    y = height;
                }
                elements[a].X = x;
                elements[a].Y = y;
            }
        }

        private static void UpdateElementsSmooth(SpectrumRendererData data)
        {
            UpdateElementsSmooth(data.Values, data.ValueElements, data.Width, data.Height, data.Margin, Orientation.Vertical);
            if (data.PeakValues != null && data.PeakElements != null)
            {
                UpdateElementsSmooth(data.PeakValues, data.PeakElements, data.Width, data.Height, data.Margin, Orientation.Vertical);
            }
            if (data.RmsValues != null && data.RmsElements != null)
            {
                UpdateElementsSmooth(data.RmsValues, data.RmsElements, data.Width, data.Height, data.Margin, Orientation.Vertical);
            }
            if (data.PeakValues != null && data.RmsValues != null && data.CrestPoints != null)
            {
                UpdateCrestPointsSmooth(data.PeakValues, data.RmsValues, data.CrestPoints, data.Width, data.Height);
            }
        }

        private static void UpdateCrestPointsSmooth(float[] values, float[] rms, Int32Point[] elements, int width, int height)
        {
            if (values.Length == 0)
            {
                return;
            }
            height = height - 1;
            var step = width / values.Length;
            var offset = default(float);
            var minChange = 1;
            var maxChange = Convert.ToInt32(height * 0.05f);
            for (var a = 0; a < values.Length; a++)
            {
                var x = (a * step) + (step / 2);
                var y = default(int);
                if (values[a] > 0)
                {
                    y = height - Convert.ToInt32(ToCrestFactor(values[a], rms[a], offset) * height);
                }
                else
                {
                    y = height;
                }
                elements[a].X = x;
                Animate(ref elements[a].Y, y, 1, height, minChange, maxChange);
            }
        }

        public static SpectrumRendererData Create(int width, int height, int[] bands, int fftSize, bool showPeak, bool showRms, bool showCrest, int history, IDictionary<string, IntPtr> colors)
        {
            var margin = width > (bands.Length * MARGIN_MIN) ? MARGIN_ONE : MARGIN_ZERO;
            var data = new SpectrumRendererData(history, showPeak, showRms)
            {
                Width = width,
                Height = height,
                Margin = margin,
                Bands = bands,
                MinBand = bands[0],
                MaxBand = bands[bands.Length - 1],
                FFTSize = fftSize,
                Values = new float[bands.Length],
                Colors = colors,
                ValueElements = new Int32Rect[bands.Length],
            };
            if (showPeak)
            {
                data.PeakValues = new float[bands.Length];
                data.PeakElements = new Int32Rect[bands.Length];
            }
            if (showRms)
            {
                data.RmsValues = new float[bands.Length];
                data.RmsElements = new Int32Rect[bands.Length];
            }
            if (showPeak && showRms && showCrest)
            {
                data.CrestPoints = new Int32Point[bands.Length];
            }
            return data;
        }

        public class SpectrumRendererData : FFTVisualizationData
        {
            public SpectrumRendererData(int history, bool showPeak, bool showRms)
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

            public int[] Bands;

            public int MinBand;

            public int MaxBand;

            public float[] Values;

            public float[] PeakValues;

            public float[] RmsValues;

            public int Width;

            public int Height;

            public int Margin;

            public IDictionary<string, IntPtr> Colors;

            public Int32Rect[] ValueElements;

            public Int32Rect[] PeakElements;

            public Int32Rect[] RmsElements;

            public Int32Point[] CrestPoints;

            public DateTime LastUpdated;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SpectrumRenderInfo
        {
            public BitmapHelper.RenderInfo Peak;

            public BitmapHelper.RenderInfo Rms;

            public BitmapHelper.RenderInfo Value;

            public BitmapHelper.RenderInfo Crest;

            public BitmapHelper.RenderInfo Background;
        }
    }
}
