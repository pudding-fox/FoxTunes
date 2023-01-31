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
                }
                else
                {
                    data.Data = new float[1, data.Samples32.Length];
                }
                data.OnAllocated();
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
                }
                else
                {
                    data.Samples = this.OutputDataSource.GetBuffer(data.FFTSize, false);
                    data.Data = new float[1, data.Samples.Length];
                }
                data.OnAllocated();
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
                }
                else
                {
                    data.SampleCount = DownmixMono(data.Data, data.Samples, 1, data.Samples.Length);
                }
                return true;
            }
            return false;
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
