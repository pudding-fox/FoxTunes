namespace FoxTunes.Interfaces
{
    public interface ICore
    {
        IStandardComponents Components { get; }

        IStandardManagers Managers { get; }

        void LoadComponents();

        void LoadManagers();

        void LoadBehaviours();

        void InitializeComponents();
    }
}
