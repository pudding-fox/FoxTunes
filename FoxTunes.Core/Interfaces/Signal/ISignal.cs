namespace FoxTunes.Interfaces
{
    public interface ISignal
    {
        object Source { get; }

        string Name { get; }

        object State { get; }
    }
}
