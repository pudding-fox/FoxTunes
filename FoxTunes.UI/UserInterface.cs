using System;
using System.Threading.Tasks;
using FoxTunes.Interfaces;

namespace FoxTunes
{
    public abstract class UserInterface : StandardComponent, IUserInterface
    {
        public abstract Task Show();

        public abstract void Activate();

        public abstract void Run(string message);

        public abstract void Warn(string message);

        public abstract void Fatal(Exception exception);

        public abstract bool Confirm(string message);

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
