using FoxTunes.Interfaces;
using System.Threading.Tasks;

namespace FoxTunes
{
    public abstract class Output : StandardComponent, IOutput
    {
        public abstract bool IsSupported(string fileName);

        public abstract Task<IOutputStream> Load(PlaylistItem playlistItem);

        public abstract Task Preempt(IOutputStream stream);

        public abstract Task Unload(IOutputStream stream);
    }
}
