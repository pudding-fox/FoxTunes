namespace FoxTunes.Interfaces
{
    public interface IAsyncResult<T>
    {
        T Value { get; }
    }
}
