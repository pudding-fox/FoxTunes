using FoxTunes.Interfaces;
using System.Collections.Generic;

namespace FoxTunes
{
    public abstract class BassStreamAdvice : BaseComponent, IBassStreamAdvice
    {
        protected BassStreamAdvice(string fileName)
        {
            this.FileName = fileName;
        }

        public string FileName { get; private set; }

        public abstract bool Wrap(IBassStreamProvider provider, int channelHandle, IEnumerable<IBassStreamAdvice> advice, out IBassStream stream);
    }
}
