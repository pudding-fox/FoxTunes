using FoxTunes.Interfaces;
using System;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.Output)]
    public class VisualizationDataSource : StandardComponent, IVisualizationDataSource
    {
        public IOutputDataSource OutputDataSource { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.OutputDataSource = core.Components.OutputDataSource;
            base.InitializeComponent(core);
        }

        public bool Update(PCMVisualizationData data)
        {
            var rate = default(int);
            var channels = default(int);
            var format = default(OutputStreamFormat);
            if (!this.OutputDataSource.GetDataFormat(out rate, out channels, out format))
            {
                return false;
            }
            if (this.Update(data, rate, channels, format))
            {
                switch (format)
                {
                    case OutputStreamFormat.Short:
                        data.Samples16 = this.OutputDataSource.GetBuffer<short>(data.Interval);
                        data.Samples32 = new float[data.Samples16.Length];
                        break;
                    case OutputStreamFormat.Float:
                        data.Samples32 = this.OutputDataSource.GetBuffer<float>(data.Interval);
                        break;
                    default:
                        throw new NotImplementedException();
                }
                if (data.Flags.HasFlag(VisualizationDataFlags.Individual))
                {
                    data.Data = new float[data.Channels, data.Samples32.Length];
                    data.Peak = new float[data.Channels];
                }
                else
                {
                    data.Data = new float[1, data.Samples32.Length];
                    data.Peak = new float[1];
                }
                data.OnAllocated();
                if (data.History != null && data.History.Capacity > 0)
                {
                    if (data.Flags.HasFlag(VisualizationDataFlags.Individual))
                    {
                        data.History.Values = new float[data.Channels, data.Samples32.Length, data.History.Capacity];
                        data.History.Avg = new float[data.Channels, data.Samples32.Length];
                        data.History.Peak = new float[data.Channels, data.Samples32.Length];
                        data.History.Rms = new float[data.Channels, data.Samples32.Length];
                    }
                    else
                    {
                        data.History.Values = new float[1, data.Samples32.Length, data.History.Capacity];
                        data.History.Avg = new float[1, data.Samples32.Length];
                        data.History.Peak = new float[1, data.Samples32.Length];
                        data.History.Rms = new float[1, data.Samples32.Length];
                    }
                    data.History.Position = 0;
                    data.History.Count = 0;
                    data.History.OnAllocated();
                }
            }
            switch (data.Format)
            {
                case OutputStreamFormat.Short:
                    data.SampleCount = this.OutputDataSource.GetData(data.Samples16);
                    for (var a = 0; a < data.SampleCount; a++)
                    {
                        data.Samples32[a] = (float)data.Samples16[a] / short.MaxValue;
                    }
                    break;
                case OutputStreamFormat.Float:
                    data.SampleCount = this.OutputDataSource.GetData(data.Samples32);
                    break;
                default:
                    throw new NotImplementedException();
            }
            if (data.Rate > 0 && data.Channels > 0 && data.SampleCount > 0)
            {
                if (data.Flags.HasFlag(VisualizationDataFlags.Individual))
                {
                    data.SampleCount = Deinterlace(data.Data, data.Samples32, data.Channels, data.SampleCount);
                }
                else
                {
                    data.SampleCount = DownmixMono(data.Data, data.Samples32, data.Channels, data.SampleCount);
                }
                if (data.History != null && data.History.Capacity > 0)
                {
                    if (data.Flags.HasFlag(VisualizationDataFlags.Individual))
                    {
                        UpdateHistorySeperate(data, data.SampleCount);
                    }
                    else
                    {
                        UpdateHistoryMono(data, data.SampleCount);
                    }
                }
                return true;
            }
            return false;
        }


        public bool Update(FFTVisualizationData data)
        {
            var rate = default(int);
            var channels = default(int);
            var format = default(OutputStreamFormat);
            if (!this.OutputDataSource.GetDataFormat(out rate, out channels, out format))
            {
                return false;
            }
            if (this.Update(data, rate, channels, format))
            {
                if (data.Flags.HasFlag(VisualizationDataFlags.Individual))
                {
                    data.Samples = this.OutputDataSource.GetBuffer(data.FFTSize, true);
                    data.Data = new float[data.Channels, data.Samples.Length];
                    data.Peak = new float[data.Channels];
                }
                else
                {
                    data.Samples = this.OutputDataSource.GetBuffer(data.FFTSize, false);
                    data.Data = new float[1, data.Samples.Length];
                    data.Peak = new float[1];
                }
                data.OnAllocated();
                if (data.History != null && data.History.Capacity > 0)
                {
                    if (data.Flags.HasFlag(VisualizationDataFlags.Individual))
                    {
                        data.History.Values = new float[data.Channels, data.Samples.Length, data.History.Capacity];
                        data.History.Avg = new float[data.Channels, data.Samples.Length];
                        data.History.Peak = new float[data.Channels, data.Samples.Length];
                        data.History.Rms = new float[data.Channels, data.Samples.Length];
                    }
                    else
                    {
                        data.History.Values = new float[1, data.Samples.Length, data.History.Capacity];
                        data.History.Avg = new float[1, data.Samples.Length];
                        data.History.Peak = new float[1, data.Samples.Length];
                        data.History.Rms = new float[1, data.Samples.Length];
                    }
                    data.History.Position = 0;
                    data.History.Count = 0;
                    data.History.OnAllocated();
                }
            }
            if (data.Flags.HasFlag(VisualizationDataFlags.Individual))
            {
                data.SampleCount = this.OutputDataSource.GetData(data.Samples, data.FFTSize, true);
            }
            else
            {
                data.SampleCount = this.OutputDataSource.GetData(data.Samples, data.FFTSize);
            }
            if (data.Rate > 0 && data.Channels > 0 && data.SampleCount > 0)
            {
                if (data.Flags.HasFlag(VisualizationDataFlags.Individual))
                {
                    data.SampleCount = Deinterlace(data.Data, data.Samples, data.Channels, data.Samples.Length);
                    UpdatePeaksSeperate(data);
                }
                else
                {
                    data.SampleCount = DownmixMono(data.Data, data.Samples, 1, data.Samples.Length);
                    UpdatePeaksMono(data);
                }
                if (data.History != null && data.History.Capacity > 0)
                {
                    if (data.Flags.HasFlag(VisualizationDataFlags.Individual))
                    {
                        UpdateHistorySeperate(data, data.SampleCount);
                    }
                    else
                    {
                        UpdateHistoryMono(data, data.SampleCount);
                    }
                }
                return true;
            }
            return false;
        }

        protected virtual void UpdatePeaksMono(FFTVisualizationData data)
        {
            data.Peak[0] = 0.0f;
            for (var a = 0; a < data.Samples.Length; a++)
            {
                data.Peak[0] = Math.Max(data.Peak[0], data.Data[0, a]);
            }
        }

        protected virtual void UpdatePeaksSeperate(FFTVisualizationData data)
        {
            for (var channel = 0; channel < data.Channels; channel++)
            {
                data.Peak[channel] = 0.0f;
                for (var a = 0; a < data.Samples.Length; a++)
                {
                    data.Peak[channel] = Math.Max(data.Peak[channel], data.Data[channel, a]);
                }
            }
        }

        protected virtual void UpdateHistoryMono(VisualizationData data, int count)
        {
            for (var a = 0; a < count; a++)
            {
                data.History.Values[0, a, data.History.Position] = data.Data[0, a];
            }
            for (var a = 0; a < count; a++)
            {
                data.History.Avg[0, a] = 0.0f;
                data.History.Peak[0, a] = 0.0f;
                data.History.Rms[0, a] = 0.0f;
            }
            for (var a = 0; a < count; a++)
            {
                for (var b = 0; b < data.History.Count; b++)
                {
                    var value = data.History.Values[0, a, (b + data.History.Position) % data.History.Count];
                    data.History.Avg[0, a] += value;
                    if (data.History.Peak[0, a] < 0)
                    {
                        if (data.History.Peak[0, a] > value)
                        {
                            data.History.Peak[0, a] = value;
                        }
                    }
                    else if (data.History.Peak[0, a] > 0)
                    {
                        if (data.History.Peak[0, a] < value)
                        {
                            data.History.Peak[0, a] = value;
                        }
                    }
                    else
                    {
                        data.History.Peak[0, a] = value;
                    }
                    data.History.Rms[0, a] += value * value;
                }
                data.History.Avg[0, a] = data.History.Avg[0, a] / data.History.Count;
                data.History.Rms[0, a] = Convert.ToSingle(Math.Sqrt(data.History.Rms[0, a] / data.History.Count));
            }
            if (data.History.Position < data.History.Capacity - 1)
            {
                data.History.Position++;
            }
            else
            {
                data.History.Position = 0;
            }
            if (data.History.Count < data.History.Capacity)
            {
                data.History.Count++;
            }
        }

        protected virtual void UpdateHistorySeperate(VisualizationData data, int count)
        {
            for (var channel = 0; channel < data.Channels; channel++)
            {
                for (var a = 0; a < count; a++)
                {
                    data.History.Values[channel, a, data.History.Position] = data.Data[channel, a];
                }
            }
            for (var channel = 0; channel < data.Channels; channel++)
            {
                for (var a = 0; a < count; a++)
                {
                    data.History.Avg[channel, a] = 0.0f;
                    data.History.Peak[channel, a] = 0.0f;
                    data.History.Rms[channel, a] = 0.0f;
                }
            }
            for (var channel = 0; channel < data.Channels; channel++)
            {
                for (var a = 0; a < count; a++)
                {
                    for (var b = 0; b < data.History.Count; b++)
                    {
                        var value = data.History.Values[channel, a, (b + data.History.Position) % data.History.Count];
                        data.History.Avg[channel, a] += value;
                        if (data.History.Peak[channel, a] < 0)
                        {
                            if (data.History.Peak[channel, a] > value)
                            {
                                data.History.Peak[channel, a] = value;
                            }
                        }
                        else if (data.History.Peak[channel, a] > 0)
                        {
                            if (data.History.Peak[channel, a] < value)
                            {
                                data.History.Peak[channel, a] = value;
                            }
                        }
                        else
                        {
                            data.History.Peak[channel, a] = value;
                        }
                        data.History.Rms[channel, a] += value * value;
                    }
                    data.History.Avg[channel, a] = data.History.Avg[channel, a] / data.History.Count;
                    data.History.Rms[channel, a] = Convert.ToSingle(Math.Sqrt(data.History.Rms[channel, a] / data.History.Count));
                }
            }
            if (data.History.Position < data.History.Capacity - 1)
            {
                data.History.Position++;
            }
            else
            {
                data.History.Position = 0;
            }
            if (data.History.Count < data.History.Capacity)
            {
                data.History.Count++;
            }
        }

        protected virtual bool Update(VisualizationData data, int rate, int channels, OutputStreamFormat format)
        {
            if (data.Rate == rate && data.Channels == channels && data.Format == format && data.Initialized)
            {
                return false;
            }

            data.Rate = rate;
            data.Channels = channels;
            data.Format = format;
            data.Initialized = true;
            return true;
        }

        public static int Deinterlace(float[,] destination, float[] source, int channels, int count)
        {
            for (int a = 0, b = 0; a < count; a += channels, b++)
            {
                for (var channel = 0; channel < channels; channel++)
                {
                    destination[channel, b] = source[a + channel];
                }
            }
            return count / channels;
        }

        public static int DownmixMono(float[,] destination, float[] source, int channels, int count)
        {
            if (channels > 1)
            {
                for (int a = 0, b = 0; a < count; a += channels, b++)
                {
                    destination[0, b] = 0f;
                    for (var channel = 0; channel < channels; channel++)
                    {
                        destination[0, b] += source[a + channel];
                    }
                    destination[0, b] /= channels;
                }
                return count / channels;
            }
            else
            {
                for (int a = 0; a < count; a++)
                {
                    destination[0, a] = source[a];
                }
                return count;
            }
        }
    }
}
