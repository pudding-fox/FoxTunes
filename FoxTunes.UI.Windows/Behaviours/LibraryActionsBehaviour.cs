using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class LibraryActionsBehaviour : StandardBehaviour, IInvocableComponent
    {
        public const string APPEND_PLAYLIST = "AAAB";

        public const string REPLACE_PLAYLIST = "AAAC";

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
                yield return new InvocationComponent(InvocationComponent.CATEGORY_LIBRARY, APPEND_PLAYLIST, "Add To Playlist");
                yield return new InvocationComponent(InvocationComponent.CATEGORY_LIBRARY, REPLACE_PLAYLIST, "Replace Playlist");
            }
        }

        public Task InvokeAsync(IInvocationComponent component)
        {
            switch (component.Id)
            {
                case APPEND_PLAYLIST:
                case REPLACE_PLAYLIST:
                    return this.SignalEmitter.Send(new Signal(this, CommonSignals.PluginInvocation, component));
            }
            return Task.CompletedTask;
        }
    }
}
