using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FoxTunes
{
    public class BandedWaveFormRenderer : RendererBase
    {
        public BandedWaveFormGenerator.WaveFormGeneratorData GeneratorData { get; private set; }

        public WaveFormRendererData RendererData { get; private set; }

        public BandedWaveFormGenerator Generator { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public IntegerConfigurationElement Resolution { get; private set; }

        public TextConfigurationElement ColorPalette { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Generator = ComponentRegistry.Instance.GetComponent<BandedWaveFormGenerator>();
            this.PlaybackManager = core.Managers.Playback;
            this.PlaybackManager.CurrentStreamChanged += this.OnCurrentStreamChanged;
            base.InitializeComponent(core);
        }

        protected override void OnConfigurationChanged()
        {
            if (this.Configuration != null)
            {
                this.Resolution = this.Configuration.GetElement<IntegerConfigurationElement>(
                    WaveFormGeneratorConfiguration.SECTION,
                    WaveFormGeneratorConfiguration.RESOLUTION_ELEMENT
                );
                this.ColorPalette = this.Configuration.GetElement<TextConfigurationElement>(
                    BandedWaveFormStreamPositionConfiguration.SECTION,
                    BandedWaveFormStreamPositionConfiguration.COLOR_PALETTE_ELEMENT
                );
                this.Resolution.ValueChanged += this.OnValueChanged;
                this.ColorPalette.ValueChanged += this.OnValueChanged;
#if NET40
                var task = TaskEx.Run(async () =>
#else
                var task = Task.Run(async () =>
#endif
                {
                    if (this.PlaybackManager.CurrentStream != null)
                    {
                        await this.Update(this.PlaybackManager.CurrentStream).ConfigureAwait(false);
                    }
                });
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
                this.GeneratorData = BandedWaveFormGenerator.WaveFormGeneratorData.Empty;
                await this.Clear().ConfigureAwait(false);
            }

            await this.RefreshBitmap().ConfigureAwait(false);
        }

        protected virtual void OnUpdated(object sender, EventArgs e)
        {
            this.Update();
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
                return false;
            }
            this.RendererData = Create(
                generatorData,
                width,
                height,
                this.GetColorPalettes(this.ColorPalette.Value, this.Colors)
            );
            if (this.RendererData == null)
            {
                return false;
            }
            this.Dispatch(this.Update);
            return true;
        }

        protected virtual IDictionary<string, IntPtr> GetColorPalettes(string value, Color[] colors)
        {
            var flags = default(int);
            var palettes = BandedWaveFormStreamPositionConfiguration.GetColorPalette(value, colors);
            var background = palettes.GetOrAdd(
                BandedWaveFormStreamPositionConfiguration.COLOR_PALETTE_BACKGROUND,
                () => DefaultColors.GetBackground()
            );
            //Switch the default colors to the VALUE palette if one was provided.
            colors = palettes.GetOrAdd(
                BandedWaveFormStreamPositionConfiguration.COLOR_PALETTE_VALUE,
                () => DefaultColors.GetValue(colors)
            );
            palettes.GetOrAdd(
                BandedWaveFormStreamPositionConfiguration.COLOR_PALETTE_LOW,
                () => DefaultColors.GetLow(colors)
            );
            palettes.GetOrAdd(
                BandedWaveFormStreamPositionConfiguration.COLOR_PALETTE_MID,
                () => DefaultColors.GetMid(colors)
            );
            palettes.GetOrAdd(
                BandedWaveFormStreamPositionConfiguration.COLOR_PALETTE_HIGH,
                () => DefaultColors.GetHigh(colors)
            );
            return palettes.ToDictionary(
                pair => pair.Key,
                pair =>
                {
                    flags = 0;
                    colors = pair.Value;
                    if (colors.Length > 1)
                    {
                        flags |= BitmapHelper.COLOR_FROM_Y;
                        if (new[] { BandedWaveFormStreamPositionConfiguration.COLOR_PALETTE_VALUE, BandedWaveFormStreamPositionConfiguration.COLOR_PALETTE_LOW, BandedWaveFormStreamPositionConfiguration.COLOR_PALETTE_MID, BandedWaveFormStreamPositionConfiguration.COLOR_PALETTE_HIGH }.Contains(pair.Key, StringComparer.OrdinalIgnoreCase))
                        {
                            colors = colors.MirrorGradient(false);
                        }
                    }
                    return BitmapHelper.CreatePalette(flags, colors);
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
                    info = BitmapHelper.CreateRenderInfo(bitmap, data.Colors[BandedWaveFormStreamPositionConfiguration.COLOR_PALETTE_BACKGROUND]);
                }
                else
                {
                    var palettes = this.GetColorPalettes(this.ColorPalette.Value, this.Colors);
                    info = BitmapHelper.CreateRenderInfo(bitmap, palettes[BandedWaveFormStreamPositionConfiguration.COLOR_PALETTE_BACKGROUND]);
                }
                BitmapHelper.DrawRectangle(ref info, 0, 0, data.Width, data.Height);
                bitmap.AddDirtyRect(new global::System.Windows.Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
            }
            finally
            {
                bitmap.Unlock();
            }
        }

        public async Task Render(WaveFormRendererData data)
        {
            var bitmap = default(WriteableBitmap);
            var success = default(bool);
            var info = default(WaveFormRenderInfo);

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
            }).ConfigureAwait(false);

            if (!success)
            {
                //No bitmap or failed to establish lock.
                return;
            }
            try
            {
                Render(ref info, data);
            }
            catch (Exception e)
            {
                Logger.Write(this.GetType(), LogLevel.Warn, "Failed to render wave form: {0}", e.Message);
            }

            await Windows.Invoke(() =>
            {
                bitmap.AddDirtyRect(new global::System.Windows.Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
                bitmap.Unlock();
            }).ConfigureAwait(false);
        }

        public void Update()
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

        protected override int GetPixelWidth(double width)
        {
            var data = this.GeneratorData;
            if (data != null)
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

        private static void Update(BandedWaveFormGenerator.WaveFormGeneratorData generatorData, WaveFormRendererData rendererData)
        {
            if (generatorData.Peak == 0)
            {
                return;
            }
            else
            {
                UpdatePeak(generatorData, rendererData);
            }

            UpdateElements(generatorData, rendererData);
        }

        private static void UpdatePeak(BandedWaveFormGenerator.WaveFormGeneratorData generatorData, WaveFormRendererData rendererData)
        {
            var peak = GetPeak(generatorData, rendererData);
            if (generatorData.Peak > rendererData.Peak || peak > rendererData.NormalizedPeak)
            {
                rendererData.Position = 0;
                rendererData.Peak = generatorData.Peak;
                rendererData.NormalizedPeak = peak;
            }
        }

        private static void UpdateElements(BandedWaveFormGenerator.WaveFormGeneratorData generatorData, WaveFormRendererData rendererData)
        {
            var center = rendererData.Height / 2.0f;
            var factor = rendererData.NormalizedPeak;

            if (factor == 0)
            {
                //Peak has not been calculated.
                //I don't know how this happens, but it does.
                return;
            }

            var data = generatorData.Data;
            var lowElements = rendererData.LowElements;
            var midElements = rendererData.MidElements;
            var highElements = rendererData.HighElements;
            var valuesPerElement = rendererData.ValuesPerElement;

            while (rendererData.Position < rendererData.Capacity)
            {
                var valuePosition = rendererData.Position * rendererData.ValuesPerElement;
                if ((valuePosition + rendererData.ValuesPerElement) > generatorData.Position)
                {
                    if (generatorData.Position <= generatorData.Capacity)
                    {
                        break;
                    }
                    else
                    {
                        valuesPerElement = generatorData.Capacity - valuePosition;
                    }
                }

                {

                    var y = default(int);
                    var height = default(int);

                    var lowValue = default(float);
                    var midValue = default(float);
                    var highValue = default(float);
                    for (var a = 0; a < valuesPerElement; a++)
                    {
                        lowValue += data[valuePosition + a].Low;
                        midValue += data[valuePosition + a].Mid;
                        highValue += data[valuePosition + a].High;
                    }
                    lowValue /= valuesPerElement;
                    midValue /= valuesPerElement;
                    highValue /= valuesPerElement;

                    lowValue /= factor;
                    midValue /= factor;
                    highValue /= factor;

                    lowValue = Math.Min(lowValue, 1);
                    midValue = Math.Min(midValue, 1);
                    highValue = Math.Min(highValue, 1);

                    {
                        y = Convert.ToInt32(center - (lowValue * center));
                        height = Math.Max(Convert.ToInt32((center - y) + (lowValue * center)), 1);

                        lowElements[rendererData.Position].X = rendererData.Position;
                        lowElements[rendererData.Position].Y = y;
                        lowElements[rendererData.Position].Width = 1;
                        lowElements[rendererData.Position].Height = height;
                    }

                    {
                        y = Convert.ToInt32(center - (midValue * center));
                        height = Math.Max(Convert.ToInt32((center - y) + (midValue * center)), 1);

                        midElements[rendererData.Position].X = rendererData.Position;
                        midElements[rendererData.Position].Y = y;
                        midElements[rendererData.Position].Width = 1;
                        midElements[rendererData.Position].Height = height;
                    }

                    {
                        y = Convert.ToInt32(center - (highValue * center));
                        height = Math.Max(Convert.ToInt32((center - y) + (highValue * center)), 1);

                        highElements[rendererData.Position].X = rendererData.Position;
                        highElements[rendererData.Position].Y = y;
                        highElements[rendererData.Position].Width = 1;
                        highElements[rendererData.Position].Height = height;
                    }
                }

                rendererData.Position++;
            }
        }

        public static float GetPeak(BandedWaveFormGenerator.WaveFormGeneratorData generatorData, WaveFormRendererData rendererData)
        {
            var data = generatorData.Data;
            var valuesPerElement = rendererData.ValuesPerElement;
            var peak = rendererData.NormalizedPeak;

            var position = rendererData.Position;
            while (position < rendererData.Capacity)
            {
                var valuePosition = position * rendererData.ValuesPerElement;
                if ((valuePosition + rendererData.ValuesPerElement) > generatorData.Position)
                {
                    if (generatorData.Position <= generatorData.Capacity)
                    {
                        break;
                    }
                    else
                    {
                        valuesPerElement = generatorData.Capacity - valuePosition;
                    }
                }

                var value = default(float);
                for (var a = 0; a < valuesPerElement; a++)
                {
                    value += Math.Max(
                        Math.Max(
                            data[valuePosition + a].Low,
                            data[valuePosition + a].Mid
                        ),
                        data[valuePosition + a].High
                    );
                }
                value /= valuesPerElement;

                peak = Math.Max(peak, value);

                if (peak >= 1)
                {
                    return 1;
                }

                position++;
            }

            return peak;
        }

        private static WaveFormRenderInfo GetRenderInfo(WriteableBitmap bitmap, WaveFormRendererData data)
        {
            var info = new WaveFormRenderInfo()
            {
                Background = BitmapHelper.CreateRenderInfo(bitmap, data.Colors[BandedWaveFormStreamPositionConfiguration.COLOR_PALETTE_BACKGROUND])
            };
            if (data.LowElements != null)
            {
                info.Low = BitmapHelper.CreateRenderInfo(bitmap, data.Colors[BandedWaveFormStreamPositionConfiguration.COLOR_PALETTE_LOW]);
            }
            if (data.MidElements != null)
            {
                info.Mid = BitmapHelper.CreateRenderInfo(bitmap, data.Colors[BandedWaveFormStreamPositionConfiguration.COLOR_PALETTE_MID]);
            }
            if (data.HighElements != null)
            {
                info.High = BitmapHelper.CreateRenderInfo(bitmap, data.Colors[BandedWaveFormStreamPositionConfiguration.COLOR_PALETTE_HIGH]);
            }
            return info;
        }

        public static void Render(ref WaveFormRenderInfo info, WaveFormRendererData data)
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

        public static WaveFormRendererData Create(BandedWaveFormGenerator.WaveFormGeneratorData generatorData, int width, int height, IDictionary<string, IntPtr> colors)
        {
            var valuesPerElement = generatorData.Capacity / width;
            if (valuesPerElement == 0)
            {
                return null;
            }
            var data = new WaveFormRendererData()
            {
                Width = width,
                Height = height,
                ValuesPerElement = valuesPerElement,
                Colors = colors,
                LowElements = new Int32Rect[width],
                MidElements = new Int32Rect[width],
                HighElements = new Int32Rect[width],
                Position = 0,
                Capacity = width,
                Peak = 0
            };
            return data;
        }

        public class WaveFormRendererData
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

            public float Peak;

            public float NormalizedPeak;

            ~WaveFormRendererData()
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
        public struct WaveFormRenderInfo
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

            public static Color[] GetLow(Color[] colors)
            {
                return new[]
                {
                    global::System.Windows.Media.Colors.Blue
                };
            }

            public static Color[] GetMid(Color[] colors)
            {
                return new[]
                {
                    global::System.Windows.Media.Colors.Orange.WithAlpha(-100)
                };
            }

            public static Color[] GetHigh(Color[] colors)
            {
                return new[]
                {
                    global::System.Windows.Media.Colors.White.WithAlpha(-100)
                };
            }

            public static Color[] GetValue(Color[] colors)
            {
                return colors;
            }
        }
    }
}
