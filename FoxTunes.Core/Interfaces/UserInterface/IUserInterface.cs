namespace FoxTunes.Interfaces
{
    public interface IUserInterface : IStandardComponent
    {
        void Show();

        void Run(string message);
    }
}
