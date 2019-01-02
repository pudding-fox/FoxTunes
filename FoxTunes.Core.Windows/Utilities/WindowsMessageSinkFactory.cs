using FoxTunes.Interfaces;

namespace FoxTunes
{
    public class WindowsMessageSinkFactory : MessageSinkFactory, IStandardFactory
    {
        public override IMessageSink Create(uint id)
        {
            return new WindowsMessageSink(id);
        }
    }
}
