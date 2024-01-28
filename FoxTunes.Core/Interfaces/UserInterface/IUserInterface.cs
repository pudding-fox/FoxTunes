using System;

namespace FoxTunes.Interfaces
{
    public interface IUserInterface : IStandardComponent
    {
        void Show();

        void Run(string message);

        void Warn(string message);

        void Fatal(Exception exception);

        void Restart();

        event UserInterfaceWindowCreatedEventHandler WindowCreated;
    }

    public delegate void UserInterfaceWindowCreatedEventHandler(object sender, UserInterfaceWindowCreatedEvent e);

    public class UserInterfaceWindowCreatedEvent : EventArgs
    {
        public UserInterfaceWindowCreatedEvent(IntPtr handle)
        {
            this.Handle = handle;
        }

        public IntPtr Handle { get; private set; }
    }
}
