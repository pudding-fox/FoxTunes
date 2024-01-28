using System;

namespace FoxTunes.Interfaces
{
    public interface IVisualizationDataSource : IStandardComponent
    {
        bool Update(PCMVisualizationData data);

        bool Update(FFTVisualizationData data);
    }

    public abstract class VisualizationData : BaseComponent
    {
        public int Rate;

        public int Channels;

        public OutputStreamFormat Format;

        public bool Initialized;

        public VisualizationDataFlags Flags;

        public virtual void OnAllocated()
        {
            //Nothing to do.
        }
    }

    [Flags]
    public enum VisualizationDataFlags : byte
    {
        None = 0,
        Individual = 1
    }

    public class PCMVisualizationData : VisualizationData
    {
        public TimeSpan Interval;

        public short[] Samples16;

        public float[] Samples32;

        public int SampleCount;

        public float[,] Data;
    }

    public class FFTVisualizationData : VisualizationData
    {
        public int FFTSize;

        public float[] Samples;

        public int SampleCount;

        public float[,] Data;
    }
}
