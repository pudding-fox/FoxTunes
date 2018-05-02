using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class PlaylistActionsBehaviour : StandardBehaviour, IInvocableComponent
    {
        public const string LOCATE_PLAYLIST_ITEM = "0ECC7DA5-EC67-4EB2-B435-FFF44D9DCF55";

        public ISignalEmitter SignalEmitter { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.SignalEmitter = core.Components.SignalEmitter;
            base.InitializeComponent(core);
        }

        public IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                yield return new InvocationComponent(InvocationComponent.CATEGORY_PLAYLIST, LOCATE_PLAYLIST_ITEM, "Locate");
            }
        }

        public Task InvokeAsync(IInvocationComponent component)
        {
            switch (component.Id)
            {
                case LOCATE_PLAYLIST_ITEM:
                    return this.SignalEmitter.Send(new Signal(this, CommonSignals.PluginInvocation, LOCATE_PLAYLIST_ITEM));
            }
            return Task.CompletedTask;
        }
    }
}
