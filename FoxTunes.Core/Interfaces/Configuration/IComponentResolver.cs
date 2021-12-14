namespace FoxTunes.Interfaces
{
    public interface IComponentResolver
    {
        string Get(string slot);

        bool Resolve(string slot);
    }
}
