using System;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IOutput : IStandardComponent
    {
        string Name { get; }

        string Description { get; }

        bool IsStarted { get; }

        event EventHandler IsStartedChanged;

        bool IsSupported(string fileName);

        Task<IOutputStream> Load(PlaylistItem playlistItem, bool immidiate);

        Task<bool> Preempt(IOutputStream stream);

        Task Unload(IOutputStream stream);

        Task Shutdown();
    }
}
