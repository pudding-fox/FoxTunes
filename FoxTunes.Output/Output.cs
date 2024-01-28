using FoxTunes.Interfaces;
using System.Threading.Tasks;

namespace FoxTunes
{
    public abstract class Output : StandardComponent, IOutput
    {
        public abstract bool IsSupported(string fileName);

        public abstract Task<IOutputStream> Load(string fileName);

        public abstract Task Unload(IOutputStream stream);
    }
}
