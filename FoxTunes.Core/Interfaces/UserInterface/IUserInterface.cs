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

        event UserInterfaceWindowEventHandler WindowCreated;

        event UserInterfaceWindowEventHandler WindowDestroyed;
    }

    public delegate void UserInterfaceWindowEventHandler(object sender, UserInterfaceWindowEventArgs e);

    public class UserInterfaceWindowEventArgs : EventArgs
    {
        public UserInterfaceWindowEventArgs(IntPtr handle, UserInterfaceWindowRole role)
        {
            this.Handle = handle;
            this.Role = role;
        }

        public IntPtr Handle { get; private set; }

        public UserInterfaceWindowRole Role { get; private set; }
    }

    public enum UserInterfaceWindowRole : byte
    {
        None,
        Main
    }
}
