using System.Collections.ObjectModel;

namespace FoxTunes.Interfaces
{
    public interface IConfiguration : IStandardComponent
    {
        ObservableCollection<ConfigurationSection> Sections { get; }

        void RegisterSection(ConfigurationSection section);

        void Load();

        void Save();

        ConfigurationSection GetSection(string sectionId);

        ConfigurationElement GetElement(string sectionId, string elementId);

        T GetElement<T>(string sectionId, string elementId) where T : ConfigurationElement;
    }
}
