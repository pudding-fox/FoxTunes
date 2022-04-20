namespace FoxTunes
{
    public interface IBassEncoderFormat
    {
        bool AutoDepth { get; }

        int Depth { get; }

        int[] SampleRates { get; }
    }
}
