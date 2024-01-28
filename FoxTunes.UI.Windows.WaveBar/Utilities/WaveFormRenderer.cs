using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FoxTunes
{
    public class WaveFormRenderer : BaseComponent
    {
        public static readonly Duration LockTimeout = new Duration(TimeSpan.FromMilliseconds(1));

        public WaveFormGenerator.WaveFormGeneratorData GeneratorData;

        public WaveFormRendererData RendererData;

        public WriteableBitmap Bitmap;

        public Color Color;

        public IOutput Output;

        public IPlaybackManager PlaybackManager;

        public int Resolution;

        public WaveFormRendererMode Mode;

        public WaveFormRenderer(int resolution)
        {
            this.Resolution = resolution;
        }

        public override void InitializeComponent(ICore core)
        {
            this.Output = core.Components.Output;
            this.PlaybackManager = core.Managers.Playback;
            this.PlaybackManager.CurrentStreamChanged += this.OnCurrentStreamChanged;
            if (this.PlaybackManager.CurrentStream != null)
            {
                this.Dispatch(() => this.Update(this.PlaybackManager.CurrentStream));
            }
            base.InitializeComponent(core);
        }

        protected virtual void OnCurrentStreamChanged(object sender, EventArgs e)
        {
            this.Dispatch(() => this.Update(this.PlaybackManager.CurrentStream));
        }

        protected virtual async Task Update(IOutputStream stream)
        {
            var cached = default(bool);
            var generatorData = this.GeneratorData;
            var rendererData = this.RendererData;

            if (generatorData != null)
            {
                generatorData.Updated -= this.OnUpdated;
                if (generatorData.CancellationToken != null)
                {
                    generatorData.CancellationToken.Cancel();
                }
            }

            await this.Clear().ConfigureAwait(false);

            if (stream == null)
            {
                return;
            }

            if (WaveFormCache.TryLoad(stream, this.Resolution, out generatorData))
            {
                cached = true;
            }
            else
            {
                stream = await this.Output.Duplicate(stream).ConfigureAwait(false);

                generatorData = WaveFormGenerator.Create(stream, this.Resolution);
                generatorData.Updated += this.OnUpdated;
            }

            var bitmap = this.Bitmap;
            if (bitmap != null)
            {
                await Windows.Invoke(() =>
                {
                    var info = BitmapHelper.CreateRenderInfo(bitmap, this.Color);
                    rendererData = Create(generatorData, info, this.Mode);
                }).ConfigureAwait(false);
            }

            this.GeneratorData = generatorData;
            this.RendererData = rendererData;

            if (cached)
            {
                var task = this.Update();
                return;
            }

            try
            {
                WaveFormGenerator.Populate(stream, generatorData);
            }
            finally
            {
                stream.Dispose();
            }
        }

        protected virtual void OnUpdated(object sender, EventArgs e)
        {
            var task = this.Update();
        }

        public WriteableBitmap CreateBitmap(int width, int height, Color color, WaveFormRendererMode mode)
        {
            var generatorData = this.GeneratorData;
            var bitmap = new WriteableBitmap(
               width,
               height,
               96,
               96,
               PixelFormats.Pbgra32,
               null
            );

            if (generatorData != null)
            {
                var info = BitmapHelper.CreateRenderInfo(bitmap, color);
                this.RendererData = Create(generatorData, info, mode);
            }

            this.Bitmap = bitmap;
            this.Color = color;
            this.Mode = mode;

            return bitmap;
        }

        public async Task Render()
        {
            var success = default(bool);
            var info = default(BitmapHelper.RenderInfo);
            var data = this.RendererData;
            var bitmap = this.Bitmap;

            if (data == null || bitmap == null)
            {
                return;
            }

            await Windows.Invoke(() =>
            {
                success = bitmap.TryLock(LockTimeout);
                if (!success)
                {
                    return;
                }
                info = BitmapHelper.CreateRenderInfo(bitmap, this.Color);
            }).ConfigureAwait(false);

            if (!success)
            {
                //Failed to establish lock.
                return;
            }

            try
            {
                if (data.Available < data.Position)
                {
                    BitmapHelper.Clear(info);
                }

                for (; data.Position < data.Available; data.Position++)
                {
                    BitmapHelper.DrawRectangle(
                        info,
                        data.Elements[data.Position].X,
                        data.Elements[data.Position].Y,
                        data.Elements[data.Position].Width,
                        data.Elements[data.Position].Height
                    );
                }

                await Windows.Invoke(() =>
                {
                    bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
                    bitmap.Unlock();
                }).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Logger.Write(this.GetType(), LogLevel.Warn, "Failed to paint wave form, disabling: {0}", e.Message);
            }
        }

        public async Task Clear()
        {
            var success = default(bool);
            var info = default(BitmapHelper.RenderInfo);
            var bitmap = this.Bitmap;

            await Windows.Invoke(() =>
            {
                success = bitmap.TryLock(LockTimeout);
                if (!success)
                {
                    return;
                }
                info = BitmapHelper.CreateRenderInfo(bitmap, this.Color);
            }).ConfigureAwait(false);

            if (!success)
            {
                //Failed to establish lock.
                return;
            }

            try
            {
                BitmapHelper.Clear(info);

                await Windows.Invoke(() =>
                {
                    bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
                    bitmap.Unlock();
                }).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Logger.Write(this.GetType(), LogLevel.Warn, "Failed to clear wave form: {0}", e.Message);
            }
        }

        public Task Update()
        {
            var generatorData = this.GeneratorData;
            var rendererData = this.RendererData;

            if (generatorData != null && rendererData != null)
            {
                Update(generatorData, rendererData);
            }

            return this.Render();
        }

        private static void Update(WaveFormGenerator.WaveFormGeneratorData generatorData, WaveFormRendererData rendererData)
        {
            if (generatorData.Peak == 0)
            {
                return;
            }
            else if (generatorData.Peak != rendererData.Peak)
            {
                rendererData.Available = 0;
                rendererData.Peak = generatorData.Peak;
            }

            switch (rendererData.Mode)
            {
                case WaveFormRendererMode.Mono:
                    UpdateMono(generatorData, rendererData);
                    break;
                case WaveFormRendererMode.Seperate:
                    UpdateSeperate(generatorData, rendererData);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private static void UpdateMono(WaveFormGenerator.WaveFormGeneratorData generatorData, WaveFormRendererData rendererData)
        {
            var center = rendererData.Height / 2;
            var factor = rendererData.Peak / 2;

            while (rendererData.Available < rendererData.Capacity)
            {
                var valuePosition = rendererData.Available * rendererData.ValuesPerElement;
                if (valuePosition > generatorData.Position)
                {
                    break;
                }

                var x = rendererData.Available;
                var y = default(int);
                var width = 1;
                var height = default(int);

                var topValue = default(float);
                var bottomValue = default(float);
                for (var a = 0; a < rendererData.ValuesPerElement; a++)
                {
                    for (var b = 0; b < generatorData.Channels; b++)
                    {
                        topValue += Math.Abs(generatorData.Data[valuePosition + a, b].Min);
                        bottomValue += Math.Abs(generatorData.Data[valuePosition + a, b].Max);
                    }
                }
                topValue /= (rendererData.ValuesPerElement * generatorData.Channels);
                bottomValue /= (rendererData.ValuesPerElement * generatorData.Channels);

                topValue /= factor;
                bottomValue /= factor;

                topValue = Math.Min(topValue, 1);
                bottomValue = Math.Min(bottomValue, 1);

                y = Convert.ToInt32(center - (topValue * center));
                height = Convert.ToInt32((center - y) + (bottomValue * center));

                rendererData.Elements[rendererData.Available].X = x;
                rendererData.Elements[rendererData.Available].Y = y;
                rendererData.Elements[rendererData.Available].Width = width;
                rendererData.Elements[rendererData.Available].Height = height;

                rendererData.Available++;
            }
        }

        private static void UpdateSeperate(WaveFormGenerator.WaveFormGeneratorData generatorData, WaveFormRendererData rendererData)
        {
            var factor = rendererData.Peak / (generatorData.Channels * 2);

            while (rendererData.Available < rendererData.Capacity)
            {
                var valuePosition = (rendererData.Available / generatorData.Channels) * rendererData.ValuesPerElement;
                if (valuePosition > generatorData.Position)
                {
                    break;
                }

                var x = rendererData.Available / generatorData.Channels;
                var waveHeight = rendererData.Height / generatorData.Channels;

                for (var channel = 0; channel < generatorData.Channels; channel++)
                {
                    var waveCenter = (waveHeight * channel) + (waveHeight / 2);

                    var y = default(int);
                    var width = 1;
                    var height = default(int);

                    var topValue = default(float);
                    var bottomValue = default(float);
                    for (var a = 0; a < rendererData.ValuesPerElement; a++)
                    {
                        topValue += Math.Abs(generatorData.Data[valuePosition + a, channel].Min);
                        bottomValue += Math.Abs(generatorData.Data[valuePosition + a, channel].Max);
                    }
                    topValue /= (rendererData.ValuesPerElement * generatorData.Channels);
                    bottomValue /= (rendererData.ValuesPerElement * generatorData.Channels);

                    topValue /= factor;
                    bottomValue /= factor;

                    topValue = Math.Min(topValue, 1);
                    bottomValue = Math.Min(bottomValue, 1);

                    y = Convert.ToInt32(waveCenter - (topValue * (waveHeight / 2)));
                    height = Convert.ToInt32((waveCenter - y) + (bottomValue * (waveHeight / 2)));

                    rendererData.Elements[rendererData.Available].X = x;
                    rendererData.Elements[rendererData.Available].Y = y;
                    rendererData.Elements[rendererData.Available].Width = width;
                    rendererData.Elements[rendererData.Available].Height = height;

                    rendererData.Available++;
                }
            }
        }


        public static WaveFormRendererData Create(WaveFormGenerator.WaveFormGeneratorData generatorData, BitmapHelper.RenderInfo renderInfo, WaveFormRendererMode mode)
        {
            var channels = default(int);
            if (mode == WaveFormRendererMode.Seperate)
            {
                channels = generatorData.Channels;
            }
            else
            {
                channels = 1;
            }

            return new WaveFormRendererData()
            {
                Width = renderInfo.Width,
                Height = renderInfo.Height,
                ValuesPerElement = Math.Max(generatorData.Capacity / renderInfo.Width, 1),
                Elements = new Int32Rect[renderInfo.Width * channels],
                Position = 0,
                Available = 0,
                Capacity = renderInfo.Width * channels,
                Peak = 0,
                Mode = mode
            };
        }

        public class WaveFormRendererData
        {
            public int Width;

            public int Height;

            public int ValuesPerElement;

            public Int32Rect[] Elements;

            public int Position;

            public int Available;

            public int Capacity;

            public float Peak;

            public WaveFormRendererMode Mode;
        }
    }

    public enum WaveFormRendererMode : byte
    {
        None,
        Mono,
        Seperate
    }
}
