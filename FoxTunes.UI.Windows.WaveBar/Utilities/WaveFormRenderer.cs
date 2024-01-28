using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;
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

        public IConfiguration Configuration { get; private set; }

        public SelectionConfigurationElement Mode { get; private set; }

        public IntegerConfigurationElement Resolution { get; private set; }

        public BooleanConfigurationElement Rms { get; private set; }

        public TextConfigurationElement ColorPalette { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Generator = ComponentRegistry.Instance.GetComponent<WaveFormGenerator>();
            this.PlaybackManager = core.Managers.Playback;
            this.PlaybackManager.CurrentStreamChanged += this.OnCurrentStreamChanged;
            this.Configuration = core.Components.Configuration;
            this.Mode = this.Configuration.GetElement<SelectionConfigurationElement>(
                WaveBarBehaviourConfiguration.SECTION,
                WaveBarBehaviourConfiguration.MODE_ELEMENT
            );
            this.Resolution = this.Configuration.GetElement<IntegerConfigurationElement>(
                WaveBarBehaviourConfiguration.SECTION,
                WaveBarBehaviourConfiguration.RESOLUTION_ELEMENT
            );
            this.Rms = this.Configuration.GetElement<BooleanConfigurationElement>(
                WaveBarBehaviourConfiguration.SECTION,
                WaveBarBehaviourConfiguration.RMS_ELEMENT
            );
            this.ColorPalette = this.Configuration.GetElement<TextConfigurationElement>(
                WaveBarBehaviourConfiguration.SECTION,
                WaveBarBehaviourConfiguration.COLOR_PALETTE_ELEMENT
            );
            this.Mode.ValueChanged += this.OnValueChanged;
            this.Resolution.ValueChanged += this.OnValueChanged;
            this.Rms.ValueChanged += this.OnValueChanged;
            this.ColorPalette.ValueChanged += this.OnValueChanged;
            base.InitializeComponent(core);
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
            var generatorData = this.GeneratorData;
            if (generatorData == null)
            {
                return false;
            }
            var mode = WaveBarBehaviourConfiguration.GetMode(this.Mode.Value);
            var colors = WaveBarBehaviourConfiguration.GetColorPalette(this.ColorPalette.Value, this.Color);
            switch (mode)
            {
                default:
                case WaveFormRendererMode.Mono:
                    //Nothing to do.
                    break;
                case WaveFormRendererMode.Seperate:
                    if (colors.Length > 1)
                    {
                        colors = colors.DuplicateGradient(generatorData.Channels);
                    }
                    break;
            }
            this.RendererData = Create(
                generatorData,
                width,
                height,
                this.Rms.Value,
                mode,
                colors
            );
            this.Dispatch(this.Update);
            return true;
        }

        public async Task Render(WaveFormRendererData data)
        {
            const byte SHADE = 30;

            var bitmap = default(WriteableBitmap);
            var success = default(bool);
            var waveRenderInfo = default(BitmapHelper.RenderInfo);
            var powerRenderInfo = default(BitmapHelper.RenderInfo);

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
                if (data.PowerElements != null)
                {
                    var colors = this.Color.ToPair(SHADE);
                    waveRenderInfo = BitmapHelper.CreateRenderInfo(bitmap, BitmapHelper.GetOrCreatePalette(0, colors[0]));
                    powerRenderInfo = BitmapHelper.CreateRenderInfo(bitmap, BitmapHelper.GetOrCreatePalette(0, colors[1]));
                }
                else
                {
                    waveRenderInfo = BitmapHelper.CreateRenderInfo(bitmap, BitmapHelper.GetOrCreatePalette(BitmapHelper.COLOR_FROM_Y, data.Colors));
                }
            }).ConfigureAwait(false);

            if (!success)
            {
                //No bitmap or failed to establish lock.
                return;
            }
            try
            {
                Render(
                    data,
                    waveRenderInfo,
                    powerRenderInfo
                );
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

        public static void Render(WaveFormRendererData rendererData, BitmapHelper.RenderInfo waveRenderInfo, BitmapHelper.RenderInfo powerRenderInfo)
        {
            //TODO: We should only clear and re-render when the peak changes.
            BitmapHelper.Clear(ref waveRenderInfo);

            if (rendererData.Capacity == 0)
            {
                //No data.
                return;
            }

            if (rendererData.Width != waveRenderInfo.Width || rendererData.Height != waveRenderInfo.Height)
            {
                //Bitmap does not match data.
                return;
            }

            BitmapHelper.DrawRectangles(ref waveRenderInfo, rendererData.WaveElements, rendererData.WaveElements.Length);
            if (rendererData.PowerElements != null)
            {
                BitmapHelper.DrawRectangles(ref powerRenderInfo, rendererData.PowerElements, rendererData.PowerElements.Length);
            }
        }

        public static WaveFormRendererData Create(WaveFormGenerator.WaveFormGeneratorData generatorData, int width, int height, bool rms, WaveFormRendererMode mode, Color[] colors)
        {
            var valuesPerElement = generatorData.Capacity / width;
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

            public Color[] Colors;

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
}
