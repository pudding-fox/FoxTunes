using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FoxTunes
{
    public class WaveFormRenderer : RendererBase
    {
        public WaveFormGenerator.WaveFormGeneratorData GeneratorData { get; private set; }

        public WaveFormRendererData RendererData { get; private set; }

        public WaveFormGenerator Generator { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public SelectionConfigurationElement Mode { get; private set; }

        public IntegerConfigurationElement Resolution { get; private set; }

        public BooleanConfigurationElement Rms { get; private set; }

        public TextConfigurationElement ColorPalette { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Generator = ComponentRegistry.Instance.GetComponent<WaveFormGenerator>();
            this.PlaybackManager = core.Managers.Playback;
            this.PlaybackManager.CurrentStreamChanged += this.OnCurrentStreamChanged;
            base.InitializeComponent(core);
        }

        protected override void OnConfigurationChanged()
        {
            if (this.Configuration != null)
            {
                this.Mode = this.Configuration.GetElement<SelectionConfigurationElement>(
                    WaveFormStreamPositionConfiguration.SECTION,
                    WaveFormStreamPositionConfiguration.MODE_ELEMENT
                );
                this.Resolution = this.Configuration.GetElement<IntegerConfigurationElement>(
                    WaveFormGeneratorConfiguration.SECTION,
                    WaveFormGeneratorConfiguration.RESOLUTION_ELEMENT
                );
                this.Rms = this.Configuration.GetElement<BooleanConfigurationElement>(
                    WaveFormStreamPositionConfiguration.SECTION,
                    WaveFormStreamPositionConfiguration.RMS_ELEMENT
                );
                this.ColorPalette = this.Configuration.GetElement<TextConfigurationElement>(
                    WaveFormStreamPositionConfiguration.SECTION,
                    WaveFormStreamPositionConfiguration.COLOR_PALETTE_ELEMENT
                );
                this.Mode.ValueChanged += this.OnValueChanged;
                this.Resolution.ValueChanged += this.OnValueChanged;
                this.Rms.ValueChanged += this.OnValueChanged;
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
                this.GeneratorData = WaveFormGenerator.WaveFormGeneratorData.Empty;
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
            var mode = WaveFormStreamPositionConfiguration.GetMode(this.Mode.Value);
            this.RendererData = Create(
                generatorData,
                width,
                height,
                this.Rms.Value,
                mode,
                this.GetColorPalettes(this.ColorPalette.Value, this.Rms.Value, generatorData.Channels, this.Colors, mode)
            );
            if (this.RendererData == null)
            {
                return false;
            }
            this.Dispatch(this.Update);
            return true;
        }

        protected virtual IDictionary<string, IntPtr> GetColorPalettes(string value, bool showRms, int channels, Color[] colors, WaveFormRendererMode mode)
        {
            var flags = default(int);
            var palettes = WaveFormStreamPositionConfiguration.GetColorPalette(value, colors);
            //Switch the default colors to the VALUE palette if one was provided.
            colors = palettes.GetOrAdd(
                WaveFormStreamPositionConfiguration.COLOR_PALETTE_VALUE,
                () => GetDefaultColors(WaveFormStreamPositionConfiguration.COLOR_PALETTE_VALUE, showRms, colors)
            );
            palettes.GetOrAdd(
                WaveFormStreamPositionConfiguration.COLOR_PALETTE_BACKGROUND,
                () => GetDefaultColors(WaveFormStreamPositionConfiguration.COLOR_PALETTE_BACKGROUND, showRms, colors)
            );
            if (showRms)
            {
                palettes.GetOrAdd(
                    WaveFormStreamPositionConfiguration.COLOR_PALETTE_RMS,
                    () => GetDefaultColors(WaveFormStreamPositionConfiguration.COLOR_PALETTE_RMS, showRms, colors)
                );
            }
            return palettes.ToDictionary(
                pair => pair.Key,
                pair =>
                {
                    flags = 0;
                    colors = pair.Value;
                    if (colors.Length > 1)
                    {
                        flags |= BitmapHelper.COLOR_FROM_Y;
                        if (new[] { WaveFormStreamPositionConfiguration.COLOR_PALETTE_VALUE, WaveFormStreamPositionConfiguration.COLOR_PALETTE_RMS }.Contains(pair.Key, StringComparer.OrdinalIgnoreCase))
                        {
                            colors = colors.MirrorGradient(true);
                            switch (mode)
                            {
                                case WaveFormRendererMode.Seperate:
                                    colors = colors.DuplicateGradient(channels);
                                    break;
                            }
                        }
                    }
                    return BitmapHelper.CreatePalette(flags, colors);
                },
                StringComparer.OrdinalIgnoreCase
            );
        }

        private static Color[] GetDefaultColors(string name, bool showRms, Color[] colors)
        {
            var color = colors.FirstOrDefault();
            switch (name)
            {
                case WaveFormStreamPositionConfiguration.COLOR_PALETTE_RMS:
                    if (colors.Length > 1)
                    {
                        return colors.WithAlpha(-50);
                    }
                    else
                    {
                        const byte SHADE = 30;
                        var contrast = new Color()
                        {
                            R = SHADE,
                            G = SHADE,
                            B = SHADE
                        };
                        return new[]
                        {
                            color.Shade(contrast)
                        };
                    }
                case WaveFormStreamPositionConfiguration.COLOR_PALETTE_VALUE:
                    return colors;
                case WaveFormStreamPositionConfiguration.COLOR_PALETTE_BACKGROUND:
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
                    info = BitmapHelper.CreateRenderInfo(bitmap, data.Colors[WaveFormStreamPositionConfiguration.COLOR_PALETTE_BACKGROUND]);
                }
                else
                {
                    var palettes = this.GetColorPalettes(this.ColorPalette.Value, false, 0, this.Colors, WaveFormRendererMode.None);
                    info = BitmapHelper.CreateRenderInfo(bitmap, palettes[WaveFormStreamPositionConfiguration.COLOR_PALETTE_BACKGROUND]);
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
            if (this.Mode != null)
            {
                this.Mode.ValueChanged -= this.OnValueChanged;
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

        private static void Update(WaveFormGenerator.WaveFormGeneratorData generatorData, WaveFormRendererData rendererData)
        {
            if (generatorData.Peak == 0)
            {
                return;
            }
            else
            {
                UpdatePeak(generatorData, rendererData);
            }

            if (rendererData.Channels == 1)
            {
                UpdateMono(generatorData, rendererData);
            }
            else
            {
                UpdateSeperate(generatorData, rendererData);
            }
        }

        private static void UpdatePeak(WaveFormGenerator.WaveFormGeneratorData generatorData, WaveFormRendererData rendererData)
        {
            var peak = GetPeak(generatorData, rendererData);
            if (generatorData.Peak > rendererData.Peak || peak > rendererData.NormalizedPeak)
            {
                rendererData.Position = 0;
                rendererData.Peak = generatorData.Peak;
                rendererData.NormalizedPeak = peak;
            }
        }

        private static void UpdateMono(WaveFormGenerator.WaveFormGeneratorData generatorData, WaveFormRendererData rendererData)
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
            var waveElements = rendererData.WaveElements;
            var powerElements = rendererData.PowerElements;
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

                    var topValue = default(float);
                    var bottomValue = default(float);
                    for (var a = 0; a < valuesPerElement; a++)
                    {
                        for (var b = 0; b < generatorData.Channels; b++)
                        {
                            topValue += Math.Abs(data[valuePosition + a, b].Min);
                            bottomValue += Math.Abs(data[valuePosition + a, b].Max);
                        }
                    }
                    topValue /= (valuesPerElement * generatorData.Channels);
                    bottomValue /= (valuesPerElement * generatorData.Channels);

                    topValue /= factor;
                    bottomValue /= factor;

                    topValue = Math.Min(topValue, 1);
                    bottomValue = Math.Min(bottomValue, 1);

                    y = Convert.ToInt32(center - (topValue * center));
                    height = Math.Max(Convert.ToInt32((center - y) + (bottomValue * center)), 1);

                    waveElements[0, rendererData.Position].X = rendererData.Position;
                    waveElements[0, rendererData.Position].Y = y;
                    waveElements[0, rendererData.Position].Width = 1;
                    waveElements[0, rendererData.Position].Height = height;

                }

                if (powerElements != null)
                {

                    var y = default(int);
                    var height = default(int);

                    var value = default(float);
                    for (var a = 0; a < valuesPerElement; a++)
                    {
                        for (var b = 0; b < generatorData.Channels; b++)
                        {
                            value += Math.Abs(data[valuePosition + a, b].Rms);
                        }
                    }
                    value /= (valuesPerElement * generatorData.Channels);

                    value /= factor;

                    value = Math.Min(value, 1);

                    y = Convert.ToInt32(center - (value * center));
                    height = Math.Max(Convert.ToInt32((center - y) + (value * center)), 1);

                    powerElements[0, rendererData.Position].X = rendererData.Position;
                    powerElements[0, rendererData.Position].Y = y;
                    powerElements[0, rendererData.Position].Width = 1;
                    powerElements[0, rendererData.Position].Height = height;

                }

                rendererData.Position++;
            }
        }

        private static void UpdateSeperate(WaveFormGenerator.WaveFormGeneratorData generatorData, WaveFormRendererData rendererData)
        {
            var factor = rendererData.NormalizedPeak / generatorData.Channels;

            if (factor == 0)
            {
                //Peak has not been calculated.
                //I don't know how this happens, but it does.
                return;
            }

            var data = generatorData.Data;
            var waveElements = rendererData.WaveElements;
            var powerElements = rendererData.PowerElements;
            var valuesPerElement = rendererData.ValuesPerElement;

            var waveHeight = rendererData.Height / generatorData.Channels;

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

                for (var channel = 0; channel < generatorData.Channels; channel++)
                {
                    var waveCenter = (waveHeight * channel) + (waveHeight / 2);

                    {

                        var y = default(int);
                        var height = default(int);

                        var topValue = default(float);
                        var bottomValue = default(float);
                        for (var a = 0; a < valuesPerElement; a++)
                        {
                            topValue += Math.Abs(data[valuePosition + a, channel].Min);
                            bottomValue += Math.Abs(data[valuePosition + a, channel].Max);
                        }
                        topValue /= (valuesPerElement * generatorData.Channels);
                        bottomValue /= (valuesPerElement * generatorData.Channels);

                        topValue /= factor;
                        bottomValue /= factor;

                        topValue = Math.Min(topValue, 1);
                        bottomValue = Math.Min(bottomValue, 1);

                        y = Convert.ToInt32(waveCenter - (topValue * (waveHeight / 2)));
                        height = Math.Max(Convert.ToInt32((waveCenter - y) + (bottomValue * (waveHeight / 2))), 1);

                        waveElements[channel, rendererData.Position].X = rendererData.Position;
                        waveElements[channel, rendererData.Position].Y = y;
                        waveElements[channel, rendererData.Position].Width = 1;
                        waveElements[channel, rendererData.Position].Height = height;

                    }

                    if (powerElements != null)
                    {

                        var y = default(int);
                        var height = default(int);

                        var value = default(float);
                        for (var a = 0; a < valuesPerElement; a++)
                        {
                            value += Math.Abs(data[valuePosition + a, channel].Rms);
                        }
                        value /= (valuesPerElement * generatorData.Channels);

                        value /= factor;

                        value = Math.Min(value, 1);

                        y = Convert.ToInt32(waveCenter - (value * (waveHeight / 2)));
                        height = Math.Max(Convert.ToInt32((waveCenter - y) + (value * (waveHeight / 2))), 1);

                        powerElements[channel, rendererData.Position].X = rendererData.Position;
                        powerElements[channel, rendererData.Position].Y = y;
                        powerElements[channel, rendererData.Position].Width = 1;
                        powerElements[channel, rendererData.Position].Height = height;

                    }
                }

                rendererData.Position++;
            }
        }

        public static float GetPeak(WaveFormGenerator.WaveFormGeneratorData generatorData, WaveFormRendererData rendererData)
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
                    for (var b = 0; b < generatorData.Channels; b++)
                    {
                        value += Math.Max(
                            Math.Abs(data[valuePosition + a, b].Min),
                            Math.Abs(data[valuePosition + a, b].Max)
                        );
                    }
                }
                value /= (valuesPerElement * generatorData.Channels);

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
                Background = BitmapHelper.CreateRenderInfo(bitmap, data.Colors[WaveFormStreamPositionConfiguration.COLOR_PALETTE_BACKGROUND])
            };
            if (data.PowerElements != null)
            {
                info.Rms = BitmapHelper.CreateRenderInfo(bitmap, data.Colors[WaveFormStreamPositionConfiguration.COLOR_PALETTE_RMS]);
            }
            if (data.WaveElements != null)
            {
                info.Value = BitmapHelper.CreateRenderInfo(bitmap, data.Colors[WaveFormStreamPositionConfiguration.COLOR_PALETTE_VALUE]);
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

            if (data.WaveElements != null)
            {
                BitmapHelper.DrawRectangles(ref info.Value, data.WaveElements, data.WaveElements.Length);
            }
            if (data.PowerElements != null)
            {
                BitmapHelper.DrawRectangles(ref info.Rms, data.PowerElements, data.PowerElements.Length);
            }
        }

        public static WaveFormRendererData Create(WaveFormGenerator.WaveFormGeneratorData generatorData, int width, int height, bool rms, WaveFormRendererMode mode, IDictionary<string, IntPtr> colors)
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
                Position = 0,
                Capacity = width,
                Peak = 0
            };
            switch (mode)
            {
                default:
                case WaveFormRendererMode.Mono:
                    data.WaveElements = new Int32Rect[1, width];
                    if (rms)
                    {
                        data.PowerElements = new Int32Rect[1, width];
                    }
                    data.Channels = 1;
                    break;
                case WaveFormRendererMode.Seperate:
                    data.WaveElements = new Int32Rect[generatorData.Channels, width];
                    if (rms)
                    {
                        data.PowerElements = new Int32Rect[generatorData.Channels, width];
                    }
                    data.Channels = generatorData.Channels;
                    break;
            }
            return data;
        }

        public class WaveFormRendererData
        {
            public int Width;

            public int Height;

            public int ValuesPerElement;

            public IDictionary<string, IntPtr> Colors;

            public Int32Rect[,] WaveElements;

            public Int32Rect[,] PowerElements;

            public int Channels;

            public int Position;

            public int Capacity;

            public float Peak;

            public float NormalizedPeak;
        }
    }

    public enum WaveFormRendererMode : byte
    {
        None,
        Mono,
        Seperate
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WaveFormRenderInfo
    {
        public BitmapHelper.RenderInfo Rms;

        public BitmapHelper.RenderInfo Value;

        public BitmapHelper.RenderInfo Background;
    }
}
