namespace FoxTunes.Interfaces
{
    public interface IMessageSinkFactory : IBaseComponent
    {
        IMessageSink Create(uint id);
    }
}
