using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FoxTunes
{
    public class MoodBarRenderer : RendererBase
    {
        public object SyncRoot = new object();

        public MoodBarGenerator.MoodBarGeneratorData GeneratorData { get; private set; }

        public MoodBarRendererData RendererData { get; private set; }

        public MoodBarGenerator Generator { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public IntegerConfigurationElement Resolution { get; private set; }

        public TextConfigurationElement ColorPalette { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Generator = ComponentRegistry.Instance.GetComponent<MoodBarGenerator>();
            this.PlaybackManager = core.Managers.Playback;
            this.PlaybackManager.CurrentStreamChanged += this.OnCurrentStreamChanged;
            base.InitializeComponent(core);
        }

        protected override void OnConfigurationChanged()
        {
            if (this.Configuration != null)
            {
                this.Resolution = this.Configuration.GetElement<IntegerConfigurationElement>(
                    MoodBarGeneratorConfiguration.SECTION,
                    MoodBarGeneratorConfiguration.RESOLUTION_ELEMENT
                );
                this.ColorPalette = this.Configuration.GetElement<TextConfigurationElement>(
                    MoodBarStreamPositionConfiguration.SECTION,
                    MoodBarStreamPositionConfiguration.COLOR_PALETTE_ELEMENT
                );
                this.Resolution.ValueChanged += this.OnValueChanged;
                this.ColorPalette.ValueChanged += this.OnValueChanged;
                this.OnCurrentStreamChanged(this, EventArgs.Empty);
            }
            base.OnConfigurationChanged();
        }

        protected virtual void OnCurrentStreamChanged(object sender, EventArgs e)
        {
            this.Dispatch(() => this.Update(this.PlaybackManager.CurrentStream));
        }

        protected virtual void OnValueChanged(object sender, EventArgs e)
        {
            if (object.ReferenceEquals(sender, this.Resolution))
            {
                //Changing resolution requires full refresh.
                this.Dispatch(() => this.Update(this.PlaybackManager.CurrentStream));
            }
            else
            {
                this.Dispatch(this.CreateData);
            }
        }

        protected virtual async Task Update(IOutputStream stream)
        {
            if (this.GeneratorData != null)
            {
                this.GeneratorData.Updated -= this.OnUpdated;
                if (this.GeneratorData.CancellationToken != null)
                {
                    this.GeneratorData.CancellationToken.Cancel();
                }
            }

            if (stream != null)
            {
                this.GeneratorData = this.Generator.Generate(stream);
                this.GeneratorData.Updated += this.OnUpdated;
            }
            else
            {
                this.GeneratorData = MoodBarGenerator.MoodBarGeneratorData.Empty;
                await this.Clear().ConfigureAwait(false);
            }

            await this.RefreshBitmap().ConfigureAwait(false);
        }

        protected virtual void OnUpdated(object sender, EventArgs e)
        {
            this.Dispatch(this.Update);
        }

        protected override bool CreateData(int width, int height)
        {
            if (this.Configuration == null)
            {
                return false;
            }
            var generatorData = this.GeneratorData;
            if (generatorData == null)
            {
                generatorData = MoodBarGenerator.MoodBarGeneratorData.Empty;
            }
            this.RendererData = Create(
                generatorData,
                width,
                height,
                this.GetColorPalettes(this.GetColorPaletteOrDefault(this.ColorPalette.Value))
            );
            if (this.RendererData == null)
            {
                return false;
            }
            this.Dispatch(this.Update);
            return true;
        }

        protected virtual IDictionary<string, IntPtr> GetColorPalettes(string value)
        {
            var flags = default(int);
            var palettes = MoodBarStreamPositionConfiguration.GetColorPalette(value);
            {
                var background = palettes.GetOrAdd(
                    MoodBarStreamPositionConfiguration.COLOR_PALETTE_BACKGROUND,
                    () => DefaultColors.GetBackground()
                );
                //Switch the default colors to the VALUE palette if one was provided.
                var colors = palettes.GetOrAdd(
                    MoodBarStreamPositionConfiguration.COLOR_PALETTE_VALUE,
                    () => DefaultColors.GetValue(new[] { this.ForegroundColor })
                );
                palettes.GetOrAdd(
                    MoodBarStreamPositionConfiguration.COLOR_PALETTE_LOW,
                    () => DefaultColors.GetLow(background, colors)
                );
                palettes.GetOrAdd(
                    MoodBarStreamPositionConfiguration.COLOR_PALETTE_MID,
                    () => DefaultColors.GetMid(background, colors)
                );
                palettes.GetOrAdd(
                    MoodBarStreamPositionConfiguration.COLOR_PALETTE_HIGH,
                    () => DefaultColors.GetHigh(colors)
                );
            }
            return palettes.ToDictionary(
                pair => pair.Key,
                pair =>
                {
                    if (pair.Value == null)
                    {
                        return IntPtr.Zero;
                    }
                    flags = 0;
                    var colors = pair.Value;
                    if (colors.Length > 1)
                    {
                        flags |= BitmapHelper.COLOR_FROM_Y;
                        if (new[] { MoodBarStreamPositionConfiguration.COLOR_PALETTE_LOW, MoodBarStreamPositionConfiguration.COLOR_PALETTE_MID, MoodBarStreamPositionConfiguration.COLOR_PALETTE_HIGH }.Contains(pair.Key, StringComparer.OrdinalIgnoreCase))
                        {
                            colors = colors.MirrorGradient(false);
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
                    info = BitmapHelper.CreateRenderInfo(bitmap, data.Colors[MoodBarStreamPositionConfiguration.COLOR_PALETTE_BACKGROUND]);
                }
                else
                {
                    var palettes = this.GetColorPalettes(this.GetColorPaletteOrDefault(this.ColorPalette.Value));
                    info = BitmapHelper.CreateRenderInfo(bitmap, palettes[MoodBarStreamPositionConfiguration.COLOR_PALETTE_BACKGROUND]);
                }
                BitmapHelper.DrawRectangle(ref info, 0, 0, data.Width, data.Height);
                bitmap.AddDirtyRect(new global::System.Windows.Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
            }
            finally
            {
                bitmap.Unlock();
            }
            this.Dispatch(this.Update);
        }

        public Task Render(MoodBarRendererData data)
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
                var info = GetRenderInfo(bitmap, data);
                Monitor.Enter(this.SyncRoot);
                try
                {
                    Render(ref info, data);
                }
                catch (Exception e)
                {
                    Logger.Write(this.GetType(), LogLevel.Warn, "Failed to render wave form: {0}", e.Message);
                }
                finally
                {
                    Monitor.Exit(this.SyncRoot);
                }
                bitmap.AddDirtyRect(new global::System.Windows.Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
                bitmap.Unlock();
            });
        }

        public void Update()
        {
            Monitor.Enter(this.SyncRoot);
            try
            {
                var generatorData = this.GeneratorData;
                var rendererData = this.RendererData;
                if (generatorData != null && rendererData != null)
                {
                    try
                    {
                        Update(
                            generatorData,
                            rendererData
                        );
                    }
                    catch (Exception e)
                    {
                        Logger.Write(this.GetType(), LogLevel.Warn, "Failed to update wave form data: {0}", e.Message);
                        return;
                    }
                    var task = this.Render(rendererData);
                }
            }
            finally
            {
                Monitor.Exit(this.SyncRoot);
            }
        }

        protected override int GetPixelWidth(double width)
        {
            var data = this.GeneratorData;
            if (data != null)
            {
                if (data.Capacity > 0)
                {
                    var valuesPerElement = Convert.ToInt32(
                        Math.Ceiling(
                            Math.Max(
                                (float)data.Capacity / width,
                                1
                            )
                        )
                    );
                    width = data.Capacity / valuesPerElement;
                }
            }
            return base.GetPixelWidth(width);
        }

        protected override void OnDisposing()
        {
            if (this.PlaybackManager != null)
            {
                this.PlaybackManager.CurrentStreamChanged -= this.OnCurrentStreamChanged;
            }
            if (this.GeneratorData != null)
            {
                this.GeneratorData.Updated -= this.OnUpdated;
            }
            if (this.Resolution != null)
            {
                this.Resolution.ValueChanged -= this.OnValueChanged;
            }
            if (this.ColorPalette != null)
            {
                this.ColorPalette.ValueChanged -= this.OnValueChanged;
            }
            base.OnDisposing();
        }

        private static void Update(MoodBarGenerator.MoodBarGeneratorData generatorData, MoodBarRendererData rendererData)
        {
            UpdateView(generatorData, rendererData);
            UpdateElements(rendererData);
        }

        private static void UpdateView(MoodBarGenerator.MoodBarGeneratorData generatorData, MoodBarRendererData rendererData)
        {
            var valuesPerElement = rendererData.ValuesPerElement;

            for (; rendererData.View.Position < rendererData.Width; rendererData.View.Position++)
            {
                var valuePosition = rendererData.View.Position * rendererData.ValuesPerElement;
                if ((valuePosition + rendererData.ValuesPerElement) > generatorData.Position)
                {
                    break;
                }

                var low = default(float);
                var mid = default(float);
                var high = default(float);
                for (var a = 0; a < valuesPerElement; a++)
                {
                    low += generatorData.Data[valuePosition + a].Low;
                    mid += generatorData.Data[valuePosition + a].Mid;
                    high += generatorData.Data[valuePosition + a].High;
                }

                low /= valuesPerElement;
                mid /= valuesPerElement;
                high /= valuesPerElement;

                low = Math.Min(low, 1);
                mid = Math.Min(mid, 1);
                high = Math.Min(high, 1);

                rendererData.View.Peak = Math.Max(
                    Math.Max(
                        Math.Max(
                            low,
                            mid
                        ),
                        high
                    ),
                    rendererData.View.Peak
                );
                rendererData.View.Low[rendererData.View.Position] = low;
                rendererData.View.Mid[rendererData.View.Position] = mid;
                rendererData.View.High[rendererData.View.Position] = high;
            }
        }

        private static void UpdateElements(MoodBarRendererData rendererData)
        {
            var center = rendererData.Height / 2.0f;
            var low = rendererData.View.Low;
            var mid = rendererData.View.Mid;
            var high = rendererData.View.High;
            var peak = rendererData.View.Peak;
            var lowElements = rendererData.LowElements;
            var midElements = rendererData.MidElements;
            var highElements = rendererData.HighElements;

            if (peak == 0)
            {
                peak = 1;
            }

            while (rendererData.Position < rendererData.View.Position)
            {
                {
                    var value = low[rendererData.Position] / peak;
                    var y = Convert.ToInt32(center - (value * center));
                    var height = Math.Max(Convert.ToInt32((center - y) + (value * center)), 1);

                    lowElements[rendererData.Position].X = rendererData.Position;
                    lowElements[rendererData.Position].Y = y;
                    lowElements[rendererData.Position].Width = 1;
                    lowElements[rendererData.Position].Height = height;
                }

                {
                    var value = mid[rendererData.Position] / peak;
                    var y = Convert.ToInt32(center - (value * center));
                    var height = Math.Max(Convert.ToInt32((center - y) + (value * center)), 1);

                    midElements[rendererData.Position].X = rendererData.Position;
                    midElements[rendererData.Position].Y = y;
                    midElements[rendererData.Position].Width = 1;
                    midElements[rendererData.Position].Height = height;
                }

                {
                    var value = high[rendererData.Position] / peak;
                    var y = Convert.ToInt32(center - (value * center));
                    var height = Math.Max(Convert.ToInt32((center - y) + (value * center)), 1);

                    highElements[rendererData.Position].X = rendererData.Position;
                    highElements[rendererData.Position].Y = y;
                    highElements[rendererData.Position].Width = 1;
                    highElements[rendererData.Position].Height = height;
                }

                rendererData.Position++;
            }
        }

        private static MoodBarRenderInfo GetRenderInfo(WriteableBitmap bitmap, MoodBarRendererData data)
        {
            var info = new MoodBarRenderInfo()
            {
                Background = BitmapHelper.CreateRenderInfo(bitmap, data.Colors[MoodBarStreamPositionConfiguration.COLOR_PALETTE_BACKGROUND])
            };
            if (data.LowElements != null)
            {
                info.Low = BitmapHelper.CreateRenderInfo(bitmap, data.Colors[MoodBarStreamPositionConfiguration.COLOR_PALETTE_LOW]);
            }
            if (data.MidElements != null)
            {
                info.Mid = BitmapHelper.CreateRenderInfo(bitmap, data.Colors[MoodBarStreamPositionConfiguration.COLOR_PALETTE_MID]);
            }
            if (data.HighElements != null)
            {
                info.High = BitmapHelper.CreateRenderInfo(bitmap, data.Colors[MoodBarStreamPositionConfiguration.COLOR_PALETTE_HIGH]);
            }
            return info;
        }

        public static void Render(ref MoodBarRenderInfo info, MoodBarRendererData data)
        {
            if (info.Background.Width != data.Width || info.Background.Height != data.Height)
            {
                //Bitmap does not match data.
                return;
            }
            BitmapHelper.DrawRectangle(ref info.Background, 0, 0, data.Width, data.Height);

            if (data.Position == 0)
            {
                //No data.
                return;
            }

            if (data.LowElements != null)
            {
                BitmapHelper.DrawRectangles(ref info.Low, data.LowElements, data.LowElements.Length);
            }
            if (data.MidElements != null)
            {
                BitmapHelper.DrawRectangles(ref info.Mid, data.MidElements, data.MidElements.Length);
            }
            if (data.HighElements != null)
            {
                BitmapHelper.DrawRectangles(ref info.High, data.HighElements, data.HighElements.Length);
            }
        }

        public static MoodBarRendererData Create(MoodBarGenerator.MoodBarGeneratorData generatorData, int width, int height, IDictionary<string, IntPtr> colors)
        {
            var valuesPerElement = generatorData.Capacity / width;
            if (valuesPerElement == 0)
            {
                valuesPerElement = 1;
            }
            var data = new MoodBarRendererData()
            {
                Width = width,
                Height = height,
                ValuesPerElement = valuesPerElement,
                Colors = colors,
                LowElements = new Int32Rect[width],
                MidElements = new Int32Rect[width],
                HighElements = new Int32Rect[width],
                Capacity = width,
                View = new MoodBarGeneratorDataView()
                {
                    Low = new float[width],
                    Mid = new float[width],
                    High = new float[width]
                }
            };
            return data;
        }

        public class MoodBarGeneratorDataView
        {
            public float[] Low;

            public float[] Mid;

            public float[] High;

            public float Peak;

            public int Position;
        }

        public class MoodBarRendererData
        {
            public int Width;

            public int Height;

            public int ValuesPerElement;

            public IDictionary<string, IntPtr> Colors;

            public Int32Rect[] LowElements;

            public Int32Rect[] MidElements;

            public Int32Rect[] HighElements;

            public int Position;

            public int Capacity;

            public MoodBarGeneratorDataView View;

            ~MoodBarRendererData()
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
        public struct MoodBarRenderInfo
        {
            public BitmapHelper.RenderInfo Low;

            public BitmapHelper.RenderInfo Mid;

            public BitmapHelper.RenderInfo High;

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

            public static Color[] GetLow(Color[] background, Color[] colors)
            {
                var transparency = background.Length > 1 || background.FirstOrDefault().A != 255;
                if (transparency)
                {
                    return colors.WithAlpha(-200);
                }
                else
                {
                    var color = background.FirstOrDefault();
                    return colors.Interpolate(color, 0.8f);
                }
            }

            public static Color[] GetMid(Color[] background, Color[] colors)
            {
                var transparency = background.Length > 1 || background.FirstOrDefault().A != 255;
                if (transparency)
                {
                    return colors.WithAlpha(-100);
                }
                else
                {
                    var color = background.FirstOrDefault();
                    return colors.Interpolate(color, 0.4f);
                }
            }

            public static Color[] GetHigh(Color[] colors)
            {
                return colors;
            }

            public static Color[] GetValue(Color[] colors)
            {
                return colors;
            }
        }
    }
}
