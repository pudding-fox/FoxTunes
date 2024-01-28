using FoxTunes.Interfaces;
using System.Collections.Generic;

namespace FoxTunes
{
    public interface IBassEncoderSettings
    {
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

        IEnumerable<ConfigurationElement> GetConfigurationElements();
    }

    public enum BassEncoderOutputDestination : byte
    {
        None = 0,
        Source = 1,
        Specific = 2
    }
}
