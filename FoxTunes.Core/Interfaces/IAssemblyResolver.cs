namespace FoxTunes.Interfaces
{
    public interface IAssemblyResolver
    {
        void Enable();

        void Disable();

        string Resolve(string name);
    }
}
