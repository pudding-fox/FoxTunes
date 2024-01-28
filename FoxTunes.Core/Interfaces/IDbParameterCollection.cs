namespace FoxTunes.Interfaces
{
    public interface IDbParameterCollection
    {
        object this[string parameterName] { get; set; }
    }
}
