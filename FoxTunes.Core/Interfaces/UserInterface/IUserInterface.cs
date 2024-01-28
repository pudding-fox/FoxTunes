using System;

namespace FoxTunes.Interfaces
{
    public interface IUserInterface : IStandardComponent
    {
        void Show();

        void Run(string message);

        void Fatal(Exception exception);
    }
}
