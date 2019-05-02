using FoxTunes.Interfaces;

namespace FoxTunes
{
    public interface IBassEncoderSettings
    {
        string Name { get; }

        string Executable { get; }

        string Directory { get; }

        string Extension { get; }

        IBassEncoderFormat Format { get; }

        BassEncoderOutputDestination Destination { get; }

        string Location { get; }

        bool CopyTags { get; }

        int Threads { get; }

        void InitializeComponent(ICore core);

        string GetOutput(string fileName);

        long GetLength(EncoderItem encoderItem, IBassStream stream);

        int GetDepth(EncoderItem encoderItem, IBassStream stream);

        string GetArguments(EncoderItem encoderItem, IBassStream stream);
    }

    public enum BassEncoderOutputDestination : byte
    {
        None = 0,
        Source = 1,
        Specific = 2
    }
}
