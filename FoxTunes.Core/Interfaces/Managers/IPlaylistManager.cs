using System.Collections.Generic;

namespace FoxTunes.Interfaces
{
    public interface IPlaylistManager : IStandardManager
    {
        void AddDirectory(string directoryName);

        void AddFile(string fileName);

        void Next();

        void Previous();

        IEnumerable<IPlaylistItem> Items { get; }

        void Save();

        void Load();
    }
}
