using FoxTunes.Interfaces;

namespace FoxTunes
{
    public abstract class BassStreamAdvice : BaseComponent, IBassStreamAdvice
    {
        protected BassStreamAdvice(string fileName)
        {
            this.FileName = fileName;
        }

        public string FileName { get; private set; }

        public abstract bool Wrap(IBassStreamProvider provider, int channelHandle, out IBassStream stream);
    }
}
