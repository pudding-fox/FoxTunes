using FoxTunes.Interfaces;
using System;

namespace FoxTunes
{
    public static class WaveFormGenerator
    {
        private static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        public static WaveFormGeneratorData Create(IOutputStream stream, int resolution)
        {
            var length = Convert.ToInt32(Math.Ceiling(
                stream.GetDuration(stream.Length).TotalMilliseconds / resolution
            ));
            return new WaveFormGeneratorData()
            {
                Resolution = resolution,
                Data = new WaveFormDataElement[length, stream.Channels],
                Position = 0,
                Capacity = length,
                Peak = 0,
                Channels = stream.Channels,
                CancellationToken = new CancellationToken(),
            };
        }

        public static void Populate(IOutputStream stream, WaveFormGeneratorData data)
        {
            switch (stream.Format)
            {
                case OutputStreamFormat.Short:
                    PopulateShort(stream, data);
                    break;
                case OutputStreamFormat.Float:
                    PopulateFloat(stream, data);
                    break;
                default:
                    throw new NotImplementedException();
            }

            if (data.CancellationToken.IsCancellationRequested)
            {
                return;
            }

            try
            {
                WaveFormCache.Save(stream, data);
            }
            catch (Exception e)
            {
                Logger.Write(typeof(WaveFormGenerator), LogLevel.Warn, "Failed to save wave form data for file \"{0}\": {1}", stream.FileName, e.Message);
            }
        }

        private static void PopulateShort(IOutputStream stream, WaveFormGeneratorData data)
        {
            var duration = TimeSpan.FromMilliseconds(data.Resolution);
            var buffer = stream.GetBuffer<short>(duration);
            var interval = data.Capacity / 10;
            do
            {
#if DEBUG
                if (data.Position >= data.Capacity)
                {
                    //TODO: Why?
                    break;
                }
#endif

                var length = stream.GetData(buffer) / sizeof(short);
                if (length <= 0)
                {
                    break;
                }

                for (var a = 0; a < length; a += data.Channels)
                {
                    for (var b = 0; b < data.Channels; b++)
                    {
                        var value = (float)buffer[a + b] / short.MaxValue;
                        data.Data[data.Position, b].Min = Math.Min(data.Data[data.Position, b].Min, value);
                        data.Data[data.Position, b].Max = Math.Max(data.Data[data.Position, b].Max, value);
                        data.Data[data.Position, b].Rms += value * value;
                    }
                }

                for (var a = 0; a < data.Channels; a++)
                {
                    data.Data[data.Position, a].Rms = Convert.ToSingle(
                        Math.Sqrt(data.Data[data.Position, a].Rms / (length / data.Channels))
                    );

                    data.Peak = Math.Max(
                        data.Peak,
                        Math.Max(
                            Math.Abs(data.Data[data.Position, a].Min),
                            Math.Abs(data.Data[data.Position, a].Max)
                        )
                    );
                }

                data.Position++;

                if (data.Position % interval == 0)
                {
                    data.Update();
                }

            } while (!data.CancellationToken.IsCancellationRequested);

            data.Update();
        }

        private static void PopulateFloat(IOutputStream stream, WaveFormGeneratorData data)
        {
            var duration = TimeSpan.FromMilliseconds(data.Resolution);
            var buffer = stream.GetBuffer<float>(duration);
            var interval = data.Capacity / 10;
            do
            {
#if DEBUG
                if (data.Position >= data.Capacity)
                {
                    //TODO: Why?
                    break;
                }
#endif

                var length = stream.GetData(buffer) / sizeof(float);
                if (length <= 0)
                {
                    break;
                }

                for (var a = 0; a < length; a += data.Channels)
                {
                    for (var b = 0; b < data.Channels; b++)
                    {
                        var value = buffer[a + b];
                        data.Data[data.Position, b].Min = Math.Min(data.Data[data.Position, b].Min, value);
                        data.Data[data.Position, b].Max = Math.Max(data.Data[data.Position, b].Max, value);
                        data.Data[data.Position, b].Rms += value * value;
                    }
                }

                for (var a = 0; a < data.Channels; a++)
                {
                    data.Data[data.Position, a].Rms = Convert.ToSingle(
                        Math.Sqrt(data.Data[data.Position, a].Rms / (length / data.Channels))
                    );

                    data.Peak = Math.Max(
                        data.Peak,
                        Math.Max(
                            Math.Abs(data.Data[data.Position, a].Min),
                            Math.Abs(data.Data[data.Position, a].Max)
                        )
                    );
                }

                data.Position++;

                if (data.Position % interval == 0)
                {
                    data.Update();
                }

            } while (!data.CancellationToken.IsCancellationRequested);

            data.Update();
        }

        [Serializable]
        public class WaveFormGeneratorData
        {
            public int Resolution;

            public WaveFormDataElement[,] Data;

            public int Channels;

            public int Position;

            public int Capacity;

            public float Peak;

            public void Update()
            {
                if (this.Updated == null)
                {
                    return;
                }
                this.Updated(this, EventArgs.Empty);
            }

            [field: NonSerialized]
            public event EventHandler Updated;

            [field: NonSerialized]
            public CancellationToken CancellationToken;
        }

        [Serializable]
        public struct WaveFormDataElement
        {
            public float Min;

            public float Max;

            public float Rms;
        }
    }
}
