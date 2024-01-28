using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class ImageBehaviour : StandardBehaviour, IInvocableComponent
    {
        public const string REFRESH_IMAGES = "ZAAA";

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
                yield return new InvocationComponent(InvocationComponent.CATEGORY_LIBRARY, REFRESH_IMAGES, "Refresh Images", path: "Library");
            }
        }

        public Task InvokeAsync(IInvocationComponent component)
        {
            switch (component.Id)
            {
                case REFRESH_IMAGES:
                    return this.RefreshImages();
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        private Task RefreshImages()
        {
            return this.SignalEmitter.Send(new Signal(this, CommonSignals.PluginInvocation, REFRESH_IMAGES));
        }
    }
}
