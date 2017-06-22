using FoxTunes.Interfaces;

namespace FoxTunes
{
    public abstract class Output : StandardComponent, IOutput
    {
        public abstract bool IsSupported(string fileName);

        public abstract IOutputStream Load(string fileName);

        public abstract void Unload(IOutputStream stream);
    }
}
