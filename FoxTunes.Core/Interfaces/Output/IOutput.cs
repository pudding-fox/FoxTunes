using System;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IOutput : IStandardComponent
    {
        bool IsStarted { get; }

        event EventHandler IsStartedChanged;

        bool IsSupported(string fileName);

        Task<IOutputStream> Load(PlaylistItem playlistItem);

        Task Preempt(IOutputStream stream);

        Task Unload(IOutputStream stream);

        Task Shutdown();
    }
}
