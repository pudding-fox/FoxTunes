using System;

namespace FoxTunes.Interfaces
{
    public interface IUserInterface : IStandardComponent
    {
        bool RestartPending { get; }

        void Show();

        void Run(string message);

        void Warn(string message);

        void Fatal(Exception exception);

        void Restart();
    }
}
