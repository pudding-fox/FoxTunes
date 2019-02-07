namespace FoxTunes.Interfaces
{
    public interface IComponentResolver
    {
        string Get(string slot);

        void Add(string slot);
    }
}
