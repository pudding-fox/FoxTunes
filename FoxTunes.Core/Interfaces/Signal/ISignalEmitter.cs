using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface ISignalEmitter : IStandardComponent
    {
        Task Send(ISignal signal);

        event SignalEventHandler Signal;
    }

    public delegate Task SignalEventHandler(object sender, ISignal signal);
}
