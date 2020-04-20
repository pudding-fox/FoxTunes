using System.Collections.Generic;

namespace FoxTunes.Interfaces
{
    public interface IOutputEqualizer : IOutputEffect
    {
        IEnumerable<IOutputEqualizerBand> Bands { get; }

        IEnumerable<string> Presets { get; }

        void LoadPreset(string name);
    }
}
