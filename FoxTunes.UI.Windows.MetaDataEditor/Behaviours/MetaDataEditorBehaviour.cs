using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class MetaDataEditorBehaviour : StandardBehaviour, IInvocableComponent
    {
        public const string EDIT_METADATA = "LLLL";

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
                yield return new InvocationComponent(InvocationComponent.CATEGORY_PLAYLIST, EDIT_METADATA, "Tag");
            }
        }

        public Task InvokeAsync(IInvocationComponent component)
        {
            switch (component.Id)
            {
                case EDIT_METADATA:
                    return this.SignalEmitter.Send(new Signal(this, CommonSignals.PluginInvocation, component));
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }
    }
}
