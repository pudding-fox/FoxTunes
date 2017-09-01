using FoxTunes.Interfaces;

namespace FoxTunes
{
    public class SignalEmitter : StandardComponent, ISignalEmitter
    {
        public void Send(ISignal signal)
        {
            this.OnSignal(signal);
        }

        protected virtual void OnSignal(ISignal signal)
        {
            if (this.Signal == null)
            {
                return;
            }
            this.Signal(this, signal);
        }

        public event SignalEventHandler Signal = delegate { };
    }
}
