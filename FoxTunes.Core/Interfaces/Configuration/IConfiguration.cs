using System.Collections.ObjectModel;

namespace FoxTunes.Interfaces
{
    public interface IConfiguration : IStandardComponent
    {
        ObservableCollection<ConfigurationSection> Sections { get; }

        void RegisterSection(ConfigurationSection section);

        void Load();

        void Save();
    }
}
