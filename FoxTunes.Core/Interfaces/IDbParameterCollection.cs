namespace FoxTunes.Interfaces
{
    public interface IDbParameterCollection
    {
        int Count { get; }

        bool Contains(string name);

        object this[string name] { get; set; }
    }
}
