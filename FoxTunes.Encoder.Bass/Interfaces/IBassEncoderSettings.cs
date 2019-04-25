using System.Collections.Generic;

namespace FoxTunes
{
    public interface IBassEncoderSettings
    {
        string Executable { get; }

        string Directory { get; }

        IBassEncoderFormat Format { get; }

        string GetOutput(string fileName);

        string GetArguments(int rate, int channels, long length);

        IEnumerable<ConfigurationElement> GetConfigurationElements();
    }
}
