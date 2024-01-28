using System.Collections.ObjectModel;

namespace FoxTunes.Interfaces
{
    public interface IConfiguration : IStandardComponent
    {
        ReleaseType ReleaseType { get; }

        ObservableCollection<ConfigurationSection> Sections { get; }

        void RegisterSection(ConfigurationSection section);

        void Load();

        void Save();

        void Reset();

        void ConnectDependencies();

        ConfigurationSection GetSection(string sectionId);

        ConfigurationElement GetElement(string sectionId, string elementId);

        T GetElement<T>(string sectionId, string elementId) where T : ConfigurationElement;

        string SaveValue<T>(T value);

        T LoadValue<T>(string value);
    }

    public enum ReleaseType : byte
    {
        Default = 0,
        Minimal = 1
    }
}
