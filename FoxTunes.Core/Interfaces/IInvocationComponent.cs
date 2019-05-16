namespace FoxTunes.Interfaces
{
    public interface IInvocationComponent
    {
        string Category { get; }

        string Id { get; }

        string Name { get; }

        string Description { get; }

        string Path { get; }

        byte Attributes { get; }
    }
}
