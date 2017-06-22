using FoxTunes.Interfaces;

namespace FoxTunes
{
    public abstract class UserInterface : StandardComponent, IUserInterface
    {
        public abstract void Show();
    }
}
