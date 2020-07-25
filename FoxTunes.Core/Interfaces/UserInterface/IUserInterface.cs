using System;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IUserInterface : IStandardComponent
    {
        Task Show();

        void Activate();

        void Run(string message);

        void Warn(string message);

        void Fatal(Exception exception);

        bool Confirm(string message);

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
