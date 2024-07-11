using FoxTunes.Interfaces;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.Output)]
    public class FFTDataTransformerFactory : StandardComponent, IFFTDataTransformerFactory
    {
        public IFFTDataTransformer Create(int[] bands)
        {
            return new FFTDataTransformer(bands);
        }
    }
}
