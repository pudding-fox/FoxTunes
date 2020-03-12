using System;
using FoxTunes.Interfaces;

namespace FoxTunes
{
    public abstract class UserInterface : StandardComponent, IUserInterface
    {
        public abstract void Show();

        public abstract void Activate();

        public abstract void Run(string message);

        public abstract void Warn(string message);

        public abstract void Fatal(Exception exception);

        public abstract void Restart();

        protected virtual void OnWindowCreated(IntPtr handle)
        {
            if (this.WindowCreated == null)
            {
                return;
            }
            this.WindowCreated(this, new UserInterfaceWindowCreatedEvent(handle));
        }

        public event UserInterfaceWindowCreatedEventHandler WindowCreated;
    }
}
