namespace FoxTunes.Interfaces
{
    public interface ISignalEmitter : IStandardComponent
    {
        void Send(ISignal signal);

        event SignalEventHandler Signal;
    }

    public delegate void SignalEventHandler(object sender, ISignal signal);
}
