using FoxTunes.Interfaces;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class SignalEmitter : StandardComponent, ISignalEmitter
    {
        public Task Send(ISignal signal)
        {
            return this.OnSignal(signal);
        }

        protected virtual Task OnSignal(ISignal signal)
        {
            if (this.Signal == null)
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            return this.Signal(this, signal);
        }

        public event SignalEventHandler Signal;
    }
}
