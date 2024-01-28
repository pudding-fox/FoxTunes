using CSCore;

namespace FoxTunes
{
    public abstract class WaveSourceFactory : BaseComponent, IWaveSourceFactory
    {
        public abstract bool IsSupported(string fileName);

        public abstract IWaveSource CreateWaveSource(string fileName);
    }
}
