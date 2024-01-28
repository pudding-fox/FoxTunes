namespace FoxTunes.Interfaces
{
    public interface ICore
    {
        IStandardComponents Components { get; }

        IStandardManagers Managers { get; }

        IStandardFactories Factories { get; }

        void Load();
    }
}
