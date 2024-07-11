using System.Collections.Generic;

namespace FoxTunes.Interfaces
{
    public interface IPlaylistColumnManager : IStandardComponent
    {
        IEnumerable<IPlaylistColumnProvider> Providers { get; }

        IPlaylistColumnProvider GetProvider(string plugin);
    }
}
