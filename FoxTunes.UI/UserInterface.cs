using System;
using FoxTunes.Interfaces;

namespace FoxTunes
{
    public abstract class UserInterface : StandardComponent, IUserInterface
    {
        public bool RestartPending { get; protected set; }

        public abstract void Show();

        public abstract void Run(string message);

        public abstract void Warn(string message);

        public abstract void Fatal(Exception exception);

        public abstract void Restart();
    }
}
