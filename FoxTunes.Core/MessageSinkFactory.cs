using FoxTunes.Interfaces;

namespace FoxTunes
{
    public abstract class MessageSinkFactory : BaseComponent, IMessageSinkFactory
    {
        public abstract IMessageSink Create(uint id);
    }
}
